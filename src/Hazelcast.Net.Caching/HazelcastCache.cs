// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Microsoft.Extensions.Caching.Distributed;

namespace Hazelcast.Caching;

/// <summary>
/// Provides a <see cref="IDistributedCache"/> implementation based on Hazelcast.
/// </summary>
/// <remarks>
/// <para>The <see cref="HazelcastCache"/> relies on a <see cref="IHazelcastClient"/> which
/// needs to be properly configured, and in particular MUST be configured for reconnecting
/// the cluster (see <see cref="Networking.NetworkingOptions.ReconnectMode"/>). Otherwise,
/// the cache may end up in a disconnected state, and never be able to recover.</para>
/// <para>The <see cref="HazelcastCache"/> uses the <see cref="IHazelcastClient"/> which
/// is fully asynchronous. Although <see cref="IDistributedCache"/> exposes synchronous
/// methods such as <see cref="Get"/>, these are implemented on top of their asynchronous
/// counterpart via the <c>.GetAwaiter().GetResult()</c> pattern and should be avoided.</para>
/// </remarks>
public class HazelcastCache : IDistributedCache, IAsyncDisposable
{
    private readonly HazelcastOptions? _hazelcastOptions;
    private readonly HazelcastFailoverOptions? _hazelcastFailoverOptions;
    private readonly SemaphoreSlim _clientLock = new(1, 1);
    private readonly string _mapName;
    private IHazelcastClient? _client;
    private IHMap<string, byte[]>? _map;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HazelcastCache"/> class.
    /// </summary>
    /// <param name="hazelcastOptions">The Hazelcast options.</param>
    /// <param name="cacheOptions">The caching configuration options.</param>
    public HazelcastCache(HazelcastOptions hazelcastOptions, HazelcastCacheOptions cacheOptions)
    {
        if (cacheOptions == null) throw new ArgumentNullException(nameof(cacheOptions));
        _hazelcastOptions = hazelcastOptions ?? throw new ArgumentNullException(nameof(hazelcastOptions));
        _mapName = CreateMapName(cacheOptions.CacheUniqueIdentifier);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HazelcastCache"/> class which will use a failover client.
    /// </summary>
    /// <param name="hazelcastFailoverOptions">The Hazelcast failover options.</param>
    /// <param name="cacheOptions">The caching configuration options.</param>
    public HazelcastCache(HazelcastFailoverOptions hazelcastFailoverOptions, HazelcastCacheOptions cacheOptions)
    {
        if (cacheOptions == null) throw new ArgumentNullException(nameof(cacheOptions));
        _hazelcastFailoverOptions = hazelcastFailoverOptions ?? throw new ArgumentNullException(nameof(hazelcastFailoverOptions));
        _mapName = CreateMapName(cacheOptions.CacheUniqueIdentifier);
    }

    private static string CreateMapName(string? cacheId)
        => string.IsNullOrWhiteSpace(cacheId) ? "distributed_cache" : cacheId!;

    private async Task ConnectAsync(CancellationToken token = default)
    {
        if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        token.ThrowIfCancellationRequested();

        if (_map != null) return;

        await _clientLock.WaitAsync(token).CfAwait();
        try
        {
            if (_map != null) return;

            if (_hazelcastOptions is not null)
                _client = await HazelcastClientFactory.StartNewClientAsync(_hazelcastOptions, token).CfAwait();
            else if (_hazelcastFailoverOptions is not null)
                _client = await HazelcastClientFactory.StartNewFailoverClientAsync(_hazelcastFailoverOptions, token).CfAwait();
            else
                throw new ConfigurationException("Hazelcast client options or failover client options must be provided.");

            _map = await _client.GetMapAsync<string, byte[]>(_mapName).CfAwait();
        }
        finally
        {
            _clientLock.Release();
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>This method is implemented via the <c>.GetAwaiter().GetResult()</c> on top of
    /// its <see cref="GetAsync"/> counterpart. Avoid using it and prefer the asynchronous
    /// method.</para>
    /// </remarks>
    public byte[]? Get(string key) => GetAsync(key).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => GetAndRefreshAsync(key, getData: true, token);

    /// <inheritdoc />
    /// <remarks>
    /// <para>This method is implemented via the <c>.GetAwaiter().GetResult()</c> on top of
    /// its <see cref="SetAsync"/> counterpart. Avoid using it and prefer the asynchronous
    /// method.</para>
    /// </remarks>
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => SetAsync(key, value, options).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (options == null) throw new ArgumentNullException(nameof(options));

        token.ThrowIfCancellationRequested();
        await ConnectAsync(token).CfAwait();

        // the precedence order used here, i.e. AbsoluteExpirationRelativeToNow taking
        // over AbsoluteExpiration, is not formally specified but is what Microsoft uses
        // in all their own providers.

        var maxIdle = options.SlidingExpiration ?? TimeSpan.Zero; // zero = never idle
        var timeToLive = options.AbsoluteExpirationRelativeToNow ??
                                (options.AbsoluteExpiration.HasValue
                                    ? options.AbsoluteExpiration.Value - DateTimeOffset.UtcNow
                                    : TimeSpan.Zero); // zero = infinite

        if (maxIdle < TimeSpan.Zero || timeToLive < TimeSpan.Zero)
            throw new ArgumentException("Options produce negative max-idle or time-to-live.", nameof(options));

        await _map!.SetAsync(key, value, timeToLive, maxIdle).CfAwait();
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>This method is implemented via the <c>.GetAwaiter().GetResult()</c> on top of
    /// its <see cref="RefreshAsync"/> counterpart. Avoid using it and prefer the asynchronous
    /// method.</para>
    /// </remarks>
    public void Refresh(string key) => GetAndRefreshAsync(key, getData: false).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task RefreshAsync(string key, CancellationToken token = default) => GetAndRefreshAsync(key, getData: false, token);

    private async Task<byte[]?> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        token.ThrowIfCancellationRequested();
        await ConnectAsync(token).CfAwait();

        if (getData) return await _map!.GetAsync(key).CfAwait();

        await _map!.ContainsKeyAsync(key).CfAwait(); // 'contains' does bump idle reference i.e. last read time
        return default;
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>This method is implemented via the <c>.GetAwaiter().GetResult()</c> on top of
    /// its <see cref="RemoveAsync"/> counterpart. Avoid using it and prefer the asynchronous
    /// method.</para>
    /// </remarks>
    public void Remove(string key) => RemoveAsync(key).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        await ConnectAsync(token).CfAwait();
        await _map!.DeleteAsync(key).CfAwait();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_map != null) await _map.DisposeAsync();
        if (_client != null) await _client.DisposeAsync();
    }
}
