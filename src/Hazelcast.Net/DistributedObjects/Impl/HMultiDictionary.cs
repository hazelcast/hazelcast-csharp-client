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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal class HMultiDictionary<TKey, TValue> : DistributedObjectBase, IHMultiDictionary<TKey, TValue>
    {
        private readonly ISequence<long> _lockReferenceIdSequence;

        public HMultiDictionary(string name, Cluster cluster, ISerializationService serializationService, ISequence<long> lockReferenceIdSequence, ILoggerFactory loggerFactory)
            : base(HMultiDictionary.ServiceName, name, cluster, serializationService, loggerFactory)
        {
            _lockReferenceIdSequence = lockReferenceIdSequence;
        }

        public Task<Guid> SubscribeAsync(bool includeValues, Action<MultiDictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(includeValues, default, false, handle);

        public Task<Guid> SubscribeAsync(bool includeValues, TKey key, Action<MultiDictionaryEventHandlers<TKey, TValue>> handle)
            => SubscribeAsync(includeValues, key, true, handle);

        private async Task<Guid> SubscribeAsync(bool includeValues, TKey key, bool hasKey, Action<MultiDictionaryEventHandlers<TKey, TValue>> handle)
        {
            if (hasKey && key == null) throw new ArgumentNullException(nameof(key));
            if (handle == null) throw new ArgumentNullException(nameof(handle));

            var handlers = new MultiDictionaryEventHandlers<TKey, TValue>();
            handle(handlers);

            // 0: no entryKey
            // 1: entryKey
            var mode = hasKey ? 1 : 0;

            var subscribeRequest = mode switch
            {
                0 => MultiMapAddEntryListenerCodec.EncodeRequest(Name, includeValues, Cluster.IsSmartRouting),
                1 => MultiMapAddEntryListenerToKeyCodec.EncodeRequest(Name, ToData(key), includeValues, Cluster.IsSmartRouting),
                _ => throw new NotSupportedException()
            };

            var subscription = new ClusterSubscription(
                subscribeRequest,
                ReadSubscribeResponse,
                CreateUnsubscribeRequest,
                ReadUnsubscribeResponse,
                HandleEventAsync,
                new MapSubscriptionState(mode, Name, handlers));

            await Cluster.InstallSubscriptionAsync(subscription).CAF();

            return subscription.Id;
        }

        private class MapSubscriptionState : SubscriptionState<MultiDictionaryEventHandlers<TKey, TValue>>
        {
            public MapSubscriptionState(int mode, string name, MultiDictionaryEventHandlers<TKey, TValue> handlers)
                : base(name, handlers)
            {
                Mode = mode;
            }

            public int Mode { get; }
        }

        private ValueTask HandleEventAsync(ClientMessage eventMessage, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);

            async ValueTask HandleEntryEventAsync(IData keyData, IData valueData, IData oldValueData, IData mergingValueData, int eventTypeData, Guid memberId, int numberOfAffectedEntries)
            {
                var eventType = (HDictionaryEventTypes)eventTypeData;
                if (eventType == HDictionaryEventTypes.Nothing) return;

                var member = Cluster.GetMember(memberId);
                var key = LazyArg<TKey>(keyData);
                var value = LazyArg<TValue>(valueData);
                var oldValue = LazyArg<TValue>(oldValueData);
                var mergingValue = LazyArg<TValue>(mergingValueData);

                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var handler in sstate.Handlers)
                {
                    if (handler.EventType.HasAll(eventType))
                    {
                        var task = handler switch
                        {
                            IDictionaryEntryEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>> entryHandler => entryHandler.HandleAsync(this, member, key, value, oldValue, mergingValue, eventType, numberOfAffectedEntries),
                            IDictionaryEventHandler<TKey, TValue, IHMultiDictionary<TKey, TValue>> mapHandler => mapHandler.HandleAsync(this, member, numberOfAffectedEntries),
                            _ => throw new NotSupportedException()
                        };
                        await task.CAF();
                    }
                }
            }

            return sstate.Mode switch
            {
                0 => MultiMapAddEntryListenerCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, LoggerFactory),
                1 => MultiMapAddEntryListenerToKeyCodec.HandleEventAsync(eventMessage, HandleEntryEventAsync, LoggerFactory),
                _ => throw new NotSupportedException()
            };
        }

        private static ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);
            return MultiMapRemoveEntryListenerCodec.EncodeRequest(sstate.Name, subscriptionId);
        }

        private static Guid ReadSubscribeResponse(ClientMessage responseMessage, object state)
        {
            var sstate = ToSafeState<MapSubscriptionState>(state);

            return sstate.Mode switch
            {
                0 => MultiMapAddEntryListenerCodec.DecodeResponse(responseMessage).Response,
                1 => MultiMapAddEntryListenerToKeyCodec.DecodeResponse(responseMessage).Response,
                _ => throw new NotSupportedException()
            };
        }

        private static bool ReadUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, object state)
        {
            return MultiMapRemoveEntryListenerCodec.DecodeResponse(unsubscribeResponseMessage).Response;
        }

        public async Task<bool> TryAddAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = MultiMapPutCodec.EncodeRequest(Name, keyData, valueData, ContextId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            return MultiMapPutCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<IReadOnlyList<TValue>> GetAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = MultiMapGetCodec.EncodeRequest(Name, keyData, ContextId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            var response = MultiMapGetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public async Task<IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>> GetAllAsync()
        {
            var requestMessage = MultiMapEntrySetCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage).CAF();
            var response = MultiMapEntrySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyDictionaryOfList<TKey, TValue>(SerializationService) { response };
        }

        public async Task<IReadOnlyList<TKey>> GetKeysAsync()
        {
            var requestMessage = MultiMapKeySetCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage).CAF();
            var response = MultiMapKeySetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TKey>(response, SerializationService);
        }

        public async Task<IReadOnlyList<TValue>> GetValuesAsync()
        {
            var requestMessage = MultiMapValuesCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage).CAF();
            var response  = MultiMapValuesCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public async Task<bool> ContainsEntryAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = MultiMapContainsEntryCodec.EncodeRequest(Name, keyData, valueData, ContextId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            return MultiMapContainsEntryCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<bool> ContainsKeyAsync(TKey key)
        {
            var keyData = ToData(key);
            var requestMessage = MultiMapContainsKeyCodec.EncodeRequest(Name, keyData, ContextId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            return MultiMapContainsKeyCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<bool> ContainsValueAsync(TValue value)
        {
            var valueData = ToData(value);
            var requestMessage = MultiMapContainsValueCodec.EncodeRequest(Name, valueData);
            var responseMessage = await Cluster.SendAsync(requestMessage).CAF();
            return MultiMapContainsValueCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<int> CountAsync()
        {
            var requestMessage = MultiMapSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage).CAF();
            return MultiMapSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<int> CountValuesAsync(TKey key)
        {
            var keyData = ToData(key);
            var requestMessage = MultiMapValueCountCodec.EncodeRequest(Name, keyData, ContextId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            return MultiMapValueCountCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<bool> RemoveAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);

            var requestMessage = MultiMapRemoveEntryCodec.EncodeRequest(Name, keyData, valueData, ContextId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            return MultiMapRemoveEntryCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<IReadOnlyList<TValue>> RemoveAsync(TKey key)
        {
            var keyData = ToData(key);

            var requestMessage = MultiMapRemoveCodec.EncodeRequest(Name, keyData, ContextId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            var response = MultiMapRemoveCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public async Task ClearAsync()
        {
            var requestMessage = MultiMapClearCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage).CAF();
            _ = MultiMapClearCodec.DecodeResponse(responseMessage);
        }

        public Task LockAsync(TKey key)
            => LockForAsync(key, LeaseTime.InfiniteTimeSpan);

        public Task<bool> TryLockAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryWaitLockAsync(TKey key, TimeSpan timeToWait)
            => TryWaitLockForAsync(key, timeToWait, LeaseTime.InfiniteTimeSpan);

        public async Task<bool> TryWaitLockForAsync(TKey key, TimeSpan timeToWait, TimeSpan leaseTime)
        {
            var keyData = ToSafeData(key);
            var leaseTimeMs = leaseTime.CodecMilliseconds(long.MaxValue);
            var timeToWaitMs = timeToWait.CodecMilliseconds(0);
            var requestMessage = MultiMapTryLockCodec.EncodeRequest(Name, keyData, ContextId, leaseTimeMs, timeToWaitMs, _lockReferenceIdSequence.GetNext());
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            return MultiMapTryLockCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task LockForAsync(TKey key, TimeSpan leaseTime)
        {
            var keyData = ToSafeData(key);
            var leaseTimeMs = leaseTime.CodecMilliseconds(long.MaxValue);
            var requestMessage = MultiMapLockCodec.EncodeRequest(Name, keyData, ContextId, leaseTimeMs, _lockReferenceIdSequence.GetNext());
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            _ = MultiMapLockCodec.DecodeResponse(responseMessage);
        }

        public async Task<bool> IsLockedAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = MultiMapIsLockedCodec.EncodeRequest(Name, keyData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            return MultiMapIsLockedCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task UnlockAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = MultiMapUnlockCodec.EncodeRequest(Name, keyData, ContextId, _lockReferenceIdSequence.GetNext());
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            _ = MultiMapUnlockCodec.DecodeResponse(responseMessage);
        }

        public async Task ForceUnlockAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = MultiMapForceUnlockCodec.EncodeRequest(Name, keyData, _lockReferenceIdSequence.GetNext());
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CAF();
            _ = MultiMapForceUnlockCodec.DecodeResponse(responseMessage);
        }
    }
}
