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

using System.Collections.Generic;
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
        /// Gets the <see cref="SingletonServiceFactory{TService}"/> for <see cref="ILoggerFactory"/>.
        /// </summary>
        /// <returns>The <see cref="SingletonServiceFactory{T}"/> for <see cref="ILoggerFactory"/>.</returns>
        /// <remarks>
        /// <para>The only option available for logging is the <see cref="ILoggerFactory"/> creator, which can only
        /// be set programmatically. All other logging options (level, etc.) are configured via the
        /// default Microsoft configuration system. See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging
        /// for details and documentation.</para>
        /// </remarks>

        [BinderIgnore]
        public SingletonLoggerFactoryServiceFactory LoggerFactory { get; } = new SingletonLoggerFactoryServiceFactory();

        // FIXME document?
        public class SingletonLoggerFactoryServiceFactory : SingletonServiceFactory<ILoggerFactory>, ILoggerFactory
        {
            public SingletonLoggerFactoryServiceFactory() { }

            public SingletonLoggerFactoryServiceFactory(SingletonLoggerFactoryServiceFactory other, bool shallow) : base(other, shallow) { }

            /// <inheritdoc />
            public ILogger CreateLogger(string categoryName) => Service.CreateLogger(categoryName);

            /// <inheritdoc />
            public void AddProvider(ILoggerProvider provider) => Service.AddProvider(provider); // FIXME not sure we want to support this?

            /// <summary>
            /// Clones this service factory.
            /// </summary>
            /// <remarks>
            /// <para>When cloning a singleton service factory, a shallow clones is performed
            /// by default, meaning that the (lazy) service instance is cloned too. In other
            /// words, the singleton remains a singleton across clones.</para>
            /// <para>If <paramref name="shallow"/> is set to false, a deep clone is created,
            /// which would create an entirely new singleton.</para>
            /// </remarks>
            /// <returns>A clone of the service factory.</returns>
            internal new SingletonLoggerFactoryServiceFactory Clone(bool shallow = true) => new SingletonLoggerFactoryServiceFactory(this, shallow);
        }

        /// <summary>
        /// Gets the <see cref="SerializationOptions"/>.
        /// </summary>
        /// <returns>The serialization options.</returns>
        public SerializationOptions Serialization { get; } = new SerializationOptions();

        /// <summary>
        /// Gets the <see cref="CommonNearCacheOptions"/>.
        /// </summary>
        /// <returns>The common Near Cache options.</returns>
        public CommonNearCacheOptions NearCache { get; } = new CommonNearCacheOptions();

        /// <summary>
        /// Gets or sets the configuration pattern matcher.
        /// </summary>
        /// <remarks>
        /// <para>This can only be set programmatically.</para>
        /// </remarks>
        public IPatternMatcher PatternMatcher { get; set; } = new MatchingPointPatternMatcher();

        /// <summary>
        /// Gets the dictionary which contains the <see cref="NearCacheOptions"/> for each near cache.
        /// </summary>
        public IDictionary<string, NearCacheOptions> NearCaches { get; } = new Dictionary<string, NearCacheOptions>();

        /// <summary>
        /// Gets options for a near cache.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Options for the Near Cache matching the specified <paramref name="name"/>.</returns>
        public NearCacheOptions GetNearCacheOptions(string name)
        {
            if (NearCaches.TryGetValue(name, out var configuration))
                return configuration;

            var key = PatternMatcher.Matches(NearCaches.Keys, name);
            return key == null ? null : NearCaches[key];
        }
    }
}
