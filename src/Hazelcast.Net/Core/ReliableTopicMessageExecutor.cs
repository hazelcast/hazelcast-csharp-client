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
    private long _sequence = long.MinValue;
    private ReliableTopicEventHandlerOptions _options;
    private Cluster _cluster;
    private SerializationService _serializationService;

    private ReliableTopicMessageExecutor(IHRingBuffer<ReliableTopicMessage> ringBuffer,
        Action<ReliableTopicEventHandler<TItem>> events,
        object stateObject,
        IHReliableTopic<TItem> topic,
        ReliableTopicEventHandlerOptions options,
        int batchSize,
        Cluster cluster,
        SerializationService serializationService,
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
        _options = options;
        _handlers = new ReliableTopicEventHandler<TItem>();
        _stateObject = stateObject;
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
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        return new ReliableTopicMessageExecutor<TItem>(ringBuffer, action, stateObject, topic, options, batchSize, cluster, serializationService, loggerFactory, cancellationToken);
    }

    private async Task Execute(CancellationToken cancellationToken)
    {
        await SetInitialSequenceOnceAsync().CfAwait();

        _logger.IfDebug().LogDebug("Reading messages from ring buffer");

        var result = await _ringBuffer.ReadManyWithResultSetAsync(_sequence, 1, _batchSize).CfAwait();

        long lostCount = result.NextSequence - result.Count - _sequence;

        if (lostCount != 0 && !_options.IsLossTolerant)
        {
            //todo: cancel the thread
            return;
        }

        for (var i = 0; i < result.Count; i++)
        {
            try
            {
                var message = result[i];
                var member = _cluster.Members.GetMembers().FirstOrDefault(m => m.ConnectAddress.Equals(message.PublisherAddress));
                var payloadObject = await PayloadToObjectAsync(message.Payload).CfAwait();
                var seq = result.GetSequence(i);

                foreach (var handler in _handlers)
                {
                    await handler.HandleAsync(_topic, member, message.PublishTime, payloadObject, seq, _stateObject).CfAwait();
                }
            }
            catch (Exception ex)
            {
                _logger.IfDebug().LogDebug("Something went wrong while handling the event", ex);

                if (ShouldDispose(ex))
                    await DisposeAsync().CfAwait();

                // todo: fetch next batch
            }
        }
    }

    private bool ShouldDispose(Exception exception)
    {
        if (exception.GetType() == typeof(ClientOfflineException))
        {
            _logger.IfDebug().LogDebug("Operation is terminating since client is offline", exception);
            return true;
        }
        else if (exception.GetType() == typeof(ArgumentOutOfRangeException) && _options.IsLossTolerant)
        {
            _logger.IfDebug().LogDebug("ReliableTopicMessageExecutor on topic {TopicName} requested a too large sequence: {Sequence} "
                                       + ". Jumping from old sequence: {Sequence} to head sequence", _ringBuffer.Name, exception);
            return false;
        }

        else if(exception.GetType() == typeof(ArgumentOutOfRangeException))
        

        return true;
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

        if (_sequence != long.MinValue) return;

        var seq = _options.InitialSequence;

        if (seq == -1)
        {
            seq = await _ringBuffer.GetTailSequenceAsync().CfAwait() + 1;
        }

        _sequence = seq;
    }

    public ValueTask DisposeAsync()
    {
        // dispose task
        return default;
    }
}
