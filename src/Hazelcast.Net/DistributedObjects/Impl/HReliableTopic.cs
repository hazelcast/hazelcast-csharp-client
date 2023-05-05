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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl;

internal class HReliableTopic<TItem> : DistributedObjectBase, IHReliableTopic<TItem>
{
    private IHRingBuffer<ReliableTopicMessage> _ringBuffer;
    private int _backOffMax = 2000;
    private int _backOffInitial = 100;
    private SerializationService _serializationService;
    private ILogger _logger;
    private ReliableTopicOptions _options;
    private string _name;
    private int _disposed;
    private ConcurrentDictionary<Guid, Task> _executors = new();

    public HReliableTopic(string serviceName, string name, DistributedObjectFactory factory, ReliableTopicOptions options, Cluster cluster, SerializationService serializationService, IHRingBuffer<ReliableTopicMessage> ringBuffer, ILoggerFactory loggerFactory)
        : base(serviceName, name, factory, cluster, serializationService, loggerFactory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _name = string.IsNullOrEmpty(name) ? throw new ArgumentNullException(nameof(name)) : name;
        _logger = loggerFactory.CreateLogger<HReliableTopic<TItem>>();
        _serializationService = SerializationService ?? throw new ArgumentNullException(nameof(serializationService));
        _ringBuffer = ringBuffer ?? throw new ArgumentNullException(nameof(ringBuffer));
    }

    /// <inheritdoc />
    public Task<Guid> SubscribeAsync(Action<ReliableTopicEventHandler<TItem>> events, object state = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask<bool> UnsubscribeAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task PublishAsync(TItem message, CancellationToken cancellationToken = default)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        try
        {
            var data = _serializationService.ToData(message);
            var rtMessage = new ReliableTopicMessage(data, null);

            _logger.IfDebug().LogDebug("Adding message with policy {OptionsPolicy}", _options.Policy);

            return _options.Policy switch
            {
                TopicOverloadPolicy.DiscardOldest => _ringBuffer.AddAsync(rtMessage, OverflowPolicy.Overwrite),
                TopicOverloadPolicy.DiscardNewest => _ringBuffer.AddAsync(rtMessage, OverflowPolicy.Fail),
                TopicOverloadPolicy.Block => AddAsBlockingAsync(rtMessage, cancellationToken),
                TopicOverloadPolicy.Error => AddOrFail(rtMessage),
                _ => throw new ArgumentOutOfRangeException(nameof(_options.Policy))
            };
        }
        catch (Exception e)
        {
            _logger.IfDebug().LogError("Failed while publishing a message {Message} on topic {Name}", message, Name, e);
            throw;
        }
    }

    private async Task AddOrFail(ReliableTopicMessage rtMessage)
    {
        var result = await _ringBuffer.AddAsync(rtMessage, OverflowPolicy.Fail).CfAwait();

        _logger.IfDebug().LogError("Failed to publish a message [{Message}] on topic [{Name}]", rtMessage, Name);

        if (result == -1)
            throw new TopicOverloadException($"Failed to publish a message [{rtMessage}] on topic [{Name}].");
    }

    private async Task AddAsBlockingAsync(ReliableTopicMessage rtMessage, CancellationToken cancellationToken = default)
    {
        var wait = _backOffInitial;

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await _ringBuffer.AddAsync(rtMessage, OverflowPolicy.Fail).CfAwait();

            if (result != -1) break;

            _logger.IfDebug().LogDebug("Waiting to publish message with {Duration}ms back off", wait);
            await Task.Delay(wait, cancellationToken).CfAwait();

            wait *= 2;

            if (wait > _backOffMax) wait = _backOffMax;
        }

        if (cancellationToken.IsCancellationRequested)
            _logger.IfDebug().LogDebug("Publishing process is canceled. {Message}", rtMessage);
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed.InterlockedZeroToOne())
        {
            // todo dispose tasks.
        }

        return default;
    }

    public ValueTask DestroyAsync()
    {
        return _ringBuffer.DestroyAsync();
    }
}
