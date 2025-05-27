﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Caching;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.DependencyInjection;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Hazelcast.Tests.Caching;

[TestFixture]
public class HazelcastCacheTests : SingleMemberRemoteTestBase
{
    private void ConfigureOptions(HazelcastOptions options)
    {
        options.ClusterName = RcCluster?.Id ?? options.ClusterName;
        options.Networking.Addresses.Clear();
        options.Networking.Addresses.Add("127.0.0.1:5701");
        options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync; // important!
        options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 2000;
        options.LoggerFactory.Creator = () => LoggerFactory;
    }

    protected override HazelcastOptions CreateHazelcastOptions()
        => CreateHazelcastOptionsBuilder().With(ConfigureOptions).Build();

    [Test]
    public void TestOptions()
    {
        var cacheOptions = new HazelcastCacheOptions
        {
            CacheUniqueIdentifier = Guid.NewGuid().ToShortString()
        };

        var ioptions = (IOptions<HazelcastCacheOptions>)cacheOptions;
        Assert.That(ioptions.Value, Is.SameAs(cacheOptions));
    }

    [Test]
    public async Task TestCacheExceptions()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new HazelcastCache(new HazelcastOptions(), null!));
        Assert.Throws<ArgumentNullException>(() => _ = new HazelcastCache(new HazelcastFailoverOptions(), null!));
        Assert.Throws<ArgumentNullException>(() => _ = new HazelcastCache(((HazelcastOptions)null)!, new HazelcastCacheOptions()));
        Assert.Throws<ArgumentNullException>(() => _ = new HazelcastCache(((HazelcastFailoverOptions)null)!, new HazelcastCacheOptions()));

        var cacheOptions = new HazelcastCacheOptions
        {
            CacheUniqueIdentifier = Guid.NewGuid().ToShortString()
        };
        await using var cache = new HazelcastCache(CreateHazelcastOptions(), cacheOptions);

        Assert.Throws<ArgumentNullException>(() => cache.Get(null!));
        Assert.Throws<ArgumentNullException>(() => cache.Set(null!, Array.Empty<byte>(), new DistributedCacheEntryOptions()));
        Assert.Throws<ArgumentNullException>(() => cache.Set("xxx", null!, new DistributedCacheEntryOptions()));
        Assert.Throws<ArgumentNullException>(() => cache.Set("xxx", Array.Empty<byte>(), null!));
        Assert.Throws<ArgumentNullException>(() => cache.Remove(null!));
        Assert.Throws<ArgumentNullException>(() => cache.Refresh(null!));

        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await cache.GetAsync(null!));
        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await cache.SetAsync(null!, Array.Empty<byte>(), new DistributedCacheEntryOptions()));
        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await cache.SetAsync("xxx", null!, new DistributedCacheEntryOptions()));
        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await cache.SetAsync("xxx", Array.Empty<byte>(), null!));
        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await cache.RemoveAsync(null!));
        await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await cache.RefreshAsync(null!));

        await AssertEx.ThrowsAsync<ArgumentException>(async () => await cache.SetAsync("xxx", Array.Empty<byte>(),
            new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(-2) }));

        await cache.DisposeAsync();

        await AssertEx.ThrowsAsync<ObjectDisposedException>(async () => await cache.SetStringAsync("xxx", "xxx"));
    }

    [Test]
    public void ProvidedCacheExceptions()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new ProvidedHazelcastCache(Substitute.For<IOptions<HazelcastOptions>>(), null!));
        Assert.Throws<ArgumentNullException>(() => _ = new ProvidedHazelcastCache(null!, Substitute.For<IOptions<HazelcastCacheOptions>>()));
        Assert.Throws<ArgumentNullException>(() => _ = new ProvidedHazelcastCacheWithFailover(Substitute.For<IOptions<HazelcastFailoverOptions>>(), null!));
        Assert.Throws<ArgumentNullException>(() => _ = new ProvidedHazelcastCacheWithFailover(null!, Substitute.For<IOptions<HazelcastCacheOptions>>()));
    }

    [Test]
    public async Task TestCacheSynchronous()
    {
        // we have to test it... but it should never be used synchronously

        var cacheOptions = new HazelcastCacheOptions
        {
            CacheUniqueIdentifier = Guid.NewGuid().ToShortString()
        };
        await using var cache = new HazelcastCache(CreateHazelcastOptions(), cacheOptions);

        // ReSharper disable MethodHasAsyncOverload

        cache.SetString("key0", "value0", new DistributedCacheEntryOptions());
        Assert.That(cache.GetString("key0"), Is.EqualTo("value0"));
        cache.Remove("key0");
        Assert.That(cache.GetString("key0"), Is.Null);

        // ReSharper restore MethodHasAsyncOverload
    }

    [Test]
    public async Task TestCache()
    {
        var cacheOptions = new HazelcastCacheOptions
        {
            CacheUniqueIdentifier = Guid.NewGuid().ToShortString()
        };
        await using var cache = new HazelcastCache(CreateHazelcastOptions(), cacheOptions);

        await cache.SetStringAsync("key0", "value0", new DistributedCacheEntryOptions());
        Assert.That(await cache.GetStringAsync("key0"), Is.EqualTo("value0"));
        await cache.RemoveAsync("key0");
        Assert.That(await cache.GetStringAsync("key0"), Is.Null);
    }

    [Test]
    [Category("enterprise")]
    public async Task TestCache_WithFailover()
    {
        var cacheOptions = new HazelcastCacheOptions
        {
            CacheUniqueIdentifier = Guid.NewGuid().ToShortString()
        };

        // note: we do *not* want to test that failover works here, just that
        // the cache works when configured with a failover client.

        var options = new HazelcastFailoverOptionsBuilder()
            .With(o =>
            {
                o.TryCount = 2;
                o.Clients.Add(CreateHazelcastOptions());
            })
            .Build();

        await using var cache = new HazelcastCache(options, cacheOptions);

        await cache.SetStringAsync("key0", "value0", new DistributedCacheEntryOptions());
        Assert.That(await cache.GetStringAsync("key0"), Is.EqualTo("value0"));
        await cache.RemoveAsync("key0");
        Assert.That(await cache.GetStringAsync("key0"), Is.Null);
    }

    [Test]
    public async Task TestCacheAbsoluteExpiration()
    {
        var cacheOptions = new HazelcastCacheOptions
        {
            CacheUniqueIdentifier = Guid.NewGuid().ToShortString()
        };
        await using var cache = new HazelcastCache(CreateHazelcastOptions(), cacheOptions);

        await cache.SetStringAsync("key0", "value0", new DistributedCacheEntryOptions{ AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(30) });
        Assert.That(await cache.GetStringAsync("key0"), Is.EqualTo("value0"));
        await AssertEx.SucceedsEventually(async () =>
        {
            Assert.That(await cache.GetStringAsync("key0"), Is.Null);
        }, 120_000, 10_000);
    }

    [Test]
    public async Task TestCacheAbsoluteExpirationFromNow()
    {
        var cacheOptions = new HazelcastCacheOptions
        {
            CacheUniqueIdentifier = Guid.NewGuid().ToShortString()
        };
        await using var cache = new HazelcastCache(CreateHazelcastOptions(), cacheOptions);

        await cache.SetStringAsync("key0", "value0", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) });
        Assert.That(await cache.GetStringAsync("key0"), Is.EqualTo("value0"));
        await AssertEx.SucceedsEventually(async () =>
        {
            Assert.That(await cache.GetStringAsync("key0"), Is.Null);
        }, 120_000, 10_000);
    }

    [Test]
    public async Task TestCacheSlidingExpirationWithGet()
    {
        var cacheOptions = new HazelcastCacheOptions
        {
            CacheUniqueIdentifier = Guid.NewGuid().ToShortString()
        };
        await using var cache = new HazelcastCache(CreateHazelcastOptions(), cacheOptions);

        await cache.SetStringAsync("key0", "value0", new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(30) });
        Assert.That(await cache.GetStringAsync("key0"), Is.EqualTo("value0"));
        for (var i = 0; i < 12; i++)
        {
            await Task.Delay(10_000); // won't go away as long as touched often enough
            Assert.That(await cache.GetStringAsync("key0"), Is.EqualTo("value0"));
        }

        Assert.That(await cache.GetStringAsync("key0"), Is.EqualTo("value0"));
        await Task.Delay(60_000); // but does away if idle for long enough
        Assert.That(await cache.GetStringAsync("key0"), Is.Null);
    }

    [Test]
    public async Task TestCacheSlidingExpirationWithRefresh()
    {
        var cacheOptions = new HazelcastCacheOptions
        {
            CacheUniqueIdentifier = Guid.NewGuid().ToShortString()
        };
        await using var cache = new HazelcastCache(CreateHazelcastOptions(), cacheOptions);

        await cache.SetStringAsync("key0", "value0", new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(30) });
        Assert.That(await cache.GetStringAsync("key0"), Is.EqualTo("value0"));
        for (var i = 0; i < 12; i++)
        {
            await Task.Delay(10_000); // won't go away as long as touched often enough
            await cache.RefreshAsync("key0");
        }

        Assert.That(await cache.GetStringAsync("key0"), Is.EqualTo("value0"));
        await Task.Delay(60_000); // but does away if idle for long enough
        Assert.That(await cache.GetStringAsync("key0"), Is.Null);
    }

    [Test]
    [Timeout(10000)]
    public async Task TestCacheProvided_ExplicitOptions()
    {
        var services = new ServiceCollection();

        // create options
        var hazelcastOptions = new HazelcastOptionsBuilder().With(ConfigureOptions).Build();
        var cacheOptions = new HazelcastCacheOptions { CacheUniqueIdentifier = Guid.NewGuid().ToShortString() };

        // add Hazelcast cache with full explicit options
        services.AddHazelcastCache(hazelcastOptions, cacheOptions);

        // the IDistributedCache will be injected into the asserter
        services.AddTransient<ProvidedCacheAsserter>();

        // create the provider and retrieve and use the cache
        await using var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<ProvidedCacheAsserter>().AssertAsync();
    }

    [Test]
    [Category("enterprise")]
    [Timeout(10000)]
    public async Task TestCacheProvided_ExplicitFailoverOptions()
    {
        var services = new ServiceCollection();

        // create options
        var hazelcastFailoverOptions = new HazelcastFailoverOptionsBuilder()
            .With(options => options.Clients.Add(new HazelcastOptionsBuilder().With(ConfigureOptions).Build()))
            .Build();
        var cacheOptions = new HazelcastCacheOptions { CacheUniqueIdentifier = Guid.NewGuid().ToShortString() };

        // add Hazelcast cache with full explicit options
        services.AddHazelcastCache(hazelcastFailoverOptions, cacheOptions);

        // the IDistributedCache will be injected into the asserter
        services.AddTransient<ProvidedCacheAsserter>();

        // create the provider and retrieve and use the cache
        await using var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<ProvidedCacheAsserter>().AssertAsync();
    }

    [Test]
    [Timeout(10000)]
    public async Task TestCacheProvided_ProvidedOptions()
    {
        var services = new ServiceCollection();

        // register options
        services.AddHazelcastOptions(builder => builder.With(ConfigureOptions));
        services.Configure<HazelcastCacheOptions>(options => options.CacheUniqueIdentifier = Guid.NewGuid().ToShortString());

        // add Hazelcast cache using provided options
        services.AddHazelcastCache();

        // the IDistributedCache will be injected into the asserter
        services.AddTransient<ProvidedCacheAsserter>();

        // create the provider and retrieve and use the cache
        await using var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<ProvidedCacheAsserter>().AssertAsync();
    }

    [Test]
    [Category("enterprise")]
    [Timeout(10000)]
    public async Task TestCacheProvided_ProvidedFailoverOptions()
    {
        var services = new ServiceCollection();

        // register Hazelcast options
        services.AddHazelcastFailoverOptions(builder => builder.With(options =>
        {
            options.Clients.Add(new HazelcastOptionsBuilder().With(ConfigureOptions).Build());
        }));
        services.Configure<HazelcastCacheOptions>(options => options.CacheUniqueIdentifier = Guid.NewGuid().ToShortString());

        // add Hazelcast cache using provided options
        services.AddHazelcastCache(withFailover: true);

        // the IDistributedCache will be injected into the asserter
        services.AddTransient<ProvidedCacheAsserter>();

        // create the provider and retrieve and use the cache
        await using var serviceProvider = services.BuildServiceProvider();
        await serviceProvider.GetRequiredService<ProvidedCacheAsserter>().AssertAsync();
    }

    [Test]
    [Timeout(10000)]
    public async Task TestCacheProvided_ConfiguredOptions()
    {
        var cacheUniqueIdentifier = Guid.NewGuid().ToShortString();

        // create the provider
        await using var serviceProvider = TestCacheProvided_ConfiguredOptions_GetServices(withFailover: false, cacheUniqueIdentifier);

        // verifications
        var ho = serviceProvider.GetRequiredService<IOptions<HazelcastOptions>>();
        Assert.That(ho.Value.Networking.ReconnectMode, Is.EqualTo(ReconnectMode.ReconnectAsync));
        var co = serviceProvider.GetRequiredService<IOptions<HazelcastCacheOptions>>();
        Assert.That(co.Value.CacheUniqueIdentifier, Is.EqualTo(cacheUniqueIdentifier));

        await serviceProvider.GetRequiredService<ProvidedCacheAsserter>().AssertAsync();
    }

    [Test]
    [Category("enterprise")]
    [Timeout(10000)]
    public async Task TestCacheProvided_ConfiguredFailoverOptions()
    {
        var cacheUniqueIdentifier = Guid.NewGuid().ToShortString();

        // create the provider
        await using var serviceProvider = TestCacheProvided_ConfiguredOptions_GetServices(withFailover: true, cacheUniqueIdentifier);

        // verifications
        var ho = serviceProvider.GetRequiredService<IOptions<HazelcastFailoverOptions>>();
        Assert.That(ho.Value.Clients.Count, Is.EqualTo(1));
        Assert.That(ho.Value.Clients[0].Networking.ReconnectMode, Is.EqualTo(ReconnectMode.ReconnectAsync));
        var co = serviceProvider.GetRequiredService<IOptions<HazelcastCacheOptions>>();
        Assert.That(co.Value.CacheUniqueIdentifier, Is.EqualTo(cacheUniqueIdentifier));

        await serviceProvider.GetRequiredService<ProvidedCacheAsserter>().AssertAsync();
    }

    private ServiceProvider TestCacheProvided_ConfiguredOptions_GetServices(bool withFailover, string cacheUniqueIdentifier)
    {
        var services = new ServiceCollection();

        // note:
        //   standard dotnet key delimiter is ':' or '__' depending on providers, but AddHazelcastAndDefaults add
        //   support for '.' too for command line, environment and in-memory collection providers - but only for keys
        //   starting with 'hazelcast.' or 'hazelcast-failover.'. Internally, the '.' separator is rewritten into the
        //   ':' separator - which should be used everywhere really, for instance in the Bind call used in the
        //   HazelcastCacheOptions registration. we're using the standard ':' delimiter everywhere in this test.

        // build the IConfiguration instance
        // pretend all options come from the configuration (via a key/values provider, could be command line or anything)
        var clientKey = withFailover ? "hazelcast-failover:clients:0" : "hazelcast";
        var configuration = new ConfigurationBuilder()
            .AddHazelcastAndDefaults(args: Array.Empty<string>(), keyValues: new KeyValuePair<string, string>[]
            {
                new($"{clientKey}:networking:addresses:0", "localhost:5701"),
                new($"{clientKey}:networking:reconnectMode", "ReconnectAsync"),
                new($"{clientKey}:clusterName", RcCluster?.Id ?? ""),
                new("hazelcast:caching:cacheUniqueIdentifier", cacheUniqueIdentifier),
                new("logging:logLevel:default", "Debug"),
            })
            .Build();

        // add logging to the container, the normal way
        services.AddLogging(builder => builder.AddConfiguration(configuration.GetSection("logging")).AddConsole());

        // configure hazelcast options
        void ConfigureHazelcastOptions<TOptions, TBuilder>(TBuilder builder)
            where TBuilder : HazelcastOptionsBuilderBase<TOptions, TBuilder>
            where TOptions : HazelcastOptionsBase, new()
        {
            builder

                // get hazelcast options from the configuration defined above
                .AddConfiguration(configuration)

                // override logger factory options to specify that it should be provided by the service provider
                .With(options => options.ObtainLoggerFactoryFromServiceProvider());
        }

        // register Hazelcast options
        if (withFailover)
            services.AddHazelcastFailoverOptions(ConfigureHazelcastOptions<HazelcastFailoverOptions, HazelcastFailoverOptionsBuilder>);
        else
            services.AddHazelcastOptions(ConfigureHazelcastOptions<HazelcastOptions, HazelcastOptionsBuilder>);

        // note: that would be a nicer syntax, but it requires that the factory has access
        // back to the options (or some sort of IProvideServiceProvider service) and that
        // would be a breaking change in the factory signature -> not now.
        //options.LoggerFactory.ObtainFromServiceProvider();

        // register Hazelcast cache options
        // get them from the configuration defined above
        services.Configure<HazelcastCacheOptions>(options => configuration.Bind("hazelcast:caching", options));

        // add Hazelcast cache using provided options
        // beware! must use the right combination of AddHazelcast[Failover]Options and withFailover: true|false
        services.AddHazelcastCache(withFailover);

        // the IDistributedCache will be injected into the asserter
        services.AddTransient<ProvidedCacheAsserter>();

        // create the provider and retrieve and use the cache
        return services.BuildServiceProvider();
    }

    internal class ProvidedCacheAsserter
    {
        private readonly IDistributedCache _cache;

        public ProvidedCacheAsserter(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task AssertAsync()
        {
            Assert.That(_cache, Is.Not.Null);
            Assert.That(_cache, Is.InstanceOf<HazelcastCache>());

            // test that we can use the cache
            await _cache.SetStringAsync("key0", "value0", new DistributedCacheEntryOptions());
            Assert.That(await _cache.GetStringAsync("key0"), Is.EqualTo("value0"));
            await _cache.RemoveAsync("key0");
            Assert.That(await _cache.GetStringAsync("key0"), Is.Null);
        }
    }
}