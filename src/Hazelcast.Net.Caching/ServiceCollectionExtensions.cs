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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Hazelcast.Caching;

/// <summary>
/// Provides extension methods to the <see cref="IServiceCollection"/> interface.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="HazelcastCache"/> as implementing <see cref="IDistributedCache"/> with the specified options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="hazelcastOptions">Hazelcast options.</param>
    /// <param name="cacheOptions">Cache options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddHazelcastCache(this IServiceCollection services, HazelcastOptions hazelcastOptions, HazelcastCacheOptions cacheOptions)
    {
        services.AddSingleton<IDistributedCache>(_ => new HazelcastCache(hazelcastOptions, cacheOptions));
        return services;
    }

    /// <summary>
    /// Registers <see cref="HazelcastCache"/> as implementing <see cref="IDistributedCache"/> with the specified options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="hazelcastOptions">Hazelcast options.</param>
    /// <param name="cacheOptions">Cache options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddHazelcastCache(this IServiceCollection services, HazelcastFailoverOptions hazelcastOptions, HazelcastCacheOptions cacheOptions)
    {
        services.AddSingleton<IDistributedCache>(_ => new HazelcastCache(hazelcastOptions, cacheOptions));
        return services;
    }

    /// <summary>
    /// Registers <see cref="HazelcastCache"/> as implementing <see cref="IDistributedCache"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="withFailover">Whether to use a failover Hazelcast client.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>Options will be obtained from the service container.</remarks>
    public static IServiceCollection AddHazelcastCache(this IServiceCollection services, bool withFailover = false)
    {
        if (withFailover)
            services.AddSingleton<IDistributedCache, ProvidedHazelcastCacheWithFailover>();
        else
            services.AddSingleton<IDistributedCache, ProvidedHazelcastCache>();
        return services;
    }
}