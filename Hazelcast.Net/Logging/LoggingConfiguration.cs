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

using Hazelcast.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast.Logging
{
    /// <summary>
    /// Represents the logging configuration.
    /// </summary>
    public class LoggingConfiguration
    {
        /// <summary>
        /// Gets the service factory for <see cref="ILoggerFactory"/>.
        /// </summary>
        public ServiceFactory<ILoggerFactory> LoggerFactory { get; } = new ServiceFactory<ILoggerFactory>(() => new NullLoggerFactory());
    }
}