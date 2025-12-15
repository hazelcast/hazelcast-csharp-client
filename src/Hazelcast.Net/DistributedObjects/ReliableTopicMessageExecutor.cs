// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.DistributedObjects;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Core;

/// <summary>
/// The class fetches the messages from a underlying ring buffer of the reliable topic as batches, and
/// invokes the registered events until disposed.
/// </summary>
/// <typeparam name="TItem"></typeparam>
internal class ReliableTopicMessageExecutor<TItem> : IAsyncDisposable
{
    private BackgroundTask _backgroundTask;
    private IHRingBuffer<ReliableTopicMessage> _ringBuffer;
    private Action<ReliableTopicEventHandlers<TItem>> _events;
    private readonly object _stateObject;
    private ReliableTopicEventHandlers<TItem> _eventHandlers;
    private IHReliableTopic<TItem> _topic;
    private int _disposed;
    private ILogger _logger;
    private int _batchSize;
    private long _sequence;
    private ReliableTopicEventHandlerOptions _options;
    private Cluster _cluster;
    private SerializationService _serializationService;
    private Guid _id;
    private Action<Guid> _onDisposed;

    private ReliableTopicMessageExecutor(IHRingBuffer<ReliableTopicMessage> ringBuffer,
        Action<ReliableTopicEventHandlers<TItem>> events,
        object stateObject,
        IHReliableTopic<TItem> topic,
        ReliableTopicEventHandlerOptions options,
        int batchSize,
        Cluster cluster,
        SerializationService serializationService,
        Guid id,
        ILoggerFactory loggerFactory,
        Action<Guid> onDisposed)
    {
        _ringBuffer = ringBuffer;
        _events = events;
        _topic = topic;
        _logger = loggerFactory.CreateLogger<ReliableTopicMessageExecutor<TItem>>();
        _batchSize = batchSize;
        _cluster = cluster;
        _serializationService = serializationService;
        _options = options ?? new ReliableTopicEventHandlerOptions();
        _eventHandlers = new ReliableTopicEventHandlers<TItem>();
        _stateObject = stateObject;
        _id = id;
        _onDisposed = onDisposed;
        _events(_eventHandlers);
        _backgroundTask = BackgroundTask.Run(ExecuteAsync);
    }

    public static ReliableTopicMessageExecutor<TItem> StartNew(IHRingBuffer<ReliableTopicMessage> ringBuffer,
        Action<ReliableTopicEventHandlers<TItem>> action,
        object stateObject,
        IHReliableTopic<TItem> topic,
        ReliableTopicEventHandlerOptions options,
        int batchSize,
        Cluster cluster,
        SerializationService serializationService,
        Guid id,
        ILoggerFactory loggerFactory,
        Action<Guid> onDisposed)
    {
        return new ReliableTopicMessageExecutor<TItem>(ringBuffer,
            action,
            stateObject,
            topic,
            options,
            batchSize,
            cluster,
            serializationService,
            id,
            loggerFactory,
            onDisposed);
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SetInitialSequenceOnceAsync().CfAwait();

        var messageHandlers = _eventHandlers
            .OfType<IReliableTopicMessageEventHandler<TItem>>()
            .ToArray();

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.IfDebug()?.LogDebug("Reading messages from ring buffer {Name}. ", _ringBuffer.Name);
            try
            {
                var result = await _ringBuffer.ReadManyWithResultSetAsync(_sequence, 1, _batchSize, cancellationToken).CfAwait();

                var lostCount = result.NextSequence - result.Count - _sequence;

                if (lostCount != 0 && !_options.IsLossTolerant)
                {
                    _logger.IfWarning()?.LogWarning("The reliable topic subscription [{Id}] is not loss tolerant. {Count} message(s) is lost. Terminating the subscription...", _id, (-1 * lostCount));
                    await DisposeAsync().CfAwaitNoThrow();
                    return;
                }

                for (var i = 0; i < result.Count; i++)
                {
                    if (await DisposeIfCanceledAsync(cancellationToken).CfAwait()) return;

                    #region Handle Result

                    try
                    {
                        var message = result[i];
                        var member = _cluster.Members
                            .GetMembers()
                            ?.FirstOrDefault(m =>
                                m.ConnectAddress?.IPEndPoint.Equals(message.PublisherAddress?.IpEndPoint) == true);

                        var payloadObject = await PayloadToObjectAsync(message.Payload).CfAwait();
                        var seq = result.GetSequence(i);

                        foreach (var handler in messageHandlers)
                        {
                            if (await DisposeIfCanceledAsync(cancellationToken).CfAwait()) return;

                            if (_options.StoreSequence) _sequence = seq;

                            await handler
                                .HandleAsync(_topic, member, message.PublishTime, payloadObject, seq,  _stateObject)
                                .CfAwait();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.IfDebug()?.LogError(ex, "Something went wrong while handling the event. ");
                        
                        var exceptionHandler = _eventHandlers
                            .OfType<IReliableTopicExceptionEventHandler<TItem>>();

                        var args = new ReliableTopicExceptionEventArgs(ex, _sequence, _stateObject);
                        foreach (var handler in exceptionHandler)
                        {
                            await handler.HandleAsync(_topic, args).CfAwaitNoThrow();
                        }

                        if (cancellationToken.IsCancellationRequested || !args.Cancel)
                        {
                            _logger.IfWarning()?.LogWarning("Exception {Exception} caused to terminate the reliable topic subscription [{Id}]. ", ex, _id);
                            await DisposeAsync().CfAwaitNoThrow();
                            return;
                        }
                    }

                    #endregion
                }

                _sequence = result.NextSequence;
            }
            catch (OperationCanceledException ex) 
            {
                // The cancelled exception is expected when cancellation or dispose is requested on subscription.
                _logger
                    .IfDebug()?
                    .LogDebug("Reliable topic subscription [{Id}] is canceled during reading from ring buffer {RingBuffer}." +
                              " Terminating the subscription... {Exception}", _id, _ringBuffer.Name, ex);
                await DisposeAsync().CfAwait();
                return;
            }
            catch (Exception ex)
            {
                // Can't throw at the background.
                _logger.IfWarning()?.LogWarning(ex, "Something went wrong while reading result from {RingBuffer}. ", _ringBuffer.Name);

                if (cancellationToken.IsCancellationRequested || await ShouldDisposeAsync(ex).CfAwait())
                {
                    _logger.IfWarning()?.LogWarning("Exception {Exception} caused to terminate the reliable topic subscription [{Id}]. ", ex, _id);
                    await DisposeAsync().CfAwaitNoThrow();
                    return;
                }
            }
        }
    }

    private async Task<bool> ShouldDisposeAsync(Exception ex)
    {
        if (ex.GetType() == typeof(ClientOfflineException) || ex.GetType() == typeof(ConnectionException))
        {
            _logger.IfWarning().LogWarning(ex, "Reliable topic message execution with subscription {[Id]} is waiting client to be connected. ", _id);
            return false;
        }
        else if (ex.GetType() == typeof(ArgumentOutOfRangeException) && _options.IsLossTolerant)
        {
            var old = _sequence;
            _sequence = await _ringBuffer.GetTailSequenceAsync().CfAwait() + 1;

            _logger.IfWarning()?.LogWarning("The reliable topic subscription on topic {TopicName} requested a too large sequence {Old}"
                                            + ". Jumping from old {Old} sequence to head sequence {Head}.", _ringBuffer.Name, old, old, _sequence);
            return false;
        }
        else if (ex.GetType() == typeof(ArgumentOutOfRangeException) && !_options.IsLossTolerant)
        {
            // nothing to do, it's not loss tolerant.
            _logger.IfWarning().LogWarning("Terminating the reliable topic subscription {ID} on topic {Name} due to " +
                                           "underlying ring buffer data related to reliable topic is lost. ", _id, _topic.Name);
            return true;
        }

        return true;
    }

    private async Task<bool> DisposeIfCanceledAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.IfDebug()?.LogDebug("Reliable topic subscription [{Id}] is canceled. Terminating the subscription... ", _id);
            await DisposeAsync().CfAwaitNoThrow();
            return true;
        }

        return false;
    }

    private ValueTask<TItem> PayloadToObjectAsync(IData messagePayload)
    {
        return _serializationService.TryToObject<TItem>(messagePayload, out var obj, out var state)
            ? new ValueTask<TItem>(obj)
            : _serializationService.ToObjectAsync<TItem>(messagePayload, state);
    }

    private async Task SetInitialSequenceOnceAsync()
    {
        // Constructor would be better place to determine the initial sequence
        // but we live in async world.

        var seq = _options.InitialSequence;

        if (seq == -1)
        {
            seq = await _ringBuffer.GetTailSequenceAsync().CfAwait() + 1;
        }

        _sequence = seq;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed.InterlockedZeroToOne())
        {
            _logger.IfDebug()?.LogDebug("Disposing the reliable topic subscription [{Id}]. ", _id);

            await _backgroundTask.CompletedOrCancelAsync(true).CfAwaitNoThrow();
            _onDisposed?.Invoke(_id);

            var terminatedHandler = _eventHandlers
                .OfType<ReliableTopicTerminatedEventHandler<TItem>>();

            // Handle Terminated event.
            foreach (var handler in terminatedHandler)
            {
                await handler.HandleAsync(default, _sequence,  _stateObject).CfAwaitNoThrow();
            }
        }
    }

    public bool IsDisposed => _disposed > 0;
}
