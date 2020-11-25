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

using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Represents the general Near Caching options.
    /// </summary>
    public class NearCachingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NearCachingOptions"/> class.
        /// </summary>
        public NearCachingOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCachingOptions"/> class.
        /// </summary>
        private NearCachingOptions(NearCachingOptions other)
        {
            ReconciliationIntervalSeconds = other.ReconciliationIntervalSeconds;
            MinReconciliationIntervalSeconds = other.MinReconciliationIntervalSeconds;
            MaxToleratedMissCount = other.ReconciliationIntervalSeconds;
            Caches = new Dictionary<string, NearCacheOptions>(other.Caches.ToDictionary(
                x => x.Key,
                x => x.Value.Clone()));
        }

        /// <summary>
        /// Gets or sets the reconciliation interval.
        /// </summary>
        public int ReconciliationIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the minimum reconciliation interval.
        /// </summary>
        public int MinReconciliationIntervalSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum tolerated miss count.
        /// </summary>
        public int MaxToleratedMissCount { get; set; } = 10;

        /// <summary>
        /// Gets options for Near Caches.
        /// </summary>
        public IDictionary<string, NearCacheOptions> Caches { get; } = new Dictionary<string, NearCacheOptions>();

        /// <summary>
        /// Gets options for a Near Cache.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="patternMatcher">A pattern matcher.</param>
        /// <returns>Options for the Near Cache matching the specified <paramref name="name"/>.</returns>
        public NearCacheOptions GetCacheOptions(string name, IPatternMatcher patternMatcher)
        {
            if (patternMatcher == null) throw new ArgumentNullException(nameof(patternMatcher));

            if (Caches.TryGetValue(name, out var configuration))
                return configuration;

            var key = patternMatcher.Matches(Caches.Keys, name);
            return key == null ? null : Caches[key];
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal NearCachingOptions Clone() => new NearCachingOptions(this);
    }
}
