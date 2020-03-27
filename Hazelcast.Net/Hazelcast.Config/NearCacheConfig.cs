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
using System.Text;

namespace Hazelcast.Config
{
    /// <summary>
    /// Contains the configuration for a Near Cache.
    /// </summary>
    public class NearCacheConfig
    {
        public const int DefaultTtlSeconds = 0;
        public const int DefaultMaxIdleSeconds = 0;
        public const int DefaultMaxSize = int.MaxValue;
        public const EvictionPolicy DefaultEvictionPolicy = EvictionPolicy.Lru;

        public static readonly InMemoryFormat DefaultMemoryFormat = InMemoryFormat.Binary;

        public NearCacheConfig()
        {
        }

        public NearCacheConfig(string name)
        {
            Name = name;
        }


        /// <summary>
        /// Returns the eviction policy for the Near Cache.
        /// </summary>
        /// <value>eviction policy</value>
        public EvictionPolicy EvictionPolicy { get; set; } = DefaultEvictionPolicy;

        /// <summary>
        /// Returns <see cref="InMemoryFormat"/> 
        /// </summary>
        /// <value>
        ///     <see cref="InMemoryFormat"/>
        /// </value>
        public InMemoryFormat InMemoryFormat { get; set; } = DefaultMemoryFormat;

        /// <summary>
        /// Gets the maximum number of seconds each entry can stay in the Near Cache as untouched (not read).
        /// </summary>
        /// <remarks>
        /// Entries that are untouched more than <c>maxIdleSeconds</c> value will get removed from the Near Cache.
        /// </remarks>
        /// <value>maximum number of seconds each entry can stay in the Near Cache as untouched</value>
        public int MaxIdleSeconds { get; set; } = DefaultMaxIdleSeconds;

        /// <summary>
        /// Returns the maximum size of the Near Cache.<br/>
        /// When the maxSize is reached, the Near Cache is evicted based on the policy defined.
        /// </summary>
        /// <value>the maximum size of the Near Cache</value>
        public int MaxSize { get; set; } = DefaultMaxSize;

        /// <summary>
        /// Returns the name of the Near Cache
        /// </summary>
        /// <value>the name of the Near Cache</value>
        public string Name { get; set; } = "default";

        /// <summary>
        /// Returns the maximum number of seconds for each entry to stay in the Near Cache (time to live).<br/>
        /// Entries that are older than <c>timeToLiveSeconds</c> will automatically be evicted from the Near Cache.
        /// </summary>
        /// <value>the maximum number of seconds for each entry to stay in the Near Cache</value>
        public int TimeToLiveSeconds { get; set; } = DefaultTtlSeconds;

        /// <summary>
        /// Checks if Near Cache entries are invalidated when the entries in the backing data structure are changed
        /// (updated or removed).
        /// <br/>
        /// When this setting is enabled, a Hazelcast instance with a Near Cache listens for cluster-wide changes
        /// on the entries of the backing data structure and invalidates its corresponding Near Cache entries.
        /// Changes done on the local Hazelcast instance always invalidate the Near Cache immediately.        
        /// </summary>
        /// <value>
        ///     <c>true</c> if Near Cache invalidations are enabled on changes, <c>false</c> otherwise
        /// </value>
        public bool InvalidateOnChange { get; set; } = true;

        public bool SerializeKeys { get; set; } = false;

        // public virtual void SetInMemoryFormat(string inMemoryFormat)
        // {
        //     Enum.TryParse(inMemoryFormat, true, out _inMemoryFormat);
        // }


   
        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder("NearCacheConfig{");
            sb.Append("timeToLiveSeconds=").Append(TimeToLiveSeconds);
            sb.Append(", maxSize=").Append(MaxSize);
            sb.Append(", evictionPolicy='").Append(EvictionPolicy).Append('\'');
            sb.Append(", maxIdleSeconds=").Append(MaxIdleSeconds);
            sb.Append(", invalidateOnChange=").Append(InvalidateOnChange);
            sb.Append(", inMemoryFormat=").Append(InMemoryFormat);
            return sb.ToString();
        }
    }
}