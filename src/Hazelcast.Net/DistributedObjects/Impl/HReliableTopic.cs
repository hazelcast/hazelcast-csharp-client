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
    private readonly IHRingBuffer<ReliableTopicMessage> _ringBuffer;
    private readonly ReliableTopicOptions _options;
    private readonly Cluster _cluster;
    private readonly SerializationService _serializationService;
    private readonly ConcurrentDictionary<Guid, ReliableTopicMessageExecutor<TItem>> _executors = new();
    private ILogger _logger;
    private ILoggerFactory _loggerFactory;
    private int _backOffMax = 2000;
    private int _backOffInitial = 100;
    private string _name;
    private int _disposed;

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
    public Task<Guid> SubscribeAsync(Action<ReliableTopicEventHandler<TItem>> events,
        ReliableTopicEventHandlerOptions handlerOptions = default,
        Func<Exception, bool> shouldTerminate = default,
        object state = null)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));

        var id = Guid.NewGuid();

        // The executor can be disposed depending on a exception or loss tolerant policy.
        void OnExecutorDisposed(Guid sId) => _executors.TryRemove(sId, out _);

        // Create and register the executor. The executor starts immediately.
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
                OnExecutorDisposed,
                shouldTerminate
            );

        _executors[id] = executor;

        _logger.IfDebug()?.LogDebug("{Id} reliable topic listener subscribed. ", id);

        return Task.FromResult(id);
    }

    /// <inheritdoc />
    public async ValueTask<bool> UnsubscribeAsync(Guid subscriptionId)
    {
        if (_executors.TryRemove(subscriptionId, out var executor))
        {
            await executor.DisposeAsync().CfAwait();
            _logger.IfDebug()?.LogDebug("Subscription {Id} is unsubscribed. ", subscriptionId);
            return true;
        }

        _logger.IfDebug()?.LogDebug("Subscription {Id} is not exist or already unsubscribed. ", subscriptionId);

        return false;
    }

    /// <inheritdoc />
    public bool IsSubscriptionExist(Guid subscriptionId)
    {
        return _executors.TryGetValue(subscriptionId, out var executor) && !executor.IsDisposed;
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
            _logger.IfDebug()?.LogDebug("Publishing process is canceled. ");
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed.InterlockedZeroToOne())
        {
            foreach (var id in _executors.Keys)
            {
                await UnsubscribeAsync(id).CfAwait();
            }
        }
    }

    public new async ValueTask DestroyAsync()
    {
        // Can't destroy if disposed.
        if (_disposed > 0) return;
        await DisposeAsync().CfAwait();
        await _ringBuffer.DestroyAsync().CfAwait();
    }
}
