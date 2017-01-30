// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal sealed class ClientMapProxy<TKey, TValue> : ClientProxy, IMap<TKey, TValue>
    {
        private readonly AtomicBoolean _nearCacheInitialized = new AtomicBoolean();
        private volatile ClientNearCache _nearCache;

        public ClientMapProxy(string serviceName, string name) : base(serviceName, name)
        {
        }

        internal ClientNearCache NearCache
        {
            get { return _nearCache; }
        }

        public bool ContainsKey(object key)
        {
            var keyData = ToData(key);
            if (_nearCache != null)
            {
                var cached = _nearCache.Get(keyData);
                if (cached != null)
                {
                    if (cached.Equals(ClientNearCache.NullObject))
                    {
                        return false;
                    }
                    return true;
                }
            }
            var request = MapContainsKeyCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            return Invoke(request, keyData, m => MapContainsKeyCodec.DecodeResponse(m).response);
        }

        public bool ContainsValue(object value)
        {
            var valueData = ToData(value);
            var request = MapContainsValueCodec.EncodeRequest(GetName(), valueData);
            var result = Invoke(request);
            return MapContainsValueCodec.DecodeResponse(result).response;
        }

        public TValue Get(object key)
        {
            var keyData = ToData(key);
            if (_nearCache != null)
            {
                var cached = _nearCache.Get(keyData);
                if (cached != null)
                {
                    if (cached.Equals(ClientNearCache.NullObject))
                    {
                        return default(TValue);
                    }
                    return (TValue) cached;
                }
            }
            var request = MapGetCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var result = Invoke(request, keyData);
            var value = ToObject<TValue>(MapGetCodec.DecodeResponse(result).response);
            if (_nearCache != null)
            {
                _nearCache.Put(keyData, value);
            }
            return value;
        }

        public TValue Put(TKey key, TValue value)
        {
            return Put(key, value, -1, TimeUnit.Seconds);
        }

        public TValue Remove(object key)
        {
            var keyData = ToData(key);
            var request = MapRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            return ToObject<TValue>(MapRemoveCodec.DecodeResponse(clientMessage).response);
        }

        public bool Remove(object key, object value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapRemoveIfSameCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            return MapRemoveIfSameCodec.DecodeResponse(clientMessage).response;
        }

        public void Delete(object key)
        {
            var keyData = ToData(key);
            var request = MapDeleteCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            Invoke(request, keyData);
        }

        public void Flush()
        {
            var request = MapFlushCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public Task<TValue> GetAsync(TKey key)
        {
            var keyData = ToData(key);
            if (_nearCache != null)
            {
                var cached = _nearCache.Get(keyData);
                if (cached != null)
                {
                    var task = GetContext().GetExecutionService().Submit(() =>
                    {
                        if (cached.Equals(ClientNearCache.NullObject))
                        {
                            return default(TValue);
                        }
                        return (TValue) cached;
                    });
                    return task;
                }
            }

            var request = MapGetCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            try
            {
                var task = GetContext().GetInvocationService().InvokeOnKeyOwner(request, key);
                var deserializeTask = task.ToTask().ContinueWith(continueTask =>
                {
                    var responseMessage = ThreadUtil.GetResult(continueTask);
                    var result = MapGetCodec.DecodeResponse(responseMessage).response;
                    if (_nearCache != null)
                    {
                        _nearCache.Put(keyData, result);
                    }
                    return ToObject<TValue>(result);
                });
                return deserializeTask;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public Task<TValue> PutAsync(TKey key, TValue value)
        {
            return PutAsync(key, value, -1, TimeUnit.Seconds);
        }

        public Task<TValue> PutAsync(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
            InvalidateNearCacheEntry(keyData);

            return InvokeAsync(request, keyData, m =>
            {
                var response = MapPutCodec.DecodeResponse(m).response;
                return ToObject<TValue>(response);
            });
        }

        public Task<TValue> RemoveAsync(TKey key)
        {
            var keyData = ToData(key);
            var request = MapRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            return InvokeAsync(request, keyData, m =>
            {
                var response = MapRemoveCodec.DecodeResponse(m).response;
                return ToObject<TValue>(response);
            });
        }

        public bool TryRemove(TKey key, long timeout, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var request = MapTryRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(timeout));
            var result = Invoke(request, keyData);
            var response = MapTryRemoveCodec.DecodeResponse(result).response;
            if (response) InvalidateNearCacheEntry(keyData);
            return response;
        }

        public bool TryPut(TKey key, TValue value, long timeout, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapTryPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(timeout));
            var result = Invoke(request, keyData);
            var response = MapTryPutCodec.DecodeResponse(result).response;
            if (response) InvalidateNearCacheEntry(keyData);
            return response;
        }

        public TValue Put(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            var response = MapPutCodec.DecodeResponse(clientMessage).response;
            return ToObject<TValue>(response);
        }

        public void PutTransient(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapPutTransientCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
            InvalidateNearCacheEntry(keyData);
            Invoke(request, keyData);
        }

        public TValue PutIfAbsent(TKey key, TValue value)
        {
            return PutIfAbsent(key, value, -1, TimeUnit.Seconds);
        }

        public TValue PutIfAbsent(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapPutIfAbsentCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
            var clientMessage = Invoke(request, keyData);
            var response = MapPutIfAbsentCodec.DecodeResponse(clientMessage).response;
            return ToObject<TValue>(response);
        }

        public bool Replace(TKey key, TValue oldValue, TValue newValue)
        {
            var keyData = ToData(key);
            var oldValueData = ToData(oldValue);
            var newValueData = ToData(newValue);
            var request = MapReplaceIfSameCodec.EncodeRequest(GetName(), keyData, oldValueData, newValueData,
                ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            return MapReplaceIfSameCodec.DecodeResponse(clientMessage).response;
        }

        public void RemoveAll(IPredicate predicate)
        {
            var request = MapRemoveAllCodec.EncodeRequest(GetName(), ToData(predicate));
            Invoke(request);
            if (_nearCache != null)
            {
                _nearCache.InvalidateAll();
            }
        }

        public TValue Replace(TKey key, TValue value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapReplaceCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            InvalidateNearCacheEntry(keyData);
            var clientMessage = Invoke(request, keyData);
            var response = MapReplaceCodec.DecodeResponse(clientMessage).response;
            return ToObject<TValue>(response);
        }

        public void Set(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = MapSetCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
            InvalidateNearCacheEntry(keyData);
            Invoke(request, keyData);
        }

        public void Lock(TKey key)
        {
            Lock(key, -1, TimeUnit.Milliseconds);
        }

        public void Lock(TKey key, long leaseTime, TimeUnit timeUnit)
        {
            var keyData = ToData(key);
            var request = MapLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                timeUnit.ToMillis(leaseTime));
            Invoke(request, keyData);
        }

        public bool IsLocked(TKey key)
        {
            var keyData = ToData(key);
            var request = MapIsLockedCodec.EncodeRequest(GetName(), keyData);
            var result = Invoke(request, keyData);
            return MapIsLockedCodec.DecodeResponse(result).response;
        }

        public Task<object> SubmitToKey(TKey key, IEntryProcessor entryProcessor)
        {
            ThrowExceptionIfNull(key);
            var keyData = ToData(key);
            InvalidateNearCacheEntry(keyData);
            var request = MapSubmitToKeyCodec.EncodeRequest(GetName(), ToData(entryProcessor), keyData,
                ThreadUtil.GetThreadId());
            var responseTask = InvokeAsync(request, keyData, m =>
            {
                var response = MapSubmitToKeyCodec.DecodeResponse(m).response;
                return ToObject<object>(response);
            });
            return responseTask;
        }

        public bool TryLock(TKey key)
        {
            try
            {
                return TryLock(key, 0, TimeUnit.Seconds);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <exception cref="System.Exception"></exception>
        public bool TryLock(TKey key, long time, TimeUnit timeunit)
        {
            return TryLock(key, time, timeunit, long.MaxValue, TimeUnit.Milliseconds);
        }

        /// <exception cref="System.Exception"></exception>
        public bool TryLock(TKey key, long time, TimeUnit timeunit, long leaseTime, TimeUnit leaseUnit)
        {
            var keyData = ToData(key);
            var request = MapTryLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                leaseUnit.ToMillis(leaseTime), timeunit.ToMillis(time));
            return Invoke(request, keyData, m => MapTryLockCodec.DecodeResponse(m).response);
        }

        public void Unlock(TKey key)
        {
            var keyData = ToData(key);
            var request = MapUnlockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            Invoke(request, keyData);
        }

        public void ForceUnlock(TKey key)
        {
            var keyData = ToData(key);
            var request = MapForceUnlockCodec.EncodeRequest(GetName(), keyData);
            Invoke(request, keyData);
        }

        public string AddInterceptor(IMapInterceptor interceptor)
        {
            var data = ToData(interceptor);
            var request = MapAddInterceptorCodec.EncodeRequest(GetName(), data);
            return Invoke(request, m => MapAddInterceptorCodec.DecodeResponse(m).response);
        }

        public void RemoveInterceptor(string id)
        {
            var request = MapRemoveInterceptorCodec.EncodeRequest(GetName(), id);
            Invoke(request);
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, bool includeValue)
        {
            var listenerFlags = GetListenerFlags(listener);
            var request = MapAddEntryListenerCodec.EncodeRequest(GetName(), includeValue, listenerFlags, false);
            DistributedEventHandler handler =
                eventData => MapAddEntryListenerCodec.AbstractEventHandler.Handle(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, includeValue, listener);
                    });

            return Listen(request, message => MapAddEntryListenerCodec.DecodeResponse(message).response, handler);
        }

        public bool RemoveEntryListener(string id)
        {
            return StopListening(s => MapRemoveEntryListenerCodec.EncodeRequest(GetName(), s),
                message => MapRemoveEntryListenerCodec.DecodeResponse(message).response, id);
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, TKey keyK, bool includeValue)
        {
            var keyData = ToData(keyK);
            var flags = GetListenerFlags(listener);
            var request = MapAddEntryListenerToKeyCodec.EncodeRequest(GetName(), keyData, includeValue, flags, false);
            DistributedEventHandler handler =
                eventData => MapAddEntryListenerToKeyCodec.AbstractEventHandler.Handle(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, includeValue, listener);
                    });

            return Listen(request, message => MapAddEntryListenerToKeyCodec.DecodeResponse(message).response, keyData,
                handler);
        }

        public IEntryView<TKey, TValue> GetEntryView(TKey key)
        {
            var keyData = ToData(key);
            var request = MapGetEntryViewCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var response = Invoke(request, keyData);
            var parameters = MapGetEntryViewCodec.DecodeResponse(response);
            var entryView = new SimpleEntryView<TKey, TValue>();
            var dataEntryView = parameters.dataEntryView;
            if (dataEntryView == null)
            {
                return null;
            }
            entryView.SetKey(ToObject<TKey>(dataEntryView.GetKey()));
            entryView.SetValue(ToObject<TValue>(dataEntryView.GetValue()));
            entryView.SetCost(dataEntryView.GetCost());
            entryView.SetCreationTime(dataEntryView.GetCreationTime());
            entryView.SetExpirationTime(dataEntryView.GetExpirationTime());
            entryView.SetHits(dataEntryView.GetHits());
            entryView.SetLastAccessTime(dataEntryView.GetLastAccessTime());
            entryView.SetLastStoredTime(dataEntryView.GetLastStoredTime());
            entryView.SetLastUpdateTime(dataEntryView.GetLastUpdateTime());
            entryView.SetVersion(dataEntryView.GetVersion());
            entryView.SetEvictionCriteriaNumber(dataEntryView.GetEvictionCriteriaNumber());
            entryView.SetTtl(dataEntryView.GetTtl());
            //TODO putCache
            return entryView;
        }

        public bool Evict(TKey key)
        {
            var keyData = ToData(key);
            var request = MapEvictCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var response = Invoke(request, keyData);
            var resultParameters = MapEvictCodec.DecodeResponse(response);
            return resultParameters.response;
        }

        public void EvictAll()
        {
            var request = MapEvictAllCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public object ExecuteOnKey(TKey key, IEntryProcessor entryProcessor)
        {
            ThrowExceptionIfNull(key);
            var keyData = ToData(key);
            InvalidateNearCacheEntry(keyData);
            var request = MapExecuteOnKeyCodec.EncodeRequest(GetName(), ToData(entryProcessor), keyData,
                ThreadUtil.GetThreadId());
            var response = Invoke(request, keyData);
            var resultParameters = MapExecuteOnKeyCodec.DecodeResponse(response);
            return ToObject<object>(resultParameters.response);
        }

        public IDictionary<TKey, object> ExecuteOnKeys(ISet<TKey> keys, IEntryProcessor entryProcessor)
        {
            if (keys != null && keys.Count == 0)
            {
                return new Dictionary<TKey, object>();
            }
            var dataList = ToDataList(keys);
            var request = MapExecuteOnKeysCodec.EncodeRequest(GetName(), ToData(entryProcessor), dataList);
            var response = Invoke(request);
            var resultParameters = MapExecuteOnKeysCodec.DecodeResponse(response);
            return DeserializeEntries<TKey>(resultParameters.response);
        }

        public IDictionary<TKey, object> ExecuteOnEntries(IEntryProcessor entryProcessor)
        {
            var request = MapExecuteOnAllKeysCodec.EncodeRequest(GetName(), ToData(entryProcessor));
            var response = Invoke(request);
            var resultParameters = MapExecuteOnAllKeysCodec.DecodeResponse(response);
            return DeserializeEntries<TKey>(resultParameters.response);
        }

        public IDictionary<TKey, object> ExecuteOnEntries(IEntryProcessor entryProcessor, IPredicate predicate)
        {
            var request = MapExecuteWithPredicateCodec.EncodeRequest(GetName(), ToData(entryProcessor), ToData(predicate));
            var response = Invoke(request);
            var resultParameters = MapExecuteWithPredicateCodec.DecodeResponse(response);
            return DeserializeEntries<TKey>(resultParameters.response);
        }

        public ISet<TKey> KeySet()
        {
            var request = MapKeySetCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => MapKeySetCodec.DecodeResponse(m).list);

            return ToSet<TKey>(result);
        }

        public IDictionary<TKey, TValue> GetAll(ICollection<TKey> keys)
        {
            var partitionToKeyData = GetPartitionKeyData(keys);

            var result = new Dictionary<TKey, TValue>();
            if (_nearCache != null)
            {
                // remove items from list which are already found in the cache
                foreach (var kvp in partitionToKeyData)
                {
                    var list = kvp.Value;
                    for (var i = list.Count - 1; i >= 0 ; i--)
                    {
                        var keyData = kvp.Value[i];
                        var cached = _nearCache.Get(keyData);
                        if (cached != null && cached != ClientNearCache.NullObject)
                        {
                            list.RemoveAt(i);
                            result.Add(ToObject<TKey>(keyData), (TValue)cached);
                        }
                    }
                }
            }

            var invocationService = GetContext().GetInvocationService();
            var futures = new List<IFuture<IClientMessage>>(partitionToKeyData.Count);
            foreach (var kvp in partitionToKeyData)
            {
                if (kvp.Value.Count > 0)
                {
                    var request = MapGetAllCodec.EncodeRequest(GetName(), kvp.Value);
                    futures.Add(invocationService.InvokeOnPartition(request, kvp.Key));
                }
            }
            var messages = ThreadUtil.GetResult(futures);
            foreach (var clientMessage in messages)
            {
                var items = MapGetAllCodec.DecodeResponse(clientMessage).entrySet;
                foreach (var entry in items)
                {
                    var key = ToObject<TKey>(entry.Key);
                    var value = ToObject<TValue>(entry.Value);
                    result.Add(key, value);
                    if (_nearCache != null)
                    {
                        _nearCache.Put(entry.Key, value);
                    }
                }
            }
            return result;
        }

        public ICollection<TValue> Values()
        {
            var request = MapValuesCodec.EncodeRequest(GetName());
            var list = Invoke(request, m => MapValuesCodec.DecodeResponse(m).list);
            ICollection<TValue> collection = new List<TValue>(list.Count);
            foreach (var data in list)
            {
                var value = ToObject<TValue>(data);
                collection.Add(value);
            }
            return collection;
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet()
        {
            var request = MapEntrySetCodec.EncodeRequest(GetName());
            var entries = Invoke(request, m => MapEntrySetCodec.DecodeResponse(m).entrySet);
            ISet<KeyValuePair<TKey, TValue>> entrySet = new HashSet<KeyValuePair<TKey, TValue>>();
            foreach (var entry in entries)
            {
                var key = ToObject<TKey>(entry.Key);
                var value = ToObject<TValue>(entry.Value);
                entrySet.Add(new KeyValuePair<TKey, TValue>(key, value));
            }
            return entrySet;
        }

        public void AddIndex(string attribute, bool ordered)
        {
            var request = MapAddIndexCodec.EncodeRequest(GetName(), attribute, ordered);
            Invoke(request);
        }

        public void Set(TKey key, TValue value)
        {
            Set(key, value, -1, TimeUnit.Seconds);
        }

        public int Size()
        {
            var request = MapSizeCodec.EncodeRequest(GetName());
            return Invoke(request, m => MapSizeCodec.DecodeResponse(m).response);
        }

        public bool IsEmpty()
        {
            var request = MapIsEmptyCodec.EncodeRequest(GetName());
            return Invoke(request, m => MapIsEmptyCodec.DecodeResponse(m).response);
        }

        public void PutAll(IDictionary<TKey, TValue> m)
        {
            var partitionService = GetContext().GetPartitionService();
            var partitions = new Dictionary<int, IDictionary<IData, IData>>(partitionService.GetPartitionCount());

            foreach (var kvp in m)
            {
                var keyData = ToData(kvp.Key);
                InvalidateNearCacheEntry(keyData);
                var partitionId = partitionService.GetPartitionId(keyData);
                IDictionary<IData, IData> partition;
                if (!partitions.TryGetValue(partitionId, out partition))
                {
                    partition = new Dictionary<IData, IData>();
                    partitions[partitionId] = partition;
                }
                partition[keyData] = ToData(kvp.Value);
            }

            var futures = new List<IFuture<IClientMessage>>(partitions.Count);
            foreach (var kvp in partitions)
            {
                var request = MapPutAllCodec.EncodeRequest(GetName(), kvp.Value);
                var future = GetContext().GetInvocationService().InvokeOnPartition(request, kvp.Key);
                futures.Add(future);
            }
            ThreadUtil.GetResult(futures);
        }

        public void Clear()
        {
            var request = MapClearCodec.EncodeRequest(GetName());
            Invoke(request);
            if (_nearCache != null)
            {
                _nearCache.InvalidateAll();
            }
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate,
            TKey key,
            bool includeValue)
        {
            var keyData = ToData(key);
            var predicateData = ToData(predicate);
            var flags = GetListenerFlags(listener);
            var request = MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(GetName(), keyData, predicateData,
                includeValue, flags, false);
            DistributedEventHandler handler =
                eventData => MapAddEntryListenerToKeyWithPredicateCodec.AbstractEventHandler.Handle(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, includeValue, listener);
                    });

            return Listen(request, message => MapAddEntryListenerCodec.DecodeResponse(message).response, keyData,
                handler);
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate,
            bool includeValue)
        {
            var predicateData = ToData(predicate);
            var flags = GetListenerFlags(listener);
            var request = MapAddEntryListenerWithPredicateCodec.EncodeRequest(GetName(), predicateData, includeValue,
                flags, false);
            DistributedEventHandler handler =
                eventData => MapAddEntryListenerWithPredicateCodec.AbstractEventHandler.Handle(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, includeValue, listener);
                    });

            return Listen(request, message => MapAddEntryListenerWithPredicateCodec.DecodeResponse(message).response,
                null,
                handler);
        }

        public ISet<TKey> KeySet(IPredicate predicate)
        {
            //TODO not supported yet
            //if (predicate is PagingPredicate)
            //{
            //    return KeySetWithPagingPredicate((PagingPredicate)predicate);
            //}
            var request = MapKeySetWithPredicateCodec.EncodeRequest(GetName(), ToData(predicate));
            var keys = Invoke(request, m => MapKeySetWithPredicateCodec.DecodeResponse(m).list);
            return ToSet<TKey>(keys);
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet(IPredicate predicate)
        {
            var request = MapEntriesWithPredicateCodec.EncodeRequest(GetName(), ToData(predicate));
            var entries = Invoke(request, m => MapEntriesWithPredicateCodec.DecodeResponse(m).entrySet);
            ISet<KeyValuePair<TKey, TValue>> entrySet = new HashSet<KeyValuePair<TKey, TValue>>();
            foreach (var dataEntry in entries)
            {
                var key = ToObject<TKey>(dataEntry.Key);
                var value = ToObject<TValue>(dataEntry.Value);
                entrySet.Add(new KeyValuePair<TKey, TValue>(key, value));
            }
            return entrySet;
        }

        public ICollection<TValue> Values(IPredicate predicate)
        {
            var request = MapValuesWithPredicateCodec.EncodeRequest(GetName(), ToData(predicate));
            var result = Invoke(request, m => MapValuesWithPredicateCodec.DecodeResponse(m).list);
            IList<TValue> values = new List<TValue>(result.Count);
            foreach (var data in result)
            {
                var value = ToObject<TValue>(data);
                values.Add(value);
            }
            return values;
        }

        public void OnEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValue,
            int eventTypeInt, string uuid,
            int numberOfAffectedEntries, bool includeValue, IEntryListener<TKey, TValue> listener)
        {
            var value = default(TValue);
            var oldValue = default(TValue);
            if (includeValue)
            {
                value = ToObject<TValue>(valueData);
                oldValue = ToObject<TValue>(oldValueData);
            }
            var key = ToObject<TKey>(keyData);
            var member = GetContext().GetClusterService().GetMember(uuid);
            var eventType = (EntryEventType) eventTypeInt;
            switch (eventType)
            {
                case EntryEventType.Added:
                {
                    listener.EntryAdded(new EntryEvent<TKey, TValue>(GetName(), member, eventType, key, oldValue, value));
                    break;
                }
                case EntryEventType.Removed:
                {
                    listener.EntryRemoved(new EntryEvent<TKey, TValue>(GetName(), member, eventType, key, oldValue,
                        value));
                    break;
                }
                case EntryEventType.Updated:
                {
                    listener.EntryUpdated(new EntryEvent<TKey, TValue>(GetName(), member, eventType, key, oldValue,
                        value));
                    break;
                }
                case EntryEventType.Evicted:
                {
                    listener.EntryEvicted(new EntryEvent<TKey, TValue>(GetName(), member, eventType, key, oldValue,
                        value));
                    break;
                }
                case EntryEventType.EvictAll:
                {
                    listener.MapEvicted(new MapEvent(GetName(), member, eventType, numberOfAffectedEntries));
                    break;
                }
                case EntryEventType.ClearAll:
                {
                    listener.MapCleared(new MapEvent(GetName(), member, eventType, numberOfAffectedEntries));
                    break;
                }
            }
        }

        protected override void OnDestroy()
        {
            if (_nearCache != null)
            {
                _nearCache.Destroy();
            }
        }

        internal override void PostInit()
        {
            if (_nearCacheInitialized.CompareAndSet(false, true))
            {
                var nearCacheConfig = GetContext().GetClientConfig().GetNearCacheConfig(GetName());
                if (nearCacheConfig == null)
                {
                    return;
                }
                _nearCache = new ClientNearCache(GetName(), ClientNearCacheType.Map, GetContext(), nearCacheConfig);
            }
        }

        private static int GetListenerFlags(IEntryListener<TKey, TValue> listener)
        {
            return (int) EntryEventType.All;
        }

        private void InvalidateNearCacheEntry(IData key)
        {
            if (_nearCache != null && _nearCache.InvalidateOnChange)
            {
                _nearCache.Invalidate(key);
            }
        }

        private Dictionary<int, IList<IData>> GetPartitionKeyData(ICollection<TKey> keys)
        {
            var partitionService = GetContext().GetPartitionService();

            // split the keys based on which partition they belong
            var partitionToKeyData = new Dictionary<int, IList<IData>>();
            foreach (object key in keys)
            {
                var keyData = ToData(key);
                var partitionId = partitionService.GetPartitionId(keyData);

                IList<IData> keyList = null;
                if (!partitionToKeyData.TryGetValue(partitionId, out keyList))
                {
                    partitionToKeyData[partitionId] = keyList = new List<IData>();
                }

                keyList.Add(keyData);
            }
            return partitionToKeyData;
        }
    }
}