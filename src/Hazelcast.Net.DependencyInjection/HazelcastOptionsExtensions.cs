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

#nullable enable

using System;

namespace Hazelcast.DependencyInjection;

public static class HazelcastOptionsExtensions
{
    /// <summary>
    /// Specifies that the options logger factory is to be obtained from the service provider.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The options so that additional calls can be chained.</returns>
    public static HazelcastOptions ObtainLoggerFactoryFromServiceProvider(this HazelcastOptions options)
    {
        options.LoggerFactory.ServiceProvider = options.ServiceProvider;
        return options;
    }

    /// <summary>
    /// Specifies that the options logger factory is to be obtained from the service provider.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The options so that additional calls can be chained.</returns>
    public static HazelcastFailoverOptions ObtainLoggerFactoryFromServiceProvider(this HazelcastFailoverOptions options)
    {
        foreach (var o in options.Clients) o.ObtainLoggerFactoryFromServiceProvider();
        return options;
    }

    /// <summary>
    /// Specifies that the options logger factory is to be obtained from the service provider.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The options so that additional calls can be chained.</returns>
    public static TOptions ObtainLoggerFactoryFromServiceProvider<TOptions>(this TOptions options)
        where TOptions : HazelcastOptionsBase
    {
        switch (options ?? throw new ArgumentNullException(nameof(options)))
        {
            case HazelcastOptions hazelcastOptions:
                hazelcastOptions.ObtainLoggerFactoryFromServiceProvider();
                break;
            case HazelcastFailoverOptions hazelcastFailoverOptions:
                hazelcastFailoverOptions.ObtainLoggerFactoryFromServiceProvider();
                break;
            default:
                throw new NotSupportedException($"Unsupported options type {options.GetType()}.");
        }

        return options;
    }
}
