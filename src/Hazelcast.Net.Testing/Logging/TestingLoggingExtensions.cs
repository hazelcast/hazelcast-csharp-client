// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Hazelcast.Core;

namespace Hazelcast.Testing.Logging
{
    /// <summary>
    /// Provides extension methods for logging.
    /// </summary>
    public static class TestingLoggingExtensions
    {
        /// <summary>
        /// Adds a <see cref="StringBuilder"/> logger.
        /// </summary>
        /// <param name="builder">This <see cref="ILoggingBuilder"/>.</param>
        /// <param name="text">The <see cref="StringBuilder"/> to write to.</param>
        /// <param name="options">Options.</param>
        /// <returns>This <see cref="ILoggingBuilder"/>.</returns>
        public static ILoggingBuilder AddStringBuilder(this ILoggingBuilder builder, StringBuilder text, TestingLoggerOptions options = null)
        {
            builder.AddConfiguration();

            var descriptor = ServiceDescriptor.Singleton<ILoggerProvider>(new StringBuilderLoggerProvider(text, options));
            builder.Services.TryAddEnumerable(descriptor);
            return builder;
        }

        /// <summary>
        /// Adds a <see cref="HConsole"/> logger.
        /// </summary>
        /// <param name="builder">This <see cref="ILoggingBuilder"/>.</param>
        /// <param name="options">Options.</param>
        /// <returns>This <see cref="ILoggingBuilder"/>.</returns>
        public static ILoggingBuilder AddHConsole(this ILoggingBuilder builder, TestingLoggerOptions options = null)
        {
            builder.AddConfiguration();

            var descriptor = ServiceDescriptor.Singleton<ILoggerProvider>(new HConsoleLoggerProvider(options));
            builder.Services.TryAddEnumerable(descriptor);
            return builder;
        }

        /// <summary>
        /// Configure an <see cref="ILoggerFactory"/> with a logger to <see cref="HConsole"/>.
        /// </summary>
        /// <param name="builder">The options builder.</param>
        /// <param name="hazelcastLevel">The log level for Hazelcast code.</param>
        /// <returns>This options builder.</returns>
        public static HazelcastOptionsBuilder WithHConsoleLogger(this HazelcastOptionsBuilder builder, LogLevel hazelcastLevel = LogLevel.Debug)
        {
            return builder
                .With("Logging:LogLevel:Default", "Debug")
                .With("Logging:LogLevel:System", "Information")
                .With("Logging:LogLevel:Microsoft", "Information")
                .With("Logging:LogLevel:Hazelcast", hazelcastLevel.ToString())
                .With((configuration, hazelcastOptions) =>
                {
                    // configure logging factory and add the logging provider
                    hazelcastOptions.LoggerFactory.Creator = () => LoggerFactory.Create(loggingBuilder =>
                        loggingBuilder
                            .AddConfiguration(configuration.GetSection("logging"))
                            .AddHConsole());
                });
        }
    }
}
