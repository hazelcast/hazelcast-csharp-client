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

using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Hazelcast.Testing.Logging
{
    public static class TestingLoggerExtensions
    {
        public static ILoggingBuilder AddStringBuilder(this ILoggingBuilder builder, StringBuilder text, TestingLoggerOptions options = null)
        {
            builder.AddConfiguration();

            var descriptor = ServiceDescriptor.Singleton<ILoggerProvider>(new StringBuilderLoggerProvider(text, options));
            builder.Services.TryAddEnumerable(descriptor);
            //LoggerProviderOptions.RegisterProviderOptions<ConsoleLoggerOptions, ConsoleLoggerProvider>(builder.Services);
            return builder;
        }

        public static ILoggingBuilder AddHConsole(this ILoggingBuilder builder, TestingLoggerOptions options = null)
        {
            builder.AddConfiguration();

            var descriptor = ServiceDescriptor.Singleton<ILoggerProvider>(new HConsoleLoggerProvider(options));
            builder.Services.TryAddEnumerable(descriptor);
            return builder;
        }

        /// <summary>
        /// Sets the logger factory
        /// </summary>
        /// <param name="builder">The options builder.</param>
        /// <returns>This options builder.</returns>
        public static HazelcastOptionsBuilder WithHConsoleLogger(this HazelcastOptionsBuilder builder)
        {
            return builder
                .With("Logging:LogLevel:Default", "Debug")
                .With("Logging:LogLevel:System", "Information")
                .With("Logging:LogLevel:Microsoft", "Information")
                .With((configuration, hazelcastOptions) =>
                {
                    // configure logging factory and add the console provider
                    hazelcastOptions.LoggerFactory.Creator = () => LoggerFactory.Create(loggingBuilder =>
                        loggingBuilder
                            .AddConfiguration(configuration.GetSection("logging"))
                            .AddHConsole());
                });
        }

        /// <summary>
        /// Sets user secrets.
        /// </summary>
        /// <param name="builder">The options builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="key">The key.</param>
        /// <param name="optional">Whether secrets are optional.</param>
        /// <returns>This options builder.</returns>
        public static HazelcastOptionsBuilder WithUserSecrets(this HazelcastOptionsBuilder builder, Assembly assembly, string key = "hazelcast" /*HazelcastOptions.Hazelcast*/, bool optional = true)
        {
            return builder
                .WithAltKey(key)
                .With(configurationBuilder =>
                {
                    configurationBuilder.AddUserSecrets(assembly, optional);
                });
        }
    }
}
