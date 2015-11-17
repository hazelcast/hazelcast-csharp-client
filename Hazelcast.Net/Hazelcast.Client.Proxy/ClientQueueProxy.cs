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
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal sealed class ClientQueueProxy<T> : ClientProxy, IQueue<T>
    {
        public ClientQueueProxy(string serviceName, string name)
            : base(serviceName, name)
        {
        }

        public string AddItemListener(IItemListener<T> listener, bool includeValue)
        {
            var request = QueueAddListenerCodec.EncodeRequest(GetName(), includeValue, false);

            DistributedEventHandler handler = m =>
                QueueAddListenerCodec.AbstractEventHandler.Handle(m,
                    (item, uuid, type) =>
                    {
                        HandleItemListener(item, uuid, (ItemEventType) type, listener, includeValue);
                    });

            return Listen(request, m => QueueAddListenerCodec.DecodeResponse(m).response, GetPartitionKey(), handler);
        }

        public bool RemoveItemListener(string registrationId)
        {
            return StopListening(s => QueueRemoveListenerCodec.EncodeRequest(GetName(), s),
                m => QueueRemoveListenerCodec.DecodeResponse(m).response, registrationId);
        }

        public bool Add(T e)
        {
            if (Offer(e))
            {
                return true;
            }
            throw new InvalidOperationException("Queue is full!");
        }

        public bool Offer(T e)
        {
            try
            {
                return Offer(e, 0, TimeUnit.Seconds);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Offer(T e, long timeout, TimeUnit unit)
        {
            var data = ToData(e);
            var request = QueueOfferCodec.EncodeRequest(GetName(), data, unit.ToMillis(timeout));
            return Invoke(request, m => QueueOfferCodec.DecodeResponse(m).response);
        }

        public void Put(T e)
        {
            var data = ToData(e);
            var request = QueuePutCodec.EncodeRequest(GetName(), data);
            Invoke(request);
        }

        public T Take()
        {
            var request = QueueTakeCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => QueueTakeCodec.DecodeResponse(m).response);
            return ToObject<T>(result);
        }

        public T Poll(long timeout, TimeUnit unit)
        {
            var request = QueuePollCodec.EncodeRequest(GetName(), unit.ToMillis(timeout));
            var result = Invoke(request, m => QueuePollCodec.DecodeResponse(m).response);
            return ToObject<T>(result);
        }

        public int RemainingCapacity()
        {
            var request = QueueRemainingCapacityCodec.EncodeRequest(GetName());
            return Invoke(request, m => QueueRemainingCapacityCodec.DecodeResponse(m).response);
        }

        public bool Remove(T item)
        {
            return Remove((object) item);
        }

        public bool Remove(object o)
        {
            var data = ToData(o);
            var request = QueueRemoveCodec.EncodeRequest(GetName(), data);
            return Invoke(request, m => QueueRemoveCodec.DecodeResponse(m).response);
        }

        public bool Contains(object o)
        {
            var value = ToData(o);
            var request = QueueContainsCodec.EncodeRequest(GetName(), value);
            return Invoke(request, m => QueueContainsCodec.DecodeResponse(m).response);
        }

        public bool Contains(T item)
        {
            return Contains((object) item);
        }

        public int DrainTo<TE>(ICollection<TE> c) where TE : T
        {
            var request = QueueDrainToCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => QueueDrainToCodec.DecodeResponse(m).list);
            foreach (var data in result)
            {
                var e = ToObject<T>(data);
                c.Add((TE) e);
            }
            return result.Count;
        }

        public int DrainTo<TE>(ICollection<TE> c, int maxElements) where TE : T
        {
            var request = QueueDrainToMaxSizeCodec.EncodeRequest(GetName(), maxElements);
            var result = Invoke(request, m => QueueDrainToMaxSizeCodec.DecodeResponse(m).list);
            foreach (var data in result)
            {
                var e = ToObject<T>(data);
                c.Add((TE) e);
            }
            return result.Count;
        }

        public T Remove()
        {
            var res = Poll();
            if (res == null)
            {
                throw new InvalidOperationException("Queue is empty!");
            }
            return res;
        }

        public T Poll()
        {
            try
            {
                return Poll(0, TimeUnit.Seconds);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public T Element()
        {
            var res = Peek();
            if (res == null)
            {
                throw new InvalidOperationException("Queue is empty!");
            }
            return res;
        }

        public T Peek()
        {
            var request = QueuePeekCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => QueuePeekCodec.DecodeResponse(m).response);
            return ToObject<T>(result);
        }

        public int Count
        {
            get { return Size(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int Size()
        {
            var request = QueueSizeCodec.EncodeRequest(GetName());
            return Invoke(request, m => QueueSizeCodec.DecodeResponse(m).response);
        }

        public bool IsEmpty()
        {
            var request = QueueIsEmptyCodec.EncodeRequest(GetName());
            return Invoke(request, m => QueueIsEmptyCodec.DecodeResponse(m).response);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var coll = GetAll();
            return new QueueIterator<T>(coll.GetEnumerator(), GetContext().GetSerializationService());
        }

        public T[] ToArray()
        {
            var coll = GetAll();
            var i = 0;
            var array = new T[coll.Count];
            foreach (var data in coll)
            {
                array[i++] = GetContext().GetSerializationService().ToObject<T>(data);
            }
            return array;
        }

        public TE[] ToArray<TE>(TE[] a)
        {
            var array = ToArray();
            if (a.Length < array.Length)
            {
                // Make a new array of a's runtime type, but my contents:
                var tmp = new TE[array.Length];
                Array.Copy(array, 0, tmp, 0, array.Length);
                return tmp;
            }
            Array.Copy(array, 0, a, 0, array.Length);
            if (a.Length > array.Length)
                a[array.Length] = default(TE);
            return a;
        }

        public bool ContainsAll<TE>(ICollection<TE> c)
        {
            var dataSet = ToDataList(c);
            var request = QueueContainsAllCodec.EncodeRequest(GetName(), dataSet);
            return Invoke(request, m => QueueContainsAllCodec.DecodeResponse(m).response);
        }

        public bool AddAll<TE>(ICollection<TE> c)
        {
            var request = QueueAddAllCodec.EncodeRequest(GetName(), ToDataList(c));
            return Invoke(request, m => QueueAddAllCodec.DecodeResponse(m).response);
        }

        public bool RemoveAll<TE>(ICollection<TE> c)
        {
            var request = QueueCompareAndRemoveAllCodec.EncodeRequest(GetName(), ToDataList(c));
            return Invoke(request, m => QueueCompareAndRemoveAllCodec.DecodeResponse(m).response);
        }

        public bool RetainAll<TE>(ICollection<TE> c)
        {
            var request = QueueCompareAndRetainAllCodec.EncodeRequest(GetName(), ToDataList(c));
            return Invoke(request, m => QueueCompareAndRetainAllCodec.DecodeResponse(m).response);
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            var request = QueueClearCodec.EncodeRequest(GetName());
            Invoke(request, QueueClearCodec.DecodeResponse);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var a = ToArray();
            if (a != null) a.CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetPartitionKey());
        }

        private ICollection<IData> GetAll()
        {
            var request = QueueIteratorCodec.EncodeRequest(GetName());
            return Invoke(request, m => QueueIteratorCodec.DecodeResponse(m).list);
        }

        private void HandleItemListener(IData itemData, string uuid, ItemEventType eventType, IItemListener<T> listener,
            bool includeValue)
        {
            var item = includeValue
                ? ToObject<T>(itemData)
                : default(T);
            var member = GetContext().GetClusterService().GetMember(uuid);
            var itemEvent = new ItemEvent<T>(GetName(), eventType, item, member);
            if (eventType == ItemEventType.Added)
            {
                listener.ItemAdded(itemEvent);
            }
            else
            {
                listener.ItemRemoved(itemEvent);
            }
        }
    }
}