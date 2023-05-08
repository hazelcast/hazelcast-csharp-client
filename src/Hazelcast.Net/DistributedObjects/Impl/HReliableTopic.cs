﻿// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Protocol.Codecs;
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
    private ILoggerFactory _loggerFactory;
    private ReliableTopicOptions _options;
    private Cluster _cluster;
    private string _name;
    private int _disposed;
    private ConcurrentDictionary<Guid, ReliableTopicMessageExecutor<TItem>> _executors = new();

    public HReliableTopic(string serviceName, string name, DistributedObjectFactory factory, ReliableTopicOptions options, Cluster cluster, SerializationService serializationService, IHRingBuffer<ReliableTopicMessage> ringBuffer, ILoggerFactory loggerFactory)
        : base(serviceName, name, factory, cluster, serializationService, loggerFactory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _name = string.IsNullOrEmpty(name) ? throw new ArgumentNullException(nameof(name)) : name;
        _loggerFactory = loggerFactory;
        _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
        _logger = loggerFactory.CreateLogger<HReliableTopic<TItem>>();
        _serializationService = SerializationService ?? throw new ArgumentNullException(nameof(serializationService));
        _ringBuffer = ringBuffer ?? throw new ArgumentNullException(nameof(ringBuffer));
    }

    /// <inheritdoc />
    public Task<Guid> SubscribeAsync(Action<ReliableTopicEventHandler<TItem>> events, ReliableTopicEventHandlerOptions handlerOptions = default, object state = null, CancellationToken cancellationToken = default)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));

        var id = Guid.NewGuid();
        
        var executor = ReliableTopicMessageExecutor<TItem>
            .New(_ringBuffer,
                events,
                state,
                this,
                handlerOptions,
                _options.BatchSize,
                _cluster,
                _serializationService,
                id,
                _loggerFactory,
                cancellationToken
            );

        _executors[id] = executor;

        _logger.IfDebug()?.LogDebug("{Id} reliable topic listener subscribed. ", id);

        return Task.FromResult(id);
    }

    /// <inheritdoc />
    public ValueTask<bool> UnsubscribeAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        if (_executors.TryRemove(subscriptionId, out var executor))
        {
            // Executor is local to client. No need to wait server, kill it.
            executor.DisposeAsync().CfAwait();
            return new ValueTask<bool>(true);
        }

        _logger.IfDebug()?.LogDebug("Subscription {Id} not exist or already removed. ", subscriptionId);

        return new ValueTask<bool>(false);
    }

    /// <inheritdoc />
    public Task PublishAsync(TItem message, CancellationToken cancellationToken = default)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        try
        {
            var data = _serializationService.ToData(message);
            var rtMessage = new ReliableTopicMessage(data, null);

            _logger.IfDebug()?.LogDebug("Adding message with policy {OptionsPolicy}", _options.Policy);

            return _options.Policy switch
            {
                TopicOverloadPolicy.DiscardOldest => _ringBuffer.AddAsync(rtMessage, OverflowPolicy.Overwrite),
                TopicOverloadPolicy.DiscardNewest => _ringBuffer.AddAsync(rtMessage, OverflowPolicy.Fail),
                TopicOverloadPolicy.Block => AddAsBlockingAsync(rtMessage, cancellationToken),
                TopicOverloadPolicy.Error => AddOrFail(rtMessage),
#pragma warning disable CA2208 Instantiate argument exceptions correctly // The check depends on policy option. 
                _ => throw new ArgumentOutOfRangeException(nameof(_options.Policy))
#pragma warning restore CA2208
            };
        }
        catch (Exception e)
        {
            _logger.IfDebug()?.LogError(e, "Failed while publishing a message {Message} on topic {Name}. ", message, Name);
            throw;
        }
    }

    private async Task AddOrFail(ReliableTopicMessage rtMessage)
    {
        var result = await _ringBuffer.AddAsync(rtMessage, OverflowPolicy.Fail).CfAwait();

        _logger.IfDebug()?.LogError("Failed to publish a message [{Message}] on topic [{Name}]", rtMessage, Name);

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

            _logger.IfDebug()?.LogDebug("Waiting to publish message with {Duration}ms back off", wait);
            await Task.Delay(wait, cancellationToken).CfAwait();

            wait *= 2;

            if (wait > _backOffMax) wait = _backOffMax;
        }

        if (cancellationToken.IsCancellationRequested)
            _logger.IfDebug()?.LogDebug("Publishing process is canceled. {Message}", rtMessage);
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed.InterlockedZeroToOne())
        {
            foreach (var id in _executors.Keys)
            {
                UnsubscribeAsync(id).CfAwait();
            }
        }

        return default;
    }

    public new async ValueTask DestroyAsync()
    {
        // Can't destroy if disposed.
        if (_disposed > 0) return;
        await _ringBuffer.DestroyAsync().CfAwait();
        await DestroyAsync().CfAwait();
    }
}
