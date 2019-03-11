// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
        public const string DefaultEvictionPolicy = "Lru";

        public static readonly InMemoryFormat DefaultMemoryFormat = InMemoryFormat.Binary;

        private string _evictionPolicy = DefaultEvictionPolicy;
        private bool _cacheLocalEntries;
        private InMemoryFormat _inMemoryFormat = DefaultMemoryFormat;
        private bool _invalidateOnChange = true;
        private bool _serializeKeys = false;
        private int _maxIdleSeconds = DefaultMaxIdleSeconds;
        private int _maxSize = DefaultMaxSize;
        private string _name = "default";
        private NearCacheConfigReadOnly _readOnly;
        private int _timeToLiveSeconds = DefaultTtlSeconds;

        public NearCacheConfig(int timeToLiveSeconds, int maxSize, string evictionPolicy, int maxIdleSeconds,
            bool invalidateOnChange, InMemoryFormat inMemoryFormat)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
            _maxSize = maxSize;
            _evictionPolicy = evictionPolicy;
            _maxIdleSeconds = maxIdleSeconds;
            _invalidateOnChange = invalidateOnChange;
            _inMemoryFormat = inMemoryFormat;
        }

        public NearCacheConfig(NearCacheConfig config)
        {
            _name = config.GetName();
            _evictionPolicy = config.GetEvictionPolicy();
            _inMemoryFormat = config.GetInMemoryFormat();
            _invalidateOnChange = config.IsInvalidateOnChange();
            _maxIdleSeconds = config.GetMaxIdleSeconds();
            _maxSize = config.GetMaxSize();
            _timeToLiveSeconds = config.GetTimeToLiveSeconds();
        }

        public NearCacheConfig()
        {
        }

        public NearCacheConfig(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Returns an immutable version of this configuration.
        /// </summary>
        /// <returns><see cref="NearCacheConfigReadOnly"/></returns>
        public virtual NearCacheConfigReadOnly GetAsReadOnly()
        {
            if (_readOnly == null)
            {
                _readOnly = new NearCacheConfigReadOnly(this);
            }
            return _readOnly;
        }

        /// <summary>
        /// Returns the eviction policy for the Near Cache.
        /// </summary>
        /// <returns>eviction policy</returns>
        public virtual string GetEvictionPolicy()
        {
            return _evictionPolicy;
        }

        /// <summary>
        /// Returns <see cref="InMemoryFormat"/> 
        /// </summary>
        /// <returns><see cref="InMemoryFormat"/> </returns>
        public virtual InMemoryFormat GetInMemoryFormat()
        {
            return _inMemoryFormat;
        }

        /// <summary>
        /// Gets the maximum number of seconds each entry can stay in the Near Cache as untouched (not read).
        /// </summary>
        /// <remarks>
        /// Entries that are untouched more than <c>maxIdleSeconds</c> value will get removed from the Near Cache.
        /// </remarks>
        /// <returns>maximum number of seconds each entry can stay in the Near Cache as untouched</returns>
        public virtual int GetMaxIdleSeconds()
        {
            return _maxIdleSeconds;
        }

        /// <summary>
        /// Returns the maximum size of the Near Cache.<br/>
        /// When the maxSize is reached, the Near Cache is evicted based on the policy defined.
        /// </summary>
        /// <returns>the maximum size of the Near Cache</returns>
        public virtual int GetMaxSize()
        {
            return _maxSize;
        }

        /// <summary>
        /// Returns the name of the Near Cache
        /// </summary>
        /// <returns>the name of the Near Cache</returns>
        public virtual string GetName()
        {
            return _name;
        }

        /// <summary>
        /// Returns the maximum number of seconds for each entry to stay in the Near Cache (time to live).<br/>
        /// Entries that are older than <c>timeToLiveSeconds</c> will automatically be evicted from the Near Cache.
        /// </summary>
        /// <returns>the maximum number of seconds for each entry to stay in the Near Cache</returns>
        public virtual int GetTimeToLiveSeconds()
        {
            return _timeToLiveSeconds;
        }

        /// <summary>
        /// Checks if Near Cache entries are invalidated when the entries in the backing data structure are changed
        /// (updated or removed).
        /// <br/>
        /// When this setting is enabled, a Hazelcast instance with a Near Cache listens for cluster-wide changes
        /// on the entries of the backing data structure and invalidates its corresponding Near Cache entries.
        /// Changes done on the local Hazelcast instance always invalidate the Near Cache immediately.        
        /// </summary>
        /// <returns><c>true</c> if Near Cache invalidations are enabled on changes, <c>false</c> otherwise</returns>
        public virtual bool IsInvalidateOnChange()
        {
            return _invalidateOnChange;
        }

        public bool IsSerializeKeys()
        {
            return _serializeKeys;
        }

        public NearCacheConfig SetSerializeKeys(bool serializeKeys)
        {
            _serializeKeys = serializeKeys;
            return this;
        }

        /// <summary>
        /// Sets the eviction policy.
        /// Valid values are:
        /// <ul>
        /// <li><c>LRU</c> (Least Recently Used)</li>
        /// <li><c>LFU</c> (Least Frequently Used)</li>
        /// <li><c>NONE</c> (no extra eviction, time-to-live-seconds or max-idle-seconds may still apply)</li>
        /// <li><c>RANDOM</c> (random entry)</li>
        /// </ul>
        /// </summary>
        /// <param name="evictionPolicy">eviction policy.</param>
        /// <returns>this Near Cache config instance</returns>
        public virtual NearCacheConfig SetEvictionPolicy(string evictionPolicy)
        {
            _evictionPolicy = evictionPolicy;
            return this;
        }

        /// <summary>
        /// Sets the data type used to store entries.
        /// <br/>
        /// Possible values:
        /// <ul>
        /// <li><c>BINARY</c>: keys and values are stored as binary data</li>
        /// <li><c>OBJECT</c>: values are stored in their object forms</li>
        /// <li><c>NATIVE</c>: keys and values are stored in native memory</li>
        /// </ul>
        /// The default value is <c>BINARY</c>.
        /// </summary>
        /// <param name="inMemoryFormat">the data type used to store entries</param>
        /// <returns>this Near Cache config instance</returns>
        public virtual NearCacheConfig SetInMemoryFormat(InMemoryFormat inMemoryFormat)
        {
            _inMemoryFormat = inMemoryFormat;
            return this;
        }

        /// <summary>
        /// <see cref="SetInMemoryFormat(Hazelcast.Config.InMemoryFormat)"/>
        /// this setter is for reflection based configuration building
        /// </summary>
        public virtual NearCacheConfig SetInMemoryFormat(string inMemoryFormat)
        {
            Enum.TryParse(inMemoryFormat, true, out _inMemoryFormat);
            return this;
        }

        /// <summary>
        /// Sets if Near Cache entries are invalidated when the entries in the backing data structure are changed
        /// (updated or removed).
        /// <br/>
        /// When this setting is enabled, a Hazelcast instance with a Near Cache listens for cluster-wide changes
        /// on the entries of the backing data structure and invalidates its corresponding Near Cache entries.
        /// Changes done on the local Hazelcast instance always invalidate the Near Cache immediately.
        /// </summary>
        /// <param name="invalidateOnChange"></param>
        /// <returns>this Near Cache config instance</returns>
        public virtual NearCacheConfig SetInvalidateOnChange(bool invalidateOnChange)
        {
            _invalidateOnChange = invalidateOnChange;
            return this;
        }

        /// <summary>
        /// Set the maximum number of seconds each entry can stay in the Near Cache as untouched (not read).
        /// </summary>
        /// <remarks>
        /// Entries that are untouched (not read) more than <c>maxIdleSeconds</c> value will get removed from the Near Cache.
        /// <br/>
        /// Accepts any integer between <c>0</c> and <c>int.MaxValue</c>. The value <c>0</c> means <c>int.MaxValue</c>.
        /// The default is <c>0</c>.
        /// </remarks>
        /// <param name="maxIdleSeconds"></param>
        /// <returns>this Near Cache config instance</returns>
        public virtual NearCacheConfig SetMaxIdleSeconds(int maxIdleSeconds)
        {
            _maxIdleSeconds = maxIdleSeconds;
            return this;
        }
        /// <summary>
        /// Sets the maximum size of the Near Cache. 
        /// When the maxSize is reached, the Near Cache is evicted based on the policy defined.
        /// <br/>
        /// Accepts any integer between <c>0</c> and <c>int.MaxValue</c>. The value <c>0</c> means <c>int.MaxValue</c>.
        /// The default is <c>0</c>.
        /// </summary>
        /// <param name="maxSize">the maximum size of the Near Cache</param>
        /// <returns>this Near Cache config instance</returns>
        public virtual NearCacheConfig SetMaxSize(int maxSize)
        {
            _maxSize = maxSize;
            return this;
        }

        /// <summary>
        /// Sets the name of the Near Cache.
        /// </summary>
        /// <param name="name">the name of the Near Cache</param>
        /// <returns>this Near Cache config instance</returns>
        public virtual NearCacheConfig SetName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Returns the maximum number of seconds for each entry to stay in the Near Cache (time to live).
        /// <br/>
        /// Entries that are older than {@code timeToLiveSeconds} will automatically be evicted from the Near Cache.
        /// <br/>
        /// Accepts any integer between <c>0</c> and <c>int.MaxValue</c>. The value <c>0</c> means <c>int.MaxValue</c>.
        /// The default is <c>0</c>.
        /// </summary>
        /// <param name="timeToLiveSeconds">the maximum number of seconds for each entry to stay in the Near Cache</param>
        /// <returns>this Near Cache config instance</returns>
        public virtual NearCacheConfig SetTimeToLiveSeconds(int timeToLiveSeconds)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder("NearCacheConfig{");
            sb.Append("timeToLiveSeconds=").Append(_timeToLiveSeconds);
            sb.Append(", maxSize=").Append(_maxSize);
            sb.Append(", evictionPolicy='").Append(_evictionPolicy).Append('\'');
            sb.Append(", maxIdleSeconds=").Append(_maxIdleSeconds);
            sb.Append(", invalidateOnChange=").Append(_invalidateOnChange);
            sb.Append(", inMemoryFormat=").Append(_inMemoryFormat);
            return sb.ToString();
        }

#pragma warning disable CS1591
        [Obsolete("This configuration is not used on client")]
        public virtual bool IsCacheLocalEntries()
        {
            return _cacheLocalEntries;
        }

        [Obsolete("This configuration is not used on client")]
        public virtual NearCacheConfig SetCacheLocalEntries(bool cacheLocalEntries)
        {
            _cacheLocalEntries = cacheLocalEntries;
            return this;
        }
#pragma warning restore

    }
}