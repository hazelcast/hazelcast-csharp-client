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
    internal sealed class ClientQueueProxy<E> : ClientProxy, IQueue<E>
    {
        public ClientQueueProxy(string serviceName, string name)
            : base(serviceName, name)
        {
        }

        public string AddItemListener(IItemListener<E> listener, bool includeValue)
        {
            var request = QueueAddListenerCodec.EncodeRequest(GetName(), includeValue);

            DistributedEventHandler handler = m =>
                QueueAddListenerCodec.AbstractEventHandler.Handle(m, (item, uuid, type) =>
                {
                    HandleItemListener(item, uuid, (ItemEventType)type, listener, includeValue);
                });

            return Listen(request, m => QueueAddListenerCodec.DecodeResponse(m).response, GetPartitionKey(), handler);
        }

        public bool RemoveItemListener(string registrationId)
        {
            var request = QueueRemoveListenerCodec.EncodeRequest(GetName(), registrationId);
            return StopListening(request, m => QueueRemoveListenerCodec.DecodeResponse(m).response, registrationId);
        }

        public bool Add(E e)
        {
            if (Offer(e))
            {
                return true;
            }
            throw new InvalidOperationException("Queue is full!");
        }

        public bool Offer(E e)
        {
            try
            {
                return Offer(e, 0, TimeUnit.SECONDS);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Offer(E e, long timeout, TimeUnit unit)
        {
            var data = ToData(e);
            var request = QueueOfferCodec.EncodeRequest(GetName(), data, unit.ToMillis(timeout));
            return Invoke(request, m => QueueOfferCodec.DecodeResponse(m).response);
        }

        public void Put(E e)
        {
            Offer(e, -1, TimeUnit.MILLISECONDS);
        }

        public E Take()
        {
            var request = QueueTakeCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => QueueTakeCodec.DecodeResponse(m).response);
            return ToObject<E>(result);
        }

        public E Poll(long timeout, TimeUnit unit)
        {
            var request = QueuePollCodec.EncodeRequest(GetName(), unit.ToMillis(timeout));
            var result = Invoke(request, m => QueuePollCodec.DecodeResponse(m).response);
            return ToObject<E>(result);
        }

        public int RemainingCapacity()
        {
            var request = QueueRemainingCapacityCodec.EncodeRequest(GetName());
            return Invoke(request, m => QueueRemainingCapacityCodec.DecodeResponse(m).response);
        }

        public bool Remove(E item)
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

        public bool Contains(E item)
        {
            return Contains((object) item);
        }

        public int DrainTo<T>(ICollection<T> c) where T : E
        {
            var request = QueueDrainToCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => QueueDrainToCodec.DecodeResponse(m).list);
            foreach (var data in result)
            {
                var e = ToObject<E>(data);
                c.Add((T)e);
            }
            return result.Count; ;
        }

        public int DrainTo<T>(ICollection<T> c, int maxElements) where T : E
        {
            var request = QueueDrainToMaxSizeCodec.EncodeRequest(GetName(), maxElements);
            var result = Invoke(request, m => QueueDrainToMaxSizeCodec.DecodeResponse(m).list);
            foreach (var data in result)
            {
                var e = ToObject<E>(data);
                c.Add((T) e);
            }
            return result.Count;
        }

        public E Remove()
        {
            var res = Poll();
            if (res == null)
            {
                throw new InvalidOperationException("Queue is empty!");
            }
            return res;
        }

        public E Poll()
        {
            try
            {
                return Poll(0, TimeUnit.SECONDS);
            }
            catch (Exception)
            {
                return default(E);
            }
        }

        public E Element()
        {
            var res = Peek();
            if (res == null)
            {
                throw new InvalidOperationException("Queue is empty!");
            }
            return res;
        }

        public E Peek()
        {
            var request = QueuePeekCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => QueuePeekCodec.DecodeResponse(m).response);
            return ToObject<E>(result);
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
            return Size() == 0;
        }

        public IEnumerator<E> GetEnumerator()
        {
            var coll = GetAll();
            return new QueueIterator<E>(coll.GetEnumerator(), GetContext().GetSerializationService());
        }

        public E[] ToArray()
        {
            var coll = GetAll();
            var i = 0;
            var array = new E[coll.Count];
            foreach (var data in coll)
            {
                array[i++] = GetContext().GetSerializationService().ToObject<E>(data);
            }
            return array;
        }

        public T[] ToArray<T>(T[] a)
        {
            var array = ToArray();
            if (a.Length < array.Length)
            {
                // Make a new array of a's runtime type, but my contents:
                var tmp = new T[array.Length];
                Array.Copy(array, 0, tmp, 0, array.Length);
                return tmp;
            }
            Array.Copy(array, 0, a, 0, array.Length);
            if (a.Length > array.Length)
                a[array.Length] = default(T);
            return a;
        }

        public bool ContainsAll<_T0>(ICollection<_T0> c)
        {
            var dataSet = GetDataSet(c);
            var request = QueueContainsAllCodec.EncodeRequest(GetName(), dataSet);
            return Invoke(request, m => QueueContainsAllCodec.DecodeResponse(m).response);
        }

        public bool AddAll<T>(ICollection<T> c)
        {
            var request = QueueAddAllCodec.EncodeRequest(GetName(), GetDataSet(c));
            return Invoke(request, m => QueueAddAllCodec.DecodeResponse(m).response);
        }

        public bool RemoveAll<T>(ICollection<T> c)
        {
            var request = QueueCompareAndRemoveAllCodec.EncodeRequest(GetName(), GetDataSet(c));
            return Invoke(request, m => QueueCompareAndRemoveAllCodec.DecodeResponse(m).response);
        }

        public bool RetainAll<T>(ICollection<T> c)
        {
            var request = QueueCompareAndRetainAllCodec.EncodeRequest(GetName(), GetDataSet(c));
            return Invoke(request, m => QueueCompareAndRetainAllCodec.DecodeResponse(m).response);
        }

        void ICollection<E>.Add(E item)
        {
            Add(item);
        }

        public void Clear()
        {
            var request = QueueClearCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public void CopyTo(E[] array, int arrayIndex)
        {
            var a = ToArray();
            if (a != null) a.CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void HandleItemListener(IData itemData, String uuid, ItemEventType eventType, IItemListener<E> listener, bool includeValue)
        {
            var item = includeValue
                ? ToObject<E>(itemData)
                : default(E);
            var member = GetContext().GetClusterService().GetMember(uuid);
            var itemEvent = new ItemEvent<E>(GetName(), eventType, item, member);
            if (eventType == ItemEventType.Added)
            {
                listener.ItemAdded(itemEvent);
            }
            else
            {
                listener.ItemRemoved(itemEvent);
            }
        }

        private ICollection<IData> GetAll()
        {
            var request = QueueIteratorCodec.EncodeRequest(GetName());
            return Invoke(request, m => QueueIteratorCodec.DecodeResponse(m).list);
        }

        private ISet<IData> GetDataSet<T>(ICollection<T> objects)
        {
            ISet<IData> dataList = new HashSet<IData>();
            foreach (object o in objects)
            {
                dataList.Add(GetContext().GetSerializationService().ToData(o));
            }
            return dataList;
        }

        protected override T Invoke<T>(IClientMessage request, Func<IClientMessage, T> decodeResponse)
        {
            return base.Invoke(request, GetPartitionKey(), decodeResponse);
        }
    }
}