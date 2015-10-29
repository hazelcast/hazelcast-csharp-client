/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

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
        private long cost;
        private long creationTime;
        private long evictionCriteriaNumber;
        private long expirationTime;
        private long hits;
        private object key;
        private long lastAccessTime;
        private long lastStoredTime;
        private long lastUpdateTime;
        private long ttl;
        private object value;
        private long version;

        public virtual long GetCost()
        {
            return cost;
        }

        public virtual long GetCreationTime()
        {
            return creationTime;
        }

        public long GetEvictionCriteriaNumber()
        {
            return evictionCriteriaNumber;
        }

        public virtual long GetExpirationTime()
        {
            return expirationTime;
        }

        public virtual long GetHits()
        {
            return hits;
        }

        public virtual object GetKey()
        {
            return key;
        }

        public virtual long GetLastAccessTime()
        {
            return lastAccessTime;
        }

        public virtual long GetLastStoredTime()
        {
            return lastStoredTime;
        }

        public virtual long GetLastUpdateTime()
        {
            return lastUpdateTime;
        }

        public long GetTtl()
        {
            return ttl;
        }

        public virtual object GetValue()
        {
            return value;
        }

        public virtual long GetVersion()
        {
            return version;
        }

        public virtual void SetCost(long cost)
        {
            this.cost = cost;
        }

        public virtual void SetCreationTime(long creationTime)
        {
            this.creationTime = creationTime;
        }

        public void SetEvictionCriteriaNumber(long evictionCriteriaNumber)
        {
            this.evictionCriteriaNumber = evictionCriteriaNumber;
        }

        public virtual void SetExpirationTime(long expirationTime)
        {
            this.expirationTime = expirationTime;
        }

        public virtual void SetHits(long hits)
        {
            this.hits = hits;
        }

        public virtual void SetKey(object key)
        {
            this.key = key;
        }

        public virtual void SetLastAccessTime(long lastAccessTime)
        {
            this.lastAccessTime = lastAccessTime;
        }

        public virtual void SetLastStoredTime(long lastStoredTime)
        {
            this.lastStoredTime = lastStoredTime;
        }

        public virtual void SetLastUpdateTime(long lastUpdateTime)
        {
            this.lastUpdateTime = lastUpdateTime;
        }

        public void SetTtl(long ttl)
        {
            this.ttl = ttl;
        }

        public virtual void SetValue(object value)
        {
            this.value = value;
        }

        public virtual void SetVersion(long version)
        {
            this.version = version;
        }
    }
}