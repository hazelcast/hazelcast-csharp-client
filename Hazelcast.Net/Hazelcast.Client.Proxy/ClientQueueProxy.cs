using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Queue;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    internal sealed class ClientQueueProxy<E> : ClientProxy, IQueue<E>
    {
        private readonly string name;

        public ClientQueueProxy(string serviceName, string name)
            : base(serviceName, name)
        {
            this.name = name;
        }

        public string AddItemListener(IItemListener<E> listener, bool includeValue)
        {
            var request = new AddListenerRequest(GetName(), includeValue);
            return Listen(request, GetPartitionKey(), args => HandleItemListener(args, listener, includeValue));
        }

        public bool RemoveItemListener(string registrationId)
        {
            var request = new RemoveListenerRequest(name, registrationId);
            return StopListening(request, registrationId);
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
            IData data = GetContext().GetSerializationService().ToData(e);
            var request = new OfferRequest(name, unit.ToMillis(timeout), data);
            var result = Invoke<bool>(request);
            return result;
        }

        public void Put(E e)
        {
            Offer(e, -1, TimeUnit.MILLISECONDS);
        }

        public E Take()
        {
            return Poll(-1, TimeUnit.MILLISECONDS);
        }

        public E Poll(long timeout, TimeUnit unit)
        {
            var request = new PollRequest(name, unit.ToMillis(timeout));
            return Invoke<E>(request);
        }

        public int RemainingCapacity()
        {
            var request = new RemainingCapacityRequest(name);
            var result = Invoke<int>(request);
            return result;
        }

        public bool Remove(E item)
        {
            return Remove((object) item);
        }

        public bool Remove(object o)
        {
            IData data = GetContext().GetSerializationService().ToData(o);
            var request = new RemoveRequest(name, data);
            var result = Invoke<bool>(request);
            return result;
        }

        public bool Contains(object o)
        {
            ICollection<IData> list = new List<IData>(1);
            list.Add(GetContext().GetSerializationService().ToData(o));
            var request = new ContainsRequest(name, list);
            var result = Invoke<bool>(request);
            return result;
        }

        public bool Contains(E item)
        {
            return Contains((object) item);
        }

        public int DrainTo<T>(ICollection<T> c) where T : E
        {
            return DrainTo(c, -1);
        }

        public int DrainTo<T>(ICollection<T> c, int maxElements) where T : E
        {
            var request = new DrainRequest(name, maxElements);
            var result = Invoke<PortableCollection>(request);
            ICollection<IData> coll = result.GetCollection();
            foreach (IData data in coll)
            {
                var e = GetContext().GetSerializationService().ToObject<E>(data);
                c.Add((T) e);
            }
            return coll.Count;
        }

        public E Remove()
        {
            E res = Poll();
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
            E res = Peek();
            if (res == null)
            {
                throw new InvalidOperationException("Queue is empty!");
            }
            return res;
        }

        public E Peek()
        {
            var request = new PeekRequest(name);
            return Invoke<E>(request);
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
            var request = new SizeRequest(GetName());
            var result = Invoke<int>(request);
            return result;
        }

        public bool IsEmpty()
        {
            return Size() == 0;
        }

        public IEnumerator<E> GetEnumerator()
        {
            ICollection<IData> coll = GetAll();
            return new QueueIterator<E>(coll.GetEnumerator(), GetContext().GetSerializationService());
        }


        public E[] ToArray()
        {
            ICollection<IData> coll = GetAll();
            int i = 0;
            var array = new E[coll.Count];
            foreach (IData data in coll)
            {
                array[i++] = GetContext().GetSerializationService().ToObject<E>(data);
            }
            return array;
        }

        public T[] ToArray<T>(T[] a)
        {
            E[] array = ToArray();
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
            IList<IData> list = GetDataList(c);
            var request = new ContainsRequest(name, list);
            var result = Invoke<bool>(request);
            return result;
        }

        public bool AddAll<T>(ICollection<T> c)
        {
            var request = new AddAllRequest(name, GetDataList(c));
            var result = Invoke<bool>(request);
            return result;
        }

        public bool RemoveAll<T>(ICollection<T> c)
        {
            var request = new CompareAndRemoveRequest(name, GetDataList(c), false);
            var result = Invoke<bool>(request);
            return result;
        }

        public bool RetainAll<T>(ICollection<T> c)
        {
            var request = new CompareAndRemoveRequest(name, GetDataList(c), true);
            var result = Invoke<bool>(request);
            return result;
        }

        void ICollection<E>.Add(E item)
        {
            Add(item);
        }

        public void Clear()
        {
            var request = new ClearRequest(name);
            Invoke<object>(request);
        }


        public void CopyTo(E[] array, int arrayIndex)
        {
            E[] a = ToArray();
            if (a != null) a.CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void HandleItemListener(IData eventData, IItemListener<E> listener, bool includeValue)
        {
            var portableItemEvent = ToObject<PortableItemEvent>(eventData);
            E item = includeValue
                ? GetContext().GetSerializationService().ToObject<E>(portableItemEvent.GetItem())
                : default(E);
            IMember member = GetContext().GetClusterService().GetMember(portableItemEvent.GetUuid());
            var itemEvent = new ItemEvent<E>(GetName(), portableItemEvent.GetEventType(), item, member);
            if (portableItemEvent.GetEventType() == ItemEventType.Added)
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
            var request = new IteratorRequest(name);
            var result = Invoke<PortableCollection>(request);
            ICollection<IData> coll = result.GetCollection();
            return coll;
        }

        protected override void OnDestroy()
        {
        }

        private IList<IData> GetDataList<T>(ICollection<T> objects)
        {
            IList<IData> dataList = new List<IData>(objects.Count);
            foreach (object o in objects)
            {
                dataList.Add(GetContext().GetSerializationService().ToData(o));
            }
            return dataList;
        }


        protected override T Invoke<T>(ClientRequest request)
        {
            return base.Invoke<T>(request, GetPartitionKey());
        }
    }
}