// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples
{
    /// <summary>
    /// Provides extension methods for the <see cref="HazelcastOptionsBuilder"/> class.
    /// </summary>
    public static class HazelcastOptionsBuilderExtensions
    {
        /// <summary>
        /// Sets the logger factory
        /// </summary>
        /// <param name="builder">The options builder.</param>
        /// <param name="hazelcastLogLevel">The Hazelcast log level.</param>
        /// <returns>The options builder.</returns>
        public static HazelcastOptionsBuilder WithConsoleLogger(this HazelcastOptionsBuilder builder, LogLevel hazelcastLogLevel = LogLevel.None)
        {
            return builder
                .With("Logging:LogLevel:Default", "None")
                .With("Logging:LogLevel:System", "Information")
                .With("Logging:LogLevel:Microsoft", "Information")
                .With("Logging:LogLevel:Hazelcast", hazelcastLogLevel.ToString())
                .With((configuration, o) =>
                {
                    // configure logging factory and add the console provider
                    o.LoggerFactory.Creator = () => LoggerFactory.Create(b =>
                        b
                            .AddConfiguration(configuration.GetSection("logging"))
                            .AddConsole());
                });
        }
    }
}
