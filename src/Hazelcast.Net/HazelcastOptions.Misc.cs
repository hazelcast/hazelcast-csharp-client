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
using Hazelcast.NearCaching;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    public partial class HazelcastOptions // Misc
    {
        // NOTE
        // AsyncStart is not an option for the CSharp client,
        // as it would make little sense for our full-async code

        /// <summary>
        /// Gets the service factory for <see cref="ILoggerFactory"/>.
        /// </summary>
        /// <returns>The service factory for <see cref="ILoggerFactory"/>.</returns>
        /// <remarks>
        /// <para>The only option available for logging is the <see cref="ILoggerFactory"/> creator, which can only
        /// be set programmatically. All other logging options (level, etc.) are configured via the
        /// default Microsoft configuration system. See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging
        /// for details and documentation.</para>
        /// </remarks>

        [BinderIgnore]
        public SingletonServiceFactory<ILoggerFactory> LoggerFactory { get; } = new SingletonServiceFactory<ILoggerFactory>();

        /// <summary>
        /// Gets the serialization options.
        /// </summary>
        /// <returns>The serialization options.</returns>
        public SerializationOptions Serialization { get; } = new SerializationOptions();

        /// <summary>
        /// Gets the general Near Caching options.
        /// </summary>
        /// <returns>The general Near Caching options.</returns>
        public NearCachingOptions NearCache { get; } = new NearCachingOptions();

        /// <summary>
        /// Gets or sets the configuration pattern matcher.
        /// </summary>
        public IPatternMatcher PatternMatcher { get; set; } = new MatchingPointPatternMatcher();
    }
}
