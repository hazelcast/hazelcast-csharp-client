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
    }
}
