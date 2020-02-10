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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Util;
using static Hazelcast.Util.IndexUtil;
using static Hazelcast.Util.ValidationUtil;
using static Hazelcast.Util.ThreadUtil;
using static Hazelcast.Util.ExceptionUtil;
using static Hazelcast.Util.SortingUtil;

namespace Hazelcast.Client.Proxy
{
    internal class ClientMapProxy<TKey, TValue> : ClientProxy, IMap<TKey, TValue>
    {
        private ClientLockReferenceIdGenerator _lockReferenceIdGenerator;

        public ClientMapProxy(string serviceName, string name, HazelcastClient client) : base(serviceName, name, client)
        {
        }

        public TResult Aggregate<TResult>(IAggregator<TResult> aggregator)
        {
            IsNotNull(aggregator, "aggregator");
            var request = MapAggregateCodec.EncodeRequest(Name, ToData(aggregator));
            var response = Invoke(request);
            var resultParameters = MapAggregateCodec.DecodeResponse(response);
            return ToObject<TResult>(resultParameters.Response);
        }

        public TResult Aggregate<TResult>(IAggregator<TResult> aggregator, IPredicate predicate)
        {
            IsNotNull(aggregator, "aggregator");
            IsNotNull(predicate, "predicate");
            var request = MapAggregateWithPredicateCodec.EncodeRequest(Name, ToData(aggregator), ToData(predicate));
            var response = Invoke(request);
            var resultParameters = MapAggregateWithPredicateCodec.DecodeResponse(response);
            return ToObject<TResult>(resultParameters.Response);
        }

        public ICollection<TResult> Project<TResult>(IProjection projection)
        {
            IsNotNull(projection, "projection");
            var request = MapProjectCodec.EncodeRequest(Name, ToData(projection));
            var response = Invoke(request);
            var resultParameters = MapProjectCodec.DecodeResponse(response);
            return new ReadOnlyLazyList<TResult, IData>(resultParameters.Response, Client.SerializationService);
        }

        public ICollection<TResult> Project<TResult>(IProjection projection, IPredicate predicate)
        {
            IsNotNull(projection, "projection");
            IsNotNull(predicate, "predicate");
            var request = MapProjectWithPredicateCodec.EncodeRequest(Name, ToData(projection), ToData(predicate));
            var response = Invoke(request);
            var resultParameters = MapProjectWithPredicateCodec.DecodeResponse(response);
            return new ReadOnlyLazyList<TResult, IData>(resultParameters.Response, Client.SerializationService);
        }

        public bool ContainsKey(object key)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            var keyData = ToData(key);
            return containsKeyInternal(keyData);
        }

        protected virtual bool containsKeyInternal(IData keyData)
        {
            var request = MapContainsKeyCodec.EncodeRequest(Name, keyData, GetThreadId());
            return Invoke(request, keyData, m => MapContainsKeyCodec.DecodeResponse(m).Response);
        }

        public bool ContainsValue(object value)
        {
            var valueData = ToData(value);
            var request = MapContainsValueCodec.EncodeRequest(Name, valueData);
            var result = Invoke(request);
            return MapContainsValueCodec.DecodeResponse(result).Response;
        }

        public TValue Get(object key)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            var keyData = ToData(key);
            return ToObject<TValue>(GetInternal(keyData));
        }

        protected virtual object GetInternal(IData keyData)
        {
            var request = MapGetCodec.EncodeRequest(Name, keyData, GetThreadId());
            var result = Invoke(request, keyData);
            return MapGetCodec.DecodeResponse(result).Response;
        }

        public TValue Put(TKey key, TValue value)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(value, NullValueIsNotAllowed);

            return Put(key, value, -1, TimeUnit.Seconds);
        }

        public TValue Remove(object key)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            var keyData = ToData(key);
            return RemoveInternal(keyData);
        }

        protected virtual TValue RemoveInternal(IData keyData)
        {
            var request = MapRemoveCodec.EncodeRequest(Name, keyData, GetThreadId());
            var clientMessage = Invoke(request, keyData);
            return ToObject<TValue>(MapRemoveCodec.DecodeResponse(clientMessage).Response);
        }

        public bool Remove(object key, object value)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(value, NullValueIsNotAllowed);
            var keyData = ToData(key);
            return RemoveInternal(keyData, value);
        }

        protected virtual bool RemoveInternal(IData keyData, object value)
        {
            var valueData = ToData(value);
            var request = MapRemoveIfSameCodec.EncodeRequest(Name, keyData, valueData, GetThreadId());
            var clientMessage = Invoke(request, keyData);
            return MapRemoveIfSameCodec.DecodeResponse(clientMessage).Response;
        }

        public void Delete(object key)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            var keyData = ToData(key);
            DeleteInternal(keyData);
        }

        protected virtual void DeleteInternal(IData keyData)
        {
            var request = MapDeleteCodec.EncodeRequest(Name, keyData, GetThreadId());
            Invoke(request, keyData);
        }

        public void Flush()
        {
            var request = MapFlushCodec.EncodeRequest(Name);
            Invoke(request);
        }

        public virtual Task<TValue> GetAsync(TKey key)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            var keyData = ToData(key);
            var request = MapGetCodec.EncodeRequest(Name, keyData, GetThreadId());
            return InvokeAsync(request, keyData, m =>
            {
                var resp = MapGetCodec.DecodeResponse(m).Response;
                return ToObject<TValue>(resp);
            });
        }

        public Task<TValue> PutAsync(TKey key, TValue value)
        {
            return PutAsync(key, value, -1, TimeUnit.Seconds);
        }

        public Task<TValue> PutAsync(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(value, NullValueIsNotAllowed);
            var keyData = ToData(key);
            return PutAsyncInternal(keyData, value, ttl, timeunit);
        }

        protected virtual Task<TValue> PutAsyncInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request = MapPutCodec.EncodeRequest(Name, keyData, valueData, GetThreadId(), timeunit.ToMillis(ttl));
            return InvokeAsync(request, keyData, m =>
            {
                var response = MapPutCodec.DecodeResponse(m).Response;
                return ToObject<TValue>(response);
            });
        }

        public Task<TValue> RemoveAsync(TKey key)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            var keyData = ToData(key);
            return RemoveAsyncInternal(keyData);
        }

        protected virtual Task<TValue> RemoveAsyncInternal(IData keyData)
        {
            var request = MapRemoveCodec.EncodeRequest(Name, keyData, GetThreadId());
            return InvokeAsync(request, keyData, m =>
            {
                var response = MapRemoveCodec.DecodeResponse(m).Response;
                return ToObject<TValue>(response);
            });
        }

        public bool TryRemove(TKey key, long timeout, TimeUnit timeunit)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            var keyData = ToData(key);
            return TryRemoveInternal(keyData, timeout, timeunit);
        }

        protected virtual bool TryRemoveInternal(IData keyData, long timeout, TimeUnit timeunit)
        {
            var request = MapTryRemoveCodec.EncodeRequest(Name, keyData, GetThreadId(), timeunit.ToMillis(timeout));
            var result = Invoke(request, keyData);
            var response = MapTryRemoveCodec.DecodeResponse(result).Response;
            return response;
        }

        public bool TryPut(TKey key, TValue value, long timeout, TimeUnit timeunit)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(value, NullValueIsNotAllowed);
            var keyData = ToData(key);
            return TryPutInternal(keyData, value, timeout, timeunit);
        }

        protected virtual bool TryPutInternal(IData keyData, TValue value, long timeout, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request = MapTryPutCodec.EncodeRequest(Name, keyData, valueData, GetThreadId(), timeunit.ToMillis(timeout));
            var result = Invoke(request, keyData);
            var response = MapTryPutCodec.DecodeResponse(result).Response;
            return response;
        }

        public TValue Put(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(value, NullValueIsNotAllowed);
            var keyData = ToData(key);

            return PutInternal(keyData, value, ttl, timeunit);
        }

        protected virtual TValue PutInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request = MapPutCodec.EncodeRequest(Name, keyData, valueData, GetThreadId(), timeunit.ToMillis(ttl));
            var clientMessage = Invoke(request, keyData);
            var response = MapPutCodec.DecodeResponse(clientMessage).Response;
            return ToObject<TValue>(response);
        }

        public void PutTransient(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(value, NullValueIsNotAllowed);
            var keyData = ToData(key);
            PutTransientInternal(keyData, value, ttl, timeunit);
        }

        protected virtual void PutTransientInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request = MapPutTransientCodec.EncodeRequest(Name, keyData, valueData, GetThreadId(), timeunit.ToMillis(ttl));
            Invoke(request, keyData);
        }

        public TValue PutIfAbsent(TKey key, TValue value)
        {
            return PutIfAbsent(key, value, -1, TimeUnit.Seconds);
        }

        public TValue PutIfAbsent(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(value, NullValueIsNotAllowed);
            var keyData = ToData(key);
            return PutIfAbsentInternal(keyData, value, ttl, timeunit);
        }

        protected virtual TValue PutIfAbsentInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request = MapPutIfAbsentCodec.EncodeRequest(Name, keyData, valueData, GetThreadId(), timeunit.ToMillis(ttl));
            var clientMessage = Invoke(request, keyData);
            var response = MapPutIfAbsentCodec.DecodeResponse(clientMessage).Response;
            return ToObject<TValue>(response);
        }

        public bool Replace(TKey key, TValue oldValue, TValue newValue)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(oldValue, NullValueIsNotAllowed);
            CheckNotNull(newValue, NullValueIsNotAllowed);
            var keyData = ToData(key);
            return ReplaceIfSameInternal(keyData, oldValue, newValue);
        }

        protected virtual bool ReplaceIfSameInternal(IData keyData, TValue oldValue, TValue newValue)
        {
            var oldValueData = ToData(oldValue);
            var newValueData = ToData(newValue);
            var request = MapReplaceIfSameCodec.EncodeRequest(Name, keyData, oldValueData, newValueData, GetThreadId());
            var clientMessage = Invoke(request, keyData);
            return MapReplaceIfSameCodec.DecodeResponse(clientMessage).Response;
        }

        public void RemoveAll(IPredicate predicate)
        {
            CheckNotNull(predicate, "predicate cannot be null");
            RemoveAllInternal(predicate);
        }

        protected virtual void RemoveAllInternal(IPredicate predicate)
        {
            var request = MapRemoveAllCodec.EncodeRequest(Name, ToData(predicate));
            Invoke(request);
        }

        public TValue Replace(TKey key, TValue value)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(value, NullValueIsNotAllowed);
            var keyData = ToData(key);
            return ReplaceInternal(keyData, value);
        }

        public TValue ReplaceInternal(IData keyData, TValue value)
        {
            var valueData = ToData(value);
            var request = MapReplaceCodec.EncodeRequest(Name, keyData, valueData, GetThreadId());
            var clientMessage = Invoke(request, keyData);
            var response = MapReplaceCodec.DecodeResponse(clientMessage).Response;
            return ToObject<TValue>(response);
        }

        public void Set(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            CheckNotNull(key, NullKeyIsNotAllowed);
            CheckNotNull(value, NullValueIsNotAllowed);
            var keyData = ToData(key);
            SetInternal(keyData, value, ttl, timeunit);
        }

        protected virtual void SetInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request = MapSetCodec.EncodeRequest(Name, keyData, valueData, GetThreadId(), timeunit.ToMillis(ttl));
            Invoke(request, keyData);
        }

        public void Lock(TKey key)
        {
            Lock(key, -1, TimeUnit.Milliseconds);
        }

        public void Lock(TKey key, long leaseTime, TimeUnit timeUnit)
        {
            var keyData = ToData(key);
            var request = MapLockCodec.EncodeRequest(Name, keyData, GetThreadId(), timeUnit.ToMillis(leaseTime),
                _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public bool IsLocked(TKey key)
        {
            var keyData = ToData(key);
            var request = MapIsLockedCodec.EncodeRequest(Name, keyData);
            var result = Invoke(request, keyData);
            return MapIsLockedCodec.DecodeResponse(result).Response;
        }

        public Task<object> SubmitToKey(TKey key, IEntryProcessor entryProcessor)
        {
            ThrowExceptionIfNull(key);
            var keyData = ToData(key);
            return SubmitToKeyInternal(keyData, entryProcessor);
        }

        protected virtual Task<object> SubmitToKeyInternal(IData keyData, IEntryProcessor entryProcessor)
        {
            var request = MapSubmitToKeyCodec.EncodeRequest(Name, ToData(entryProcessor), keyData, GetThreadId());
            var responseTask = InvokeAsync(request, keyData, m =>
            {
                var response = MapSubmitToKeyCodec.DecodeResponse(m).Response;
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

        public bool TryLock(TKey key, long time, TimeUnit timeunit)
        {
            return TryLock(key, time, timeunit, long.MaxValue, TimeUnit.Milliseconds);
        }

        public bool TryLock(TKey key, long time, TimeUnit timeunit, long leaseTime, TimeUnit leaseUnit)
        {
            var keyData = ToData(key);
            var request = MapTryLockCodec.EncodeRequest(Name, keyData, GetThreadId(), leaseUnit.ToMillis(leaseTime),
                timeunit.ToMillis(time), _lockReferenceIdGenerator.GetNextReferenceId());
            return Invoke(request, keyData, m => MapTryLockCodec.DecodeResponse(m).Response);
        }

        public void Unlock(TKey key)
        {
            var keyData = ToData(key);
            var request = MapUnlockCodec.EncodeRequest(Name, keyData, GetThreadId(),
                _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public void ForceUnlock(TKey key)
        {
            var keyData = ToData(key);
            var request = MapForceUnlockCodec.EncodeRequest(Name, keyData, _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public string AddInterceptor(IMapInterceptor interceptor)
        {
            var data = ToData(interceptor);
            var request = MapAddInterceptorCodec.EncodeRequest(Name, data);
            return Invoke(request, m => MapAddInterceptorCodec.DecodeResponse(m).Response);
        }

        public void RemoveInterceptor(string id)
        {
            var request = MapRemoveInterceptorCodec.EncodeRequest(Name, id);
            Invoke(request);
        }

        public Guid AddEntryListener(IEntryListener<TKey, TValue> listener, TKey key, bool includeValue)
        {
            return AddEntryListener((MapListener) listener, key, includeValue);
        }

        public Guid AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate, TKey key, bool includeValue)
        {
            return AddEntryListener((MapListener) listener, predicate, key, includeValue);
        }

        public Guid AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate, bool includeValue)
        {
            return AddEntryListener((MapListener) listener, predicate, includeValue);
        }

        public Guid AddEntryListener(IEntryListener<TKey, TValue> listener, bool includeValue)
        {
            return AddEntryListener((MapListener) listener, includeValue);
        }

        public Guid AddEntryListener(MapListener listener, bool includeValue)
        {
            var listenerAdapter = EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var listenerFlags = (int) listenerAdapter.ListenerFlags;
            var request = MapAddEntryListenerCodec.EncodeRequest(Name, includeValue, listenerFlags, IsSmart());
            DistributedEventHandler handler = eventData => MapAddEntryListenerCodec.EventHandler.HandleEvent(eventData,
                (key, value, oldValue, mergingValue, type, uuid, entries) =>
                {
                    OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                });

            return RegisterListener(request, message => MapAddEntryListenerCodec.DecodeResponse(message).Response,
                id => MapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public Guid AddEntryListener(MapListener listener, TKey key, bool includeValue)
        {
            var keyData = ToData(key);
            var listenerAdapter = EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var listenerFlags = (int) listenerAdapter.ListenerFlags;
            var request = MapAddEntryListenerToKeyCodec.EncodeRequest(Name, keyData, includeValue, listenerFlags, IsSmart());
            DistributedEventHandler handler = eventData => MapAddEntryListenerToKeyCodec.EventHandler.HandleEvent(eventData,
                (key_, value, oldValue, mergingValue, type, uuid, entries) =>
                {
                    OnEntryEvent(key_, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                });

            return RegisterListener(request, message => MapAddEntryListenerToKeyCodec.DecodeResponse(message).Response,
                id => MapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public Guid AddEntryListener(MapListener listener, IPredicate predicate, TKey key, bool includeValue)
        {
            var keyData = ToData(key);
            var predicateData = ToData(predicate);
            var listenerAdapter = EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var listenerFlags = (int) listenerAdapter.ListenerFlags;
            var request = MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(Name, keyData, predicateData, includeValue,
                listenerFlags, IsSmart());
            DistributedEventHandler handler = eventData =>
                MapAddEntryListenerToKeyWithPredicateCodec.EventHandler.HandleEvent(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request, message => MapAddEntryListenerCodec.DecodeResponse(message).Response,
                id => MapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public Guid AddEntryListener(MapListener listener, IPredicate predicate, bool includeValue)
        {
            var predicateData = ToData(predicate);
            var listenerAdapter = EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, Client.SerializationService);
            var listenerFlags = (int) listenerAdapter.ListenerFlags;
            var request =
                MapAddEntryListenerWithPredicateCodec.EncodeRequest(Name, predicateData, includeValue, listenerFlags, IsSmart());
            DistributedEventHandler handler = eventData =>
                MapAddEntryListenerWithPredicateCodec.EventHandler.HandleEvent(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });

            return RegisterListener(request, message => MapAddEntryListenerWithPredicateCodec.DecodeResponse(message).Response,
                id => MapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
        }

        public bool RemoveEntryListener(Guid registrationId)
        {
            return DeregisterListener(registrationId);
        }

        public IEntryView<TKey, TValue> GetEntryView(TKey key)
        {
            var keyData = ToData(key);
            var request = MapGetEntryViewCodec.EncodeRequest(Name, keyData, GetThreadId());
            var response = Invoke(request, keyData);
            var parameters = MapGetEntryViewCodec.DecodeResponse(response);
            var entryView = new SimpleEntryView<TKey, TValue>();
            var dataEntryView = parameters.Response;
            if (dataEntryView == null)
            {
                return null;
            }
            entryView.Key = ToObject<TKey>(((SimpleEntryView) dataEntryView).Key);
            entryView.Value = ToObject<TValue>(((SimpleEntryView) dataEntryView).Value);
            entryView.Cost = dataEntryView.Cost;
            entryView.CreationTime = dataEntryView.CreationTime;
            entryView.ExpirationTime = dataEntryView.ExpirationTime;
            entryView.Hits = dataEntryView.Hits;
            entryView.LastAccessTime = dataEntryView.LastAccessTime;
            entryView.LastStoredTime = dataEntryView.LastStoredTime;
            entryView.LastUpdateTime = dataEntryView.LastUpdateTime;
            entryView.Version = dataEntryView.Version;
            entryView.EvictionCriteriaNumber = dataEntryView.EvictionCriteriaNumber;
            entryView.Ttl = dataEntryView.Ttl;
            //TODO putCache
            return entryView;
        }

        public bool Evict(TKey key)
        {
            var keyData = ToData(key);
            var request = MapEvictCodec.EncodeRequest(Name, keyData, GetThreadId());
            var response = Invoke(request, keyData);
            var resultParameters = MapEvictCodec.DecodeResponse(response);
            return resultParameters.Response;
        }

        public void EvictAll()
        {
            var request = MapEvictAllCodec.EncodeRequest(Name);
            Invoke(request);
        }

        public object ExecuteOnKey(TKey key, IEntryProcessor entryProcessor)
        {
            ThrowExceptionIfNull(key);
            var keyData = ToData(key);
            return ExecuteOnKeyInternal(keyData, entryProcessor);
        }

        protected virtual object ExecuteOnKeyInternal(IData keyData, IEntryProcessor entryProcessor)
        {
            var request = MapExecuteOnKeyCodec.EncodeRequest(Name, ToData(entryProcessor), keyData, GetThreadId());
            var response = Invoke(request, keyData);
            var resultParameters = MapExecuteOnKeyCodec.DecodeResponse(response);
            return ToObject<object>(resultParameters.Response);
        }

        public IDictionary<TKey, object> ExecuteOnKeys(ISet<TKey> keys, IEntryProcessor entryProcessor)
        {
            if (keys != null && keys.Count == 0)
            {
                return new Dictionary<TKey, object>();
            }
            var dataList = ToDataList(keys);
            var request = MapExecuteOnKeysCodec.EncodeRequest(Name, ToData(entryProcessor), dataList);
            var response = Invoke(request);
            var resultParameters = MapExecuteOnKeysCodec.DecodeResponse(response);
            return DeserializeEntries<TKey>(resultParameters.Response);
        }

        public IDictionary<TKey, object> ExecuteOnEntries(IEntryProcessor entryProcessor)
        {
            var request = MapExecuteOnAllKeysCodec.EncodeRequest(Name, ToData(entryProcessor));
            var response = Invoke(request);
            var resultParameters = MapExecuteOnAllKeysCodec.DecodeResponse(response);
            return DeserializeEntries<TKey>(resultParameters.Response);
        }

        public IDictionary<TKey, object> ExecuteOnEntries(IEntryProcessor entryProcessor, IPredicate predicate)
        {
            var request = MapExecuteWithPredicateCodec.EncodeRequest(Name, ToData(entryProcessor), ToData(predicate));
            var response = Invoke(request);
            var resultParameters = MapExecuteWithPredicateCodec.DecodeResponse(response);
            return DeserializeEntries<TKey>(resultParameters.Response);
        }

        public ISet<TKey> KeySet()
        {
            var request = MapKeySetCodec.EncodeRequest(Name);
            var result = Invoke(request, m => MapKeySetCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazySet<TKey>(result, Client.SerializationService);
        }

        public IDictionary<TKey, TValue> GetAll(ICollection<TKey> keys)
        {
            if (keys == null || keys.Count == 0) return new Dictionary<TKey, TValue>();

            var resultingKeyValuePairs = new List<KeyValuePair<IData, object>>();
            try
            {
                var partitionToKeyData = GetPartitionKeyData(keys);
                GetAllInternal(partitionToKeyData, resultingKeyValuePairs);
            }
            catch (Exception e)
            {
                Rethrow(e);
            }
            return new ReadOnlyLazyDictionary<TKey, TValue, object>(resultingKeyValuePairs, Client.SerializationService);
        }

        protected virtual void GetAllInternal(List<List<IData>> partitionToKeyData,
            List<KeyValuePair<IData, object>> resultingKeyValuePairs)
        {
            var invocationService = Client.InvocationService;
            var futures = new List<IFuture<ClientMessage>>();
            for (var partitionId = 0; partitionId < partitionToKeyData.Count; partitionId++)
            {
                ICollection<IData> keyList = partitionToKeyData[partitionId];
                if (keyList.Count > 0)
                {
                    var request = MapGetAllCodec.EncodeRequest(Name, keyList);
                    futures.Add(invocationService.InvokeOnPartitionOwner(request, partitionId));
                }
            }
            foreach (var future in futures)
            {
                foreach (var kvp in MapGetAllCodec.DecodeResponse(future.Result).Response)
                {
                    resultingKeyValuePairs.Add(new KeyValuePair<IData, object>(kvp.Key, kvp.Value));
                }
            }
        }

        private List<List<IData>> GetPartitionKeyData(ICollection<TKey> keys)
        {
            var partitionService = Client.PartitionService;
            var partitionCount = partitionService.GetPartitionCount();
            var initialCapacity = 2 * (keys.Count / partitionCount);
            var partitionToKeyData = new List<List<IData>>(partitionCount);
            for (var i = 0; i < partitionCount; i++)
            {
                partitionToKeyData.Add(new List<IData>(initialCapacity));
            }
            foreach (var key in keys)
            {
                CheckNotNull(key, NullKeyIsNotAllowed);
                var keyData = ToData(key);
                var partitionId = partitionService.GetPartitionId(keyData);
                var keyList = partitionToKeyData[partitionId];
                keyList.Add(keyData);
            }
            return partitionToKeyData;
        }

        public ICollection<TValue> Values()
        {
            var request = MapValuesCodec.EncodeRequest(Name);
            var list = Invoke(request, m => MapValuesCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazyList<TValue, IData>(list, Client.SerializationService);
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet()
        {
            var request = MapEntrySetCodec.EncodeRequest(Name);
            var response = Invoke(request);

            var result = new List<KeyValuePair<IData, object>>();
            foreach (var kvp in MapEntrySetCodec.DecodeResponse(response).Response)
            {
                result.Add(new KeyValuePair<IData, object>(kvp.Key, kvp.Value));
            }
            return new ReadOnlyLazyEntrySet<TKey, TValue, object>(result, Client.SerializationService);
        }

        public void AddIndex(IndexType indexType, params string[] attributes)
        {
            CheckNotNull(attributes, "Index attributes cannot be null.");
            var indexConfig = new IndexConfig {Type = indexType, Attributes = attributes};
            AddIndex(indexConfig);
        }

        public void AddIndex(IndexConfig indexConfig)
        {
            CheckNotNull(indexConfig, "Index config cannot be null.");
            var indexConfig0 = ValidateAndNormalize(Name, indexConfig);
            var request = MapAddIndexCodec.EncodeRequest(Name, indexConfig0);
            Invoke(request);
        }

        public void Set(TKey key, TValue value)
        {
            Set(key, value, -1, TimeUnit.Seconds);
        }

        public int Size()
        {
            var request = MapSizeCodec.EncodeRequest(Name);
            return Invoke(request, m => MapSizeCodec.DecodeResponse(m).Response);
        }

        public bool IsEmpty()
        {
            var request = MapIsEmptyCodec.EncodeRequest(Name);
            return Invoke(request, m => MapIsEmptyCodec.DecodeResponse(m).Response);
        }

        public void PutAll(IDictionary<TKey, TValue> m)
        {
            var partitionService = Client.PartitionService;
            var partitionCount = partitionService.GetPartitionCount();
            var partitions = new List<KeyValuePair<IData, IData>>[partitionCount];

            for (var i = 0; i < partitionCount; i++)
            {
                partitions[i] = new List<KeyValuePair<IData, IData>>();
            }

            foreach (var kvp in m)
            {
                CheckNotNull(kvp.Key, NullKeyIsNotAllowed);
                CheckNotNull(kvp.Value, NullValueIsNotAllowed);
                var keyData = ToData(kvp.Key);
                var valueData = ToData(kvp.Value);
                var partitionId = partitionService.GetPartitionId(keyData);
                var partition = partitions[partitionId];
                partition.Add(new KeyValuePair<IData, IData>(keyData, valueData));
            }

            PutAllInternal(partitions);
        }

        protected virtual void PutAllInternal(List<KeyValuePair<IData, IData>>[] partitions)
        {
            var futures = new ConcurrentQueue<IFuture<ClientMessage>>();
            Parallel.For(0, partitions.Length, i =>
            {
                var entries = partitions[i];
                if (entries.Count > 0)
                {
                    var request = MapPutAllCodec.EncodeRequest(Name, entries);
                    var future = Client.InvocationService.InvokeOnPartitionOwner(request, i);
                    futures.Enqueue(future);
                }
            });
            GetResult(futures);
        }

        public void Clear()
        {
            var request = MapClearCodec.EncodeRequest(Name);
            Invoke(request);
        }

        public ISet<TKey> KeySet(IPredicate predicate)
        {
            if (predicate is PagingPredicate)
            {
                return KeySetWithPagingPredicate(predicate);
            }
            var request = MapKeySetWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
            var keys = Invoke(request, predicate, m => MapKeySetWithPredicateCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazySet<TKey>(keys, Client.SerializationService);
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet(IPredicate predicate)
        {
            if (ContainsPagingPredicate(predicate))
            {
                return EntrySetWithPagingPredicate(predicate);
            }
            var request = MapEntriesWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
            var entries = Invoke(request, predicate, (response) =>
            {
                var resultQueue = new List<KeyValuePair<IData, object>>();
                foreach (var kvp in MapEntriesWithPredicateCodec.DecodeResponse(response).Response)
                {
                    resultQueue.Add(new KeyValuePair<IData, object>(kvp.Key, kvp.Value));
                }
                return resultQueue;
            });
            return new ReadOnlyLazyEntrySet<TKey, TValue, object>(entries, Client.SerializationService);
        }

        public ICollection<TValue> Values(IPredicate predicate)
        {
            if (predicate is PagingPredicate)
            {
                return ValuesForPagingPredicate((PagingPredicate) predicate);
            }

            var request = MapValuesWithPredicateCodec.EncodeRequest(Name, ToData(predicate));
            var result = Invoke(request, predicate, m => MapValuesWithPredicateCodec.DecodeResponse(m).Response);
            return new ReadOnlyLazyList<TValue, IData>(result, Client.SerializationService);
        }

        private T Invoke<T>(ClientMessage request, IPredicate predicate, Func<ClientMessage, T> decodeResponse)
        {
            var partitionPredicate = predicate as PartitionPredicate;
            if (partitionPredicate != null)
            {
                return Invoke(request, partitionPredicate.GetPartitionKey(), decodeResponse);
            }
            return Invoke(request, decodeResponse);
        }

        private void OnEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValue, int eventTypeInt,
            Guid uuid, int numberOfAffectedEntries, EntryListenerAdapter<TKey, TValue> listenerAdapter)
        {
            var member = Client.ClusterService.GetMember(uuid);
            listenerAdapter.OnEntryEvent(Name, keyData, valueData, oldValueData, mergingValue, (EntryEventType) eventTypeInt,
                member, numberOfAffectedEntries);
        }

        protected internal override void OnInitialize()
        {
            _lockReferenceIdGenerator = Client.LockReferenceIdGenerator;
        }

        private ISet<KeyValuePair<TKey, TValue>> EntrySetWithPagingPredicate(IPredicate predicate)
        {
            var pagingPredicate = UnwrapPagingPredicate(predicate);
            pagingPredicate.IterationType = IterationType.Entry;
            var pagingPredicateHolder = PagingPredicateHolder.Of(predicate, Client.SerializationService);
            var request = MapEntriesWithPagingPredicateCodec.EncodeRequest(Name, pagingPredicateHolder);
            var response = InvokeWithPredicate(request, predicate);
            var resultParameters = MapEntriesWithPagingPredicateCodec.DecodeResponse(response);
            pagingPredicate.AnchorList = resultParameters.AnchorDataList.AsAnchorIterator(Client.SerializationService).ToList();
            return new ReadOnlyLazyEntrySet<TKey, TValue, IData>(resultParameters.Response, Client.SerializationService);
        }

        private ISet<TKey> KeySetWithPagingPredicate(IPredicate predicate)
        {
            var pagingPredicate = UnwrapPagingPredicate(predicate);
            pagingPredicate.IterationType = IterationType.Key;

            var pagingPredicateHolder = PagingPredicateHolder.Of(predicate, Client.SerializationService);
            var request = MapKeySetWithPagingPredicateCodec.EncodeRequest(Name, pagingPredicateHolder);
            var response = InvokeWithPredicate(request, predicate);
            var resultParameters = MapKeySetWithPagingPredicateCodec.DecodeResponse(response);
            pagingPredicate.AnchorList = resultParameters.AnchorDataList.AsAnchorIterator(Client.SerializationService).ToList();
            return new ReadOnlyLazySet<TKey>(resultParameters.Response, Client.SerializationService);
        }

        private ICollection<TValue> ValuesForPagingPredicate(IPredicate predicate)
        {
            var pagingPredicate = UnwrapPagingPredicate(predicate);
            pagingPredicate.IterationType = IterationType.Value;

            var pagingPredicateHolder = PagingPredicateHolder.Of(predicate, Client.SerializationService);
            var request = MapValuesWithPagingPredicateCodec.EncodeRequest(Name, pagingPredicateHolder);
            var response = InvokeWithPredicate(request, predicate);
            var resultParameters = MapValuesWithPagingPredicateCodec.DecodeResponse(response);
            pagingPredicate.AnchorList = resultParameters.AnchorDataList.AsAnchorIterator(Client.SerializationService).ToList();
            return new ReadOnlyLazySet<TValue>(resultParameters.Response, Client.SerializationService);
        }

        private ClientMessage InvokeWithPredicate(ClientMessage request, IPredicate predicate)
        {
            ClientMessage response;
            if (predicate is PartitionPredicate partitionPredicate)
            {
                response = Invoke(request, partitionPredicate.GetPartitionKey());
            }
            else
            {
                response = Invoke(request);
            }
            return response;
        }

        private static bool ContainsPagingPredicate(IPredicate predicate)
        {
            if (predicate is PagingPredicate)
            {
                return true;
            }
            var partitionPredicate = predicate as PartitionPredicate;
            return partitionPredicate?.GetTarget() is PagingPredicate;
        }

        private static PagingPredicate UnwrapPagingPredicate(IPredicate predicate)
        {
            if (predicate is PagingPredicate pagingPredicate)
            {
                return pagingPredicate;
            }
            var unwrappedPredicate = ((PartitionPredicate) predicate).GetTarget();
            return (PagingPredicate) unwrappedPredicate;
        }
    }
}