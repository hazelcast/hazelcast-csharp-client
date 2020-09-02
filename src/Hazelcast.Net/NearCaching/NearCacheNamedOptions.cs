﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Contains the options for a Near Cache.
    /// </summary>
    public class NearCacheNamedOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheNamedOptions"/> class.
        /// </summary>
        public NearCacheNamedOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheNamedOptions"/> class.
        /// </summary>
        private NearCacheNamedOptions(NearCacheNamedOptions other)
        {
            EvictionPolicy = other.EvictionPolicy;
            EvictionPercentage = other.EvictionPercentage;
            InMemoryFormat = other.InMemoryFormat;
            MaxIdleSeconds = other.MaxIdleSeconds;
            CleanupPeriodSeconds = other.CleanupPeriodSeconds;
            MaxSize = other.MaxSize;
            TimeToLiveSeconds = other.TimeToLiveSeconds;
            InvalidateOnChange = other.InvalidateOnChange;
        }

        /// <summary>
        /// Gets or sets the eviction policy.
        /// </summary>
        public EvictionPolicy EvictionPolicy { get; set; } = EvictionPolicy.Lru;

        /// <summary>
        /// Gets or sets the eviction percentage.
        /// </summary>
        public int EvictionPercentage { get; set; } = 20;

        /// <summary>
        /// Gets or sets the in-memory format.
        /// </summary>
        public InMemoryFormat InMemoryFormat { get; set; } = InMemoryFormat.Binary;

        /// <summary>
        /// Gets or sets the maximum number of seconds an entry can stay in the cache untouched before being evicted.
        /// </summary>
        /// <remarks>
        /// <para>zero means forever.</para>
        /// </remarks>
        public int MaxIdleSeconds { get; set; }

        /// <summary>
        /// Gets or sets the period of the cleanup.
        /// </summary>
        public int CleanupPeriodSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the maximum size of the cache before entries get evicted.
        /// </summary>
        public int MaxSize { get; set; } = int.MaxValue;

        /// <summary>
        /// Gets or sets the number of seconds entries stay in the cache before being evicted.
        /// </summary>
        public int TimeToLiveSeconds { get; set; }

        /// <summary>
        /// Whether to invalidate entries when entries in the backing data structure are changed.
        /// </summary>
        /// <remarks>
        /// <para>When true, the cache listens for cluster-wide changes and invalidate entries accordingly.</para>
        /// <para>Changes to the local Hazelcast instance always invalidate the cache immediately.</para>
        /// </remarks>
        public bool InvalidateOnChange { get; set; } = true;

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder("NearCacheConfig{");
            return text.ToString();
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal NearCacheNamedOptions Clone() => new NearCacheNamedOptions(this);
    }
}
