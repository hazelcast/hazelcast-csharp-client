// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
    public class NearCacheConfig
    {
        public const int DefaultTtlSeconds = 0;
        public const int DefaultMaxIdleSeconds = 0;
        public const int DefaultMaxSize = int.MaxValue;
        public const string DefaultEvictionPolicy = "Lru";

        public static readonly InMemoryFormat DefaultMemoryFormat = InMemoryFormat.Binary;

        private bool _cacheLocalEntries;
        private string _evictionPolicy = DefaultEvictionPolicy;
        private InMemoryFormat _inMemoryFormat = DefaultMemoryFormat;
        private bool _invalidateOnChange = true;
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
            _cacheLocalEntries = config.IsCacheLocalEntries();
        }

        public NearCacheConfig()
        {
        }

        public NearCacheConfig(string name)
        {
            _name = name;
        }

        public virtual NearCacheConfigReadOnly GetAsReadOnly()
        {
            if (_readOnly == null)
            {
                _readOnly = new NearCacheConfigReadOnly(this);
            }
            return _readOnly;
        }

        public virtual string GetEvictionPolicy()
        {
            return _evictionPolicy;
        }

        public virtual InMemoryFormat GetInMemoryFormat()
        {
            return _inMemoryFormat;
        }

        public virtual int GetMaxIdleSeconds()
        {
            return _maxIdleSeconds;
        }

        public virtual int GetMaxSize()
        {
            return _maxSize;
        }

        public virtual string GetName()
        {
            return _name;
        }

        public virtual int GetTimeToLiveSeconds()
        {
            return _timeToLiveSeconds;
        }

        public virtual bool IsCacheLocalEntries()
        {
            return _cacheLocalEntries;
        }

        public virtual bool IsInvalidateOnChange()
        {
            return _invalidateOnChange;
        }

        public virtual NearCacheConfig SetCacheLocalEntries(bool cacheLocalEntries)
        {
            _cacheLocalEntries = cacheLocalEntries;
            return this;
        }

        public virtual NearCacheConfig SetEvictionPolicy(string evictionPolicy)
        {
            _evictionPolicy = evictionPolicy;
            return this;
        }

        public virtual NearCacheConfig SetInMemoryFormat(InMemoryFormat inMemoryFormat)
        {
            _inMemoryFormat = inMemoryFormat;
            return this;
        }

        // this setter is for reflection based configuration building
        public virtual NearCacheConfig SetInMemoryFormat(string inMemoryFormat)
        {
            Enum.TryParse(inMemoryFormat, true, out _inMemoryFormat);
            return this;
        }

        public virtual NearCacheConfig SetInvalidateOnChange(bool invalidateOnChange)
        {
            _invalidateOnChange = invalidateOnChange;
            return this;
        }

        public virtual NearCacheConfig SetMaxIdleSeconds(int maxIdleSeconds)
        {
            _maxIdleSeconds = maxIdleSeconds;
            return this;
        }

        public virtual NearCacheConfig SetMaxSize(int maxSize)
        {
            _maxSize = maxSize;
            return this;
        }

        public virtual NearCacheConfig SetName(string name)
        {
            _name = name;
            return this;
        }

        public virtual NearCacheConfig SetTimeToLiveSeconds(int timeToLiveSeconds)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("NearCacheConfig{");
            sb.Append("timeToLiveSeconds=").Append(_timeToLiveSeconds);
            sb.Append(", maxSize=").Append(_maxSize);
            sb.Append(", evictionPolicy='").Append(_evictionPolicy).Append('\'');
            sb.Append(", maxIdleSeconds=").Append(_maxIdleSeconds);
            sb.Append(", invalidateOnChange=").Append(_invalidateOnChange);
            sb.Append(", inMemoryFormat=").Append(_inMemoryFormat);
            sb.Append(", cacheLocalEntries=").Append(_cacheLocalEntries);
            sb.Append('}');
            return sb.ToString();
        }
    }
}