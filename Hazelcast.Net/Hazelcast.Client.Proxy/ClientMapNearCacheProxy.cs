﻿// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.NearCache;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientMapNearCacheProxy<TKey, TValue> : ClientMapProxy<TKey, TValue>
    {
        private BaseNearCache _nearCache;

        public ClientMapNearCacheProxy(string serviceName, string name) : base(serviceName, name)
        {
        }

        internal BaseNearCache NearCache
        {
            get { return _nearCache; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            var nearCacheManager = GetContext().GetNearCacheManager();
            _nearCache = nearCacheManager.GetOrCreateNearCache(GetName());
        }

        protected override void PostDestroy()
        {
            try
            {
                GetContext().GetNearCacheManager().DestroyNearCache(GetName());
            }
            finally
            {
                base.PostDestroy();
            }
        }

        protected internal override void OnShutdown()
        {
            try
            {
                GetContext().GetNearCacheManager().DestroyNearCache(GetName());
            }
            finally
            {
                base.OnShutdown();
            }
        }

        protected override bool containsKeyInternal(IData keyData)
        {
            if (_nearCache.ContainsKey(keyData))
            {
                return true;
            }
            return base.containsKeyInternal(keyData);
        }

        protected override object GetInternal(IData keyData)
        {
            try
            {
                object value ;
                _nearCache.TryGetOrAdd(keyData, data => base.GetInternal(keyData), out value);
                return value;
            }
            catch (Exception exception)
            {
                _nearCache.Invalidate(keyData);
                throw ExceptionUtil.Rethrow(exception);
            }
        }

        protected override TValue RemoveInternal(IData keyData)
        {
            try
            {
                return base.RemoveInternal(keyData);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override bool RemoveInternal(IData keyData, object value)
        {
            try
            {
                return base.RemoveInternal(keyData, value);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override void DeleteInternal(IData keyData)
        {
            try
            {
                base.DeleteInternal(keyData);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override Task<TValue> PutAsyncInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            _nearCache.Invalidate(keyData);
            return base.PutAsyncInternal(keyData, value, ttl, timeunit);
        }

        protected override Task<TValue> RemoveAsyncInternal(IData keyData)
        {
            _nearCache.Invalidate(keyData);
            return base.RemoveAsyncInternal(keyData);
        }

        protected override bool TryRemoveInternal(IData keyData, long timeout, TimeUnit timeunit)
        {
            var response = base.TryRemoveInternal(keyData, timeout, timeunit);
            if (response)
            {
                _nearCache.Invalidate(keyData);
            }
            return response;
        }

        protected override bool TryPutInternal(IData keyData, TValue value, long timeout, TimeUnit timeunit)
        {
            var response = base.TryPutInternal(keyData, value, timeout, timeunit);
            if (response)
            {
                _nearCache.Invalidate(keyData);
            }
            return response;
        }

        protected override TValue PutInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            try
            {
                return base.PutInternal(keyData, value, ttl, timeunit);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override void PutTransientInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            try
            {
                base.PutTransientInternal(keyData, value, ttl, timeunit);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override TValue PutIfAbsentInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            try
            {
                return base.PutIfAbsentInternal(keyData, value, ttl, timeunit);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override bool ReplaceIfSameInternal(IData keyData, TValue oldValue, TValue newValue)
        {
            try
            {
                return base.ReplaceIfSameInternal(keyData, oldValue, newValue);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override void RemoveAllInternal(IPredicate predicate)
        {
            try
            {
                base.RemoveAllInternal(predicate);
            }
            finally
            {
                _nearCache.InvalidateAll();
            }
        }

        protected override void SetInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            try
            {
                base.SetInternal(keyData, value, ttl, timeunit);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override Task<object> SubmitToKeyInternal(IData keyData, IEntryProcessor entryProcessor)
        {
            try
            {
                return base.SubmitToKeyInternal(keyData, entryProcessor);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override object ExecuteOnKeyInternal(IData keyData, IEntryProcessor entryProcessor)
        {
            try
            {
                return base.ExecuteOnKeyInternal(keyData, entryProcessor);
            }
            finally
            {
                _nearCache.Invalidate(keyData);
            }
        }

        protected override void GetAllInternal(ArrayList partitionToKeyData, ConcurrentQueue<KeyValuePair<IData, object>> resultingKeyValuePairs)
        {
            Parallel.For(0, partitionToKeyData.Count, partitionId =>
            {
                var keyList = (ArrayList)partitionToKeyData[partitionId];
                for (int i = keyList.Count-1; i > -1; i--)
                {
                    var keyData = (IData)keyList[i];
                    object value;
                    if (_nearCache.TryGetValue(keyData, out value))
                    {
                        keyList.RemoveAt(i);
                        resultingKeyValuePairs.Enqueue(new KeyValuePair<IData, object>(keyData, value));
                    }
                }
            });
            base.GetAllInternal(partitionToKeyData, resultingKeyValuePairs);
            foreach (var kvp in resultingKeyValuePairs)
            {
                _nearCache.TryAdd(kvp.Key, kvp.Value);
            }
        }

        protected override void PutAllInternal(List<KeyValuePair<IData, IData>>[] partitions)
        {
            try
            {
                base.PutAllInternal(partitions);
            }
            finally
            {
                Parallel.For(0, partitions.Length, partitionId =>
                {
                    var entries = partitions[partitionId];
                    foreach (var kvp in entries)
                    {
                        _nearCache.Invalidate(kvp.Key);
                    }
                });
            }
        }
        
        public override Task<TValue> GetAsync(TKey key)
        {
            var task = GetContext().GetExecutionService().Submit(() => Get(key));
            return task;
        }

    }
}