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

namespace Hazelcast.Map
{
    internal class SimpleEntryView<TKey, TValue> : SimpleEntryView, IEntryView<TKey, TValue>
    {
        public new TKey GetKey()
        {
            return (TKey) base.GetKey();
        }

        public new TValue GetValue()
        {
            return (TValue) base.GetValue();
        }
    }

    internal class SimpleEntryView
    {
        private long _cost;
        private long _creationTime;
        private long _evictionCriteriaNumber;
        private long _expirationTime;
        private long _hits;
        private object _key;
        private long _lastAccessTime;
        private long _lastStoredTime;
        private long _lastUpdateTime;
        private long _ttl;
        private object _value;
        private long _version;

        public virtual long GetCost()
        {
            return _cost;
        }

        public virtual long GetCreationTime()
        {
            return _creationTime;
        }

        public long GetEvictionCriteriaNumber()
        {
            return _evictionCriteriaNumber;
        }

        public virtual long GetExpirationTime()
        {
            return _expirationTime;
        }

        public virtual long GetHits()
        {
            return _hits;
        }

        public virtual object GetKey()
        {
            return _key;
        }

        public virtual long GetLastAccessTime()
        {
            return _lastAccessTime;
        }

        public virtual long GetLastStoredTime()
        {
            return _lastStoredTime;
        }

        public virtual long GetLastUpdateTime()
        {
            return _lastUpdateTime;
        }

        public long GetTtl()
        {
            return _ttl;
        }

        public virtual object GetValue()
        {
            return _value;
        }

        public virtual long GetVersion()
        {
            return _version;
        }

        public virtual void SetCost(long cost)
        {
            _cost = cost;
        }

        public virtual void SetCreationTime(long creationTime)
        {
            _creationTime = creationTime;
        }

        public void SetEvictionCriteriaNumber(long evictionCriteriaNumber)
        {
            _evictionCriteriaNumber = evictionCriteriaNumber;
        }

        public virtual void SetExpirationTime(long expirationTime)
        {
            _expirationTime = expirationTime;
        }

        public virtual void SetHits(long hits)
        {
            _hits = hits;
        }

        public virtual void SetKey(object key)
        {
            _key = key;
        }

        public virtual void SetLastAccessTime(long lastAccessTime)
        {
            _lastAccessTime = lastAccessTime;
        }

        public virtual void SetLastStoredTime(long lastStoredTime)
        {
            _lastStoredTime = lastStoredTime;
        }

        public virtual void SetLastUpdateTime(long lastUpdateTime)
        {
            _lastUpdateTime = lastUpdateTime;
        }

        public void SetTtl(long ttl)
        {
            _ttl = ttl;
        }

        public virtual void SetValue(object value)
        {
            _value = value;
        }

        public virtual void SetVersion(long version)
        {
            _version = version;
        }
    }
}