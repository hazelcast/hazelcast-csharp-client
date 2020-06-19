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

using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Logging
{
    /// <summary>
    /// Represents the logging options.
    /// </summary>
    /// <remarks>
    /// <para>The only option available is the <see cref="ILoggerFactory"/> creator, which can only
    /// be set programmatically. All other logging options (level, etc.) are configured via the
    /// default Microsoft configuration system. See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging
    /// for details and documentation.</para>
    /// </remarks>
    public class LoggingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingOptions"/> class.
        /// </summary>
        public LoggingOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingOptions"/> class.
        /// </summary>
        private LoggingOptions(LoggingOptions other)
        {
            LoggerFactory = other.LoggerFactory.Clone();
        }

        /// <summary>
        /// Gets the service factory for <see cref="ILoggerFactory"/>.
        /// </summary>
        [BinderIgnore]
        public SingletonServiceFactory<ILoggerFactory> LoggerFactory { get; } = new SingletonServiceFactory<ILoggerFactory>();

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal LoggingOptions Clone() => new LoggingOptions(this);
    }
}
