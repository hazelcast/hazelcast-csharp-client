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
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast.Logging
{
    // NOTES
    //
    // Users of ILogger should avoid string interpolation, such as
    //   logger.LogDebug($"Get ID {id} failed.");
    // as the string is always computed, regardless of the current log
    // level. Rather, users should use message template format:
    //   logger.LogDebug("Get ID {Id} failed.", id);

    /// <summary>
    /// Provides logging extension methods for the <see cref="Services.ServiceGetter"/> class.
    /// </summary>
    internal static class ServicesLoggingExtensions
    {
        /// <summary>
        /// Gets the <see cref="ILoggerFactory"/>.
        /// </summary>
        /// <param name="objects">Container objects.</param>
        /// <returns>The <see cref="ILoggerFactory"/>.</returns>
        public static ILoggerFactory LoggerFactory(this Services.ServiceGetter objects)
            => Services.TryGetInstance<ILoggerFactory>() ?? new NullLoggerFactory();
    }
}