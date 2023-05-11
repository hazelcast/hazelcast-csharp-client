// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
    private CancellationToken _cancellationToken;
    private IHRingBuffer<ReliableTopicMessage> _ringBuffer;
    private Action<ReliableTopicEventHandler<TItem>> _events;
    private readonly object _stateObject;
    private ReliableTopicEventHandler<TItem> _handlers;
    private IHReliableTopic<TItem> _topic;
    private int _disposed;
    private ILogger _logger;
    private int _batchSize;
    private long _sequence;
    private ReliableTopicEventHandlerOptions _options;
    private Cluster _cluster;
    private SerializationService _serializationService;
    private Guid _id;

    private ReliableTopicMessageExecutor(IHRingBuffer<ReliableTopicMessage> ringBuffer,
        Action<ReliableTopicEventHandler<TItem>> events,
        object stateObject,
        IHReliableTopic<TItem> topic,
        ReliableTopicEventHandlerOptions options,
        int batchSize,
        Cluster cluster,
        SerializationService serializationService,
        Guid id,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _ringBuffer = ringBuffer;
        _events = events;
        _topic = topic;
        _logger = loggerFactory.CreateLogger<ReliableTopicMessageExecutor<TItem>>();
        _batchSize = batchSize;
        _cluster = cluster;
        _serializationService = serializationService;
        _options = options ?? new ReliableTopicEventHandlerOptions();
        _handlers = new ReliableTopicEventHandler<TItem>();
        _stateObject = stateObject;
        _id = id;
        _events(_handlers);
        _backgroundTask = BackgroundTask.Run(token => Execute(cancellationToken));
    }

    public static ReliableTopicMessageExecutor<TItem> New(IHRingBuffer<ReliableTopicMessage> ringBuffer,
        Action<ReliableTopicEventHandler<TItem>> action,
        object stateObject,
        IHReliableTopic<TItem> topic,
        ReliableTopicEventHandlerOptions options,
        int batchSize,
        Cluster cluster,
        SerializationService serializationService,
        Guid id,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
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
            cancellationToken);
    }

    private async Task Execute(CancellationToken cancellationToken)
    {
        await SetInitialSequenceOnceAsync().CfAwait();

        while (true)
        {
            _logger.IfDebug()?.LogDebug("Reading messages from ring buffer {Name}. ", _ringBuffer.Name);

            var result = await _ringBuffer.ReadManyWithResultSetAsync(_sequence, 1, _batchSize).CfAwait();

            var lostCount = result.NextSequence - result.Count - _sequence;

            if (lostCount != 0 && !_options.IsLossTolerant)
            {
                _logger.IfDebug()?.LogDebug("The reliable topic subscription is not loss tolerant. Disposing the process...");
                await DisposeAsync().CfAwait();
                return;
            }

            for (var i = 0; i < result.Count; i++)
            {
                if (await DisposeIfCanceled(cancellationToken).CfAwait()) return;

                try
                {
                    var message = result[i];
                    var member = _cluster.Members
                        .GetMembers()
                        .FirstOrDefault(m => m.ConnectAddress.Equals(message.PublisherAddress));

                    var payloadObject = await PayloadToObjectAsync(message.Payload).CfAwait();
                    var seq = result.GetSequence(i);

                    foreach (var handler in _handlers)
                    {
                        if (await DisposeIfCanceled(cancellationToken).CfAwait()) return;

                        await handler
                            .HandleAsync(_topic, member, message.PublishTime, payloadObject, seq, _stateObject)
                            .CfAwait();
                    }

                    _sequence = result.NextSequence;
                }
                catch (Exception ex)
                {
                    _logger.IfDebug()?.LogError("Something went wrong while handling the event. ", ex);

                    if (ex.GetType() == typeof(ClientOfflineException))
                    {
                        _logger.IfDebug()?.LogError(ex, "Operation is terminating since client is offline. ");
                        await DisposeAsync().CfAwait();
                        return;
                    }

                    if (ex.GetType() == typeof(ArgumentOutOfRangeException) && _options.IsLossTolerant)
                    {
                        var old = _sequence;
                        _sequence = await _ringBuffer.GetTailSequenceAsync().CfAwait() + 1;

                        _logger.IfDebug()?.LogDebug("The reliable topic subscription on topic {TopicName} requested a too large sequence {Old}"
                                                    + ". Jumping from old {Old} sequence to head sequence {Head}.", _ringBuffer.Name, old, old, _sequence);
                    }

                    if (ex.GetType() == typeof(ArgumentOutOfRangeException) && !_options.IsLossTolerant)
                    {
                        // nothing to do, it's not loss tolerant.
                        _logger.IfWarning().LogWarning("Terminating the reliable topic subscription {ID} on topic {Name} due to " +
                                                       "underlying ring buffer data related to reliable topic is lost. ", _id, _topic.Name);
                        await DisposeAsync().CfAwait();
                        return;
                    }

                    throw;
                }
            }
        }
    }

    private async Task<bool> DisposeIfCanceled(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.IfDebug()?.LogDebug("Process canceled. Disposing the process... ");
            await DisposeAsync().CfAwait();
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
            await _backgroundTask.CompletedOrCancelAsync(true).CfAwait();
        }
    }
}
