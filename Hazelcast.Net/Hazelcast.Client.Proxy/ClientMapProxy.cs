// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientMapProxy<TKey, TValue> : ClientProxy, IMap<TKey, TValue>
    {
        private ClientLockReferenceIdGenerator _lockReferenceIdGenerator;

        public ClientMapProxy(string serviceName, string name) : base(serviceName, name)
        {
        }

        public TResult Aggregate<TResult>(IAggregator<TResult> aggregator)
        {
            ValidationUtil.IsNotNull(aggregator, "aggregator");
            var request = MapAggregateCodec.EncodeRequest(GetName(), ToData(aggregator));
            var response = Invoke(request);
            var resultParameters = MapAggregateCodec.DecodeResponse(response);
            return ToObject<TResult>(resultParameters.response);
        }

        public TResult Aggregate<TResult>(IAggregator<TResult> aggregator, IPredicate predicate)
        {
            ValidationUtil.IsNotNull(aggregator, "aggregator");
            ValidationUtil.IsNotNull(predicate, "predicate");
            var request = MapAggregateWithPredicateCodec.EncodeRequest(GetName(), ToData(aggregator), ToData(predicate));
            var response = Invoke(request);
            var resultParameters = MapAggregateWithPredicateCodec.DecodeResponse(response);
            return ToObject<TResult>(resultParameters.response);
        }

        public ICollection<TResult> Project<TResult>(IProjection projection)
        {
            ValidationUtil.IsNotNull(projection, "projection");
            var request = MapProjectCodec.EncodeRequest(GetName(), ToData(projection));
            var response = Invoke(request);
            var resultParameters = MapProjectCodec.DecodeResponse(response);
            return new ReadOnlyLazyList<TResult>(resultParameters.response, GetContext().GetSerializationService());
        }

        public ICollection<TResult> Project<TResult>(IProjection projection, IPredicate predicate)
        {
            ValidationUtil.IsNotNull(projection, "projection");
            ValidationUtil.IsNotNull(predicate, "predicate");
            var request = MapProjectWithPredicateCodec.EncodeRequest(GetName(), ToData(projection), ToData(predicate));
            var response = Invoke(request);
            var resultParameters = MapProjectWithPredicateCodec.DecodeResponse(response);
            return new ReadOnlyLazyList<TResult>(resultParameters.response, GetContext().GetSerializationService());
        }

        public bool ContainsKey(object key)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return containsKeyInternal(keyData);
        }

        protected virtual bool containsKeyInternal(IData keyData)
        {
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
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return ToObject<TValue>(GetInternal(keyData));
        }

        protected virtual object GetInternal(IData keyData)
        {
            var request = MapGetCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var result = Invoke(request, keyData);
            return MapGetCodec.DecodeResponse(result).response;
        }

        public TValue Put(TKey key, TValue value)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);

            return Put(key, value, -1, TimeUnit.Seconds);
        }

        public TValue Remove(object key)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            var keyData = ToData(key);            
            return RemoveInternal(keyData);
        }

        protected virtual TValue RemoveInternal(IData keyData)
        {
            var request = MapRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var clientMessage = Invoke(request, keyData);
            return ToObject<TValue>(MapRemoveCodec.DecodeResponse(clientMessage).response);
        }

        public bool Remove(object key, object value)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return RemoveInternal(keyData, value);
        }

        protected virtual bool RemoveInternal(IData keyData, object value)
        {
            var valueData = ToData(value);
            var request = MapRemoveIfSameCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            var clientMessage = Invoke(request, keyData);
            return MapRemoveIfSameCodec.DecodeResponse(clientMessage).response;
        }

        public void Delete(object key)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            DeleteInternal(keyData);
        }

        protected virtual void DeleteInternal(IData keyData)
        {
            var request = MapDeleteCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            Invoke(request, keyData);
        }

        public void Flush()
        {
            var request = MapFlushCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public virtual Task<TValue> GetAsync(TKey key)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            var request = MapGetCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            return InvokeAsync(request, keyData, m =>
            {
                var resp = MapGetCodec.DecodeResponse(m).response;
                return ToObject<TValue>(resp);
            });
        }

        public Task<TValue> PutAsync(TKey key, TValue value)
        {
            return PutAsync(key, value, -1, TimeUnit.Seconds);
        }

        public Task<TValue> PutAsync(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return PutAsyncInternal(keyData, value, ttl, timeunit);
        }

        protected virtual Task<TValue> PutAsyncInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request =
                MapPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(), timeunit.ToMillis(ttl));
            return InvokeAsync(request, keyData, m =>
            {
                var response = MapPutCodec.DecodeResponse(m).response;
                return ToObject<TValue>(response);
            });
        }

        public Task<TValue> RemoveAsync(TKey key)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return RemoveAsyncInternal(keyData);
        }

        protected virtual Task<TValue> RemoveAsyncInternal(IData keyData)
        {
            var request = MapRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            return InvokeAsync(request, keyData, m =>
            {
                var response = MapRemoveCodec.DecodeResponse(m).response;
                return ToObject<TValue>(response);
            });
        }

        public bool TryRemove(TKey key, long timeout, TimeUnit timeunit)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return TryRemoveInternal(keyData, timeout, timeunit);
        }

        protected virtual bool TryRemoveInternal(IData keyData, long timeout, TimeUnit timeunit)
        {
            var request =
                MapTryRemoveCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(), timeunit.ToMillis(timeout));
            var result = Invoke(request, keyData);
            var response = MapTryRemoveCodec.DecodeResponse(result).response;
            return response;
        }

        public bool TryPut(TKey key, TValue value, long timeout, TimeUnit timeunit)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return TryPutInternal(keyData, value, timeout, timeunit);
        }

        protected virtual bool TryPutInternal(IData keyData, TValue value, long timeout, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request = MapTryPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(timeout));
            var result = Invoke(request, keyData);
            var response = MapTryPutCodec.DecodeResponse(result).response;
            return response;
        }

        public TValue Put(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            var keyData = ToData(key);

            return PutInternal(keyData, value, ttl, timeunit);
        }

        protected virtual TValue PutInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request =
                MapPutCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(), timeunit.ToMillis(ttl));
            var clientMessage = Invoke(request, keyData);
            var response = MapPutCodec.DecodeResponse(clientMessage).response;
            return ToObject<TValue>(response);
        }

        public void PutTransient(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            PutTransientInternal(keyData, value, ttl, timeunit);
        }

        protected virtual void PutTransientInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request = MapPutTransientCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
            Invoke(request, keyData);
        }

        public TValue PutIfAbsent(TKey key, TValue value)
        {
            return PutIfAbsent(key, value, -1, TimeUnit.Seconds);
        }

        public TValue PutIfAbsent(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return PutIfAbsentInternal(keyData, value, ttl, timeunit);
        }

        protected virtual TValue PutIfAbsentInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request = MapPutIfAbsentCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(),
                timeunit.ToMillis(ttl));
            var clientMessage = Invoke(request, keyData);
            var response = MapPutIfAbsentCodec.DecodeResponse(clientMessage).response;
            return ToObject<TValue>(response);
        }

        public bool Replace(TKey key, TValue oldValue, TValue newValue)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(oldValue, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(newValue, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return ReplaceIfSameInternal(keyData, oldValue, newValue);
        }

        protected virtual bool ReplaceIfSameInternal(IData keyData, TValue oldValue, TValue newValue)
        {
            var oldValueData = ToData(oldValue);
            var newValueData = ToData(newValue);
            var request =
                MapReplaceIfSameCodec.EncodeRequest(GetName(), keyData, oldValueData, newValueData, ThreadUtil.GetThreadId());
            var clientMessage = Invoke(request, keyData);
            return MapReplaceIfSameCodec.DecodeResponse(clientMessage).response;
        }

        public void RemoveAll(IPredicate predicate)
        {
            ValidationUtil.CheckNotNull(predicate, "predicate cannot be null");
            RemoveAllInternal(predicate);
        }

        protected virtual void RemoveAllInternal(IPredicate predicate)
        {
            var request = MapRemoveAllCodec.EncodeRequest(GetName(), ToData(predicate));
            Invoke(request);
        }

        public TValue Replace(TKey key, TValue value)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            return ReplaceInternal(keyData, value);
        }

        public TValue ReplaceInternal(IData keyData, TValue value)
        {
            var valueData = ToData(value);
            var request = MapReplaceCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId());
            var clientMessage = Invoke(request, keyData);
            var response = MapReplaceCodec.DecodeResponse(clientMessage).response;
            return ToObject<TValue>(response);
        }

        public void Set(TKey key, TValue value, long ttl, TimeUnit timeunit)
        {
            ValidationUtil.CheckNotNull(key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
            ValidationUtil.CheckNotNull(value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            var keyData = ToData(key);
            SetInternal(keyData, value, ttl, timeunit);
        }

        protected virtual void SetInternal(IData keyData, TValue value, long ttl, TimeUnit timeunit)
        {
            var valueData = ToData(value);
            var request =
                MapSetCodec.EncodeRequest(GetName(), keyData, valueData, ThreadUtil.GetThreadId(), timeunit.ToMillis(ttl));
            Invoke(request, keyData);
        }

        public void Lock(TKey key)
        {
            Lock(key, -1, TimeUnit.Milliseconds);
        }

        public void Lock(TKey key, long leaseTime, TimeUnit timeUnit)
        {
            var keyData = ToData(key);
            var request = MapLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(), timeUnit.ToMillis(leaseTime),
                _lockReferenceIdGenerator.GetNextReferenceId());
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
            ValidationUtil.ThrowExceptionIfNull(key);
            var keyData = ToData(key);
            return SubmitToKeyInternal(keyData, entryProcessor);
        }

        protected virtual Task<object> SubmitToKeyInternal(IData keyData, IEntryProcessor entryProcessor)
        {
            var request = MapSubmitToKeyCodec.EncodeRequest(GetName(), ToData(entryProcessor), keyData, ThreadUtil.GetThreadId());
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

        public bool TryLock(TKey key, long time, TimeUnit timeunit)
        {
            return TryLock(key, time, timeunit, long.MaxValue, TimeUnit.Milliseconds);
        }

        public bool TryLock(TKey key, long time, TimeUnit timeunit, long leaseTime, TimeUnit leaseUnit)
        {
            var keyData = ToData(key);
            var request = MapTryLockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                leaseUnit.ToMillis(leaseTime), timeunit.ToMillis(time), _lockReferenceIdGenerator.GetNextReferenceId());
            return Invoke(request, keyData, m => MapTryLockCodec.DecodeResponse(m).response);
        }

        public void Unlock(TKey key)
        {
            var keyData = ToData(key);
            var request = MapUnlockCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId(),
                _lockReferenceIdGenerator.GetNextReferenceId());
            Invoke(request, keyData);
        }

        public void ForceUnlock(TKey key)
        {
            var keyData = ToData(key);
            var request = MapForceUnlockCodec.EncodeRequest(GetName(), keyData, _lockReferenceIdGenerator.GetNextReferenceId());
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

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, TKey key, bool includeValue)
        {
            return AddEntryListener((MapListener) listener, key, includeValue);
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate, TKey key, bool includeValue)
        {
            return AddEntryListener((MapListener) listener, predicate, key, includeValue);
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate, bool includeValue)
        {
            return AddEntryListener((MapListener) listener, predicate, includeValue);
        }

        public string AddEntryListener(IEntryListener<TKey, TValue> listener, bool includeValue)
        {
            return AddEntryListener((MapListener) listener, includeValue);
        }

        public string AddEntryListener(MapListener listener, bool includeValue)
        {
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var listenerFlags = (int) listenerAdapter.ListenerFlags;
            var request = MapAddEntryListenerCodec.EncodeRequest(GetName(), includeValue, listenerFlags, IsSmart());
            DistributedEventHandler handler = eventData => MapAddEntryListenerCodec.EventHandler.HandleEvent(eventData,
                (key, value, oldValue, mergingValue, type, uuid, entries) =>
                {
                    OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                });

            return RegisterListener(request, message => MapAddEntryListenerCodec.DecodeResponse(message).response,
                id => MapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public string AddEntryListener(MapListener listener, TKey key, bool includeValue)
        {
            var keyData = ToData(key);
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var listenerFlags = (int) listenerAdapter.ListenerFlags;
            var request = MapAddEntryListenerToKeyCodec.EncodeRequest(GetName(), keyData, includeValue, listenerFlags, IsSmart());
            DistributedEventHandler handler = eventData => MapAddEntryListenerToKeyCodec.EventHandler.HandleEvent(eventData,
                (key_, value, oldValue, mergingValue, type, uuid, entries) =>
                {
                    OnEntryEvent(key_, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                });

            return RegisterListener(request, message => MapAddEntryListenerToKeyCodec.DecodeResponse(message).response,
                id => MapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public string AddEntryListener(MapListener listener, IPredicate predicate, TKey key, bool includeValue)
        {
            var keyData = ToData(key);
            var predicateData = ToData(predicate);
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var listenerFlags = (int) listenerAdapter.ListenerFlags;
            var request = MapAddEntryListenerToKeyWithPredicateCodec.EncodeRequest(GetName(), keyData, predicateData,
                includeValue, listenerFlags, IsSmart());
            DistributedEventHandler handler = eventData =>
                MapAddEntryListenerToKeyWithPredicateCodec.EventHandler.HandleEvent(eventData,
                    (k, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(k, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });
            return RegisterListener(request, message => MapAddEntryListenerCodec.DecodeResponse(message).response,
                id => MapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public string AddEntryListener(MapListener listener, IPredicate predicate, bool includeValue)
        {
            var predicateData = ToData(predicate);
            var listenerAdapter =
                EntryListenerAdapter<TKey, TValue>.CreateAdapter(listener, GetContext().GetSerializationService());
            var listenerFlags = (int) listenerAdapter.ListenerFlags;
            var request =
                MapAddEntryListenerWithPredicateCodec.EncodeRequest(GetName(), predicateData, includeValue, listenerFlags,
                    IsSmart());
            DistributedEventHandler handler = eventData =>
                MapAddEntryListenerWithPredicateCodec.EventHandler.HandleEvent(eventData,
                    (key, value, oldValue, mergingValue, type, uuid, entries) =>
                    {
                        OnEntryEvent(key, value, oldValue, mergingValue, type, uuid, entries, listenerAdapter);
                    });

            return RegisterListener(request, message => MapAddEntryListenerWithPredicateCodec.DecodeResponse(message).response,
                id => MapRemoveEntryListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public bool RemoveEntryListener(string registrationId)
        {
            return DeregisterListener(registrationId);
        }

        public IEntryView<TKey, TValue> GetEntryView(TKey key)
        {
            var keyData = ToData(key);
            var request = MapGetEntryViewCodec.EncodeRequest(GetName(), keyData, ThreadUtil.GetThreadId());
            var response = Invoke(request, keyData);
            var parameters = MapGetEntryViewCodec.DecodeResponse(response);
            var entryView = new SimpleEntryView<TKey, TValue>();
            var dataEntryView = parameters.response;
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
            ValidationUtil.ThrowExceptionIfNull(key);
            var keyData = ToData(key);
            return ExecuteOnKeyInternal(keyData, entryProcessor);
        }

        protected virtual object ExecuteOnKeyInternal(IData keyData, IEntryProcessor entryProcessor)
        {
            var request =
                MapExecuteOnKeyCodec.EncodeRequest(GetName(), ToData(entryProcessor), keyData, ThreadUtil.GetThreadId());
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
            var result = Invoke(request, m => MapKeySetCodec.DecodeResponse(m).response);
            return new ReadOnlyLazySet<TKey>(result, GetContext().GetSerializationService());
        }

        public IDictionary<TKey, TValue> GetAll(ICollection<TKey> keys)
        {
            if (keys == null || keys.Count == 0) return new Dictionary<TKey, TValue>();

            var keysSize = keys.Count;
            var resultingKeyValuePairs = new List<object>(keysSize * 2);
            
            var keyDatas = new List<IData>();
            foreach (var key in keys)
            {
                var keyData = ToData(key);
                keyDatas.Add(keyData);
            }
            GetAllInternal(keyDatas, resultingKeyValuePairs);

            var result = new Dictionary<TKey, TValue>(keysSize);
            for (var i = 0; i < resultingKeyValuePairs.Count;)
            {
                var key = ToObject<TKey>((IData) resultingKeyValuePairs[i++]);
                var value = ToObject<TValue>((IData) resultingKeyValuePairs[i++]);
                result.Add(key, value);
            }
            return result;
        }

        protected virtual void GetAllInternal(ICollection<IData> keyDatas, List<object> resultingKeyValuePairs)
        {
            var partitionToKeyData = GetPartitionKeyData(keyDatas);
            var invocationService = GetContext().GetInvocationService();
            var futures = new List<IFuture<IClientMessage>>(partitionToKeyData.Count);
            foreach (var kvp in partitionToKeyData)
            {
                var partitionId = kvp.Key;
                var keyList = kvp.Value;
                if (keyList.Count > 0)
                {
                    var request = MapGetAllCodec.EncodeRequest(GetName(), keyList);
                    futures.Add(invocationService.InvokeOnPartition(request, partitionId));
                }
            }
            var messages = ThreadUtil.GetResult(futures);
            foreach (var clientMessage in messages)
            {
                var items = MapGetAllCodec.DecodeResponse(clientMessage).response;
                foreach (var entry in items)
                {
                    resultingKeyValuePairs.Add(entry.Key);
                    resultingKeyValuePairs.Add(entry.Value);
                }
            }
        }

        private Dictionary<int, IList<IData>> GetPartitionKeyData(ICollection<IData> keyDatas)
        {
            var partitionService = GetContext().GetPartitionService();
            // split the keys based on which partition they belong
            var partitionToKeyData = new Dictionary<int, IList<IData>>();
            foreach (var keyData in keyDatas)
            {
                var partitionId = partitionService.GetPartitionId(keyData);
                IList<IData> keyList;
                if (!partitionToKeyData.TryGetValue(partitionId, out keyList))
                {
                    partitionToKeyData[partitionId] = keyList = new List<IData>();
                }
                keyList.Add(keyData);
            }
            return partitionToKeyData;
        }

        public ICollection<TValue> Values()
        {
            var request = MapValuesCodec.EncodeRequest(GetName());
            var list = Invoke(request, m => MapValuesCodec.DecodeResponse(m).response);
            return new ReadOnlyLazyList<TValue>(list, GetContext().GetSerializationService());
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet()
        {
            var request = MapEntrySetCodec.EncodeRequest(GetName());
            var entries = Invoke(request, m => MapEntrySetCodec.DecodeResponse(m).response);
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
                ValidationUtil.CheckNotNull(kvp.Key, ValidationUtil.NULL_KEY_IS_NOT_ALLOWED);
                ValidationUtil.CheckNotNull(kvp.Value, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);

                var keyData = ToData(kvp.Key);
                var partitionId = partitionService.GetPartitionId(keyData);
                IDictionary<IData, IData> partition;
                if (!partitions.TryGetValue(partitionId, out partition))
                {
                    partition = new Dictionary<IData, IData>();
                    partitions[partitionId] = partition;
                }

                partition[keyData] = ToData(kvp.Value);
            }
            PutAllInternal(m, partitions);
        }

        protected virtual void PutAllInternal(IDictionary<TKey, TValue> map,
            Dictionary<int, IDictionary<IData, IData>> partitions)
        {
            var futures = new List<IFuture<IClientMessage>>(partitions.Count);
            foreach (var kvp in partitions)
            {
                var request = MapPutAllCodec.EncodeRequest(GetName(), kvp.Value.ToList());
                var future = GetContext().GetInvocationService().InvokeOnPartition(request, kvp.Key);
                futures.Add(future);
            }
            ThreadUtil.GetResult(futures);
        }

        public void Clear()
        {
            var request = MapClearCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public ISet<TKey> KeySet(IPredicate predicate)
        {
            if (predicate is PagingPredicate)
            {
                return KeySetWithPagingPredicate((PagingPredicate) predicate);
            }
            var request = MapKeySetWithPredicateCodec.EncodeRequest(GetName(), ToData(predicate));
            var keys = Invoke(request, predicate, m => MapKeySetWithPredicateCodec.DecodeResponse(m).response);
            return new ReadOnlyLazySet<TKey>(keys, GetContext().GetSerializationService());
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet(IPredicate predicate)
        {
            if (predicate is PagingPredicate)
            {
                return EntrySetWithPagingPredicate((PagingPredicate) predicate);
            }

            var request = MapEntriesWithPredicateCodec.EncodeRequest(GetName(), ToData(predicate));
            var entries = Invoke(request, predicate, m => MapEntriesWithPredicateCodec.DecodeResponse(m).response);
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
            if (predicate is PagingPredicate)
            {
                return ValuesForPagingPredicate((PagingPredicate) predicate);
            }

            var request = MapValuesWithPredicateCodec.EncodeRequest(GetName(), ToData(predicate));
            var result = Invoke(request, predicate, m => MapValuesWithPredicateCodec.DecodeResponse(m).response);
            return new ReadOnlyLazyList<TValue>(result, GetContext().GetSerializationService());
        }

        private T Invoke<T>(IClientMessage request, IPredicate predicate, Func<IClientMessage, T> decodeResponse)
        {
            var partitionPredicate = predicate as PartitionPredicate;
            if (partitionPredicate != null)
            {
                return Invoke(request, partitionPredicate.GetPartitionKey(), decodeResponse);
            }
            return Invoke(request, decodeResponse);
        }

        private void OnEntryEvent(IData keyData, IData valueData, IData oldValueData, IData mergingValue, int eventTypeInt,
            string uuid, int numberOfAffectedEntries, EntryListenerAdapter<TKey, TValue> listenerAdapter)
        {
            var member = GetContext().GetClusterService().GetMember(uuid);
            listenerAdapter.OnEntryEvent(GetName(), keyData, valueData, oldValueData, mergingValue, (EntryEventType) eventTypeInt,
                member, numberOfAffectedEntries);
        }

        protected override void OnInitialize()
        {
            _lockReferenceIdGenerator = GetContext().GetClient().GetLockReferenceIdGenerator();
        }

        private ISet<KeyValuePair<TKey, TValue>> EntrySetWithPagingPredicate(PagingPredicate pagingPredicate)
        {
            pagingPredicate.IterationType = IterationType.Entry;

            var request = MapEntriesWithPagingPredicateCodec.EncodeRequest(GetName(), ToData(pagingPredicate));
            var response = Invoke(request);
            var resultParameters = MapEntriesWithPagingPredicateCodec.DecodeResponse(response);

            var entryList = new List<KeyValuePair<object, object>>();
            foreach (var dataEntry in resultParameters.response)
            {
                var key = ToObject<TKey>(dataEntry.Key);
                var value = ToObject<TValue>(dataEntry.Value);
                entryList.Add(new KeyValuePair<object, object>(key, value));
            }
            var resultEnumerator =
                SortingUtil.GetSortedQueryResultSet<TKey, TValue>(entryList, pagingPredicate, IterationType.Entry);
            return new HashSet<KeyValuePair<TKey, TValue>>(resultEnumerator.Cast<KeyValuePair<TKey, TValue>>());
        }

        private ISet<TKey> KeySetWithPagingPredicate(PagingPredicate pagingPredicate)
        {
            pagingPredicate.IterationType = IterationType.Key;
            var request = MapKeySetWithPagingPredicateCodec.EncodeRequest(GetName(), ToData(pagingPredicate));
            var response = Invoke(request);
            var resultParameters = MapKeySetWithPagingPredicateCodec.DecodeResponse(response);

            var resultList = new List<KeyValuePair<object, object>>();
            foreach (var keyData in resultParameters.response)
            {
                var key = ToObject<TKey>(keyData);
                resultList.Add(new KeyValuePair<object, object>(key, default(TValue)));
            }
            var resultEnumerator =
                SortingUtil.GetSortedQueryResultSet<TKey, TValue>(resultList, pagingPredicate, IterationType.Key);
            return new HashSet<TKey>(resultEnumerator.Cast<TKey>());
        }

        private ICollection<TValue> ValuesForPagingPredicate(PagingPredicate pagingPredicate)
        {
            pagingPredicate.IterationType = IterationType.Value;

            var request = MapValuesWithPagingPredicateCodec.EncodeRequest(GetName(), ToData(pagingPredicate));
            var response = Invoke(request);
            var resultParameters = MapValuesWithPagingPredicateCodec.DecodeResponse(response);

            var resultList = new List<KeyValuePair<object, object>>();
            foreach (var dataEntry in resultParameters.response)
            {
                var key = ToObject<TKey>(dataEntry.Key);
                var value = ToObject<TValue>(dataEntry.Value);
                resultList.Add(new KeyValuePair<object, object>(key, value));
            }
            var resultEnumerator =
                SortingUtil.GetSortedQueryResultSet<TKey, TValue>(resultList, pagingPredicate, IterationType.Value);
            return resultEnumerator.Cast<TValue>().ToList();
        }
    }
}