using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Client.Request.Queue;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

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
            return Listen(request, GetPartitionKey(),(args) => HandleItemListener((PortableItemEvent) args, listener, includeValue));
        }

        public bool RemoveItemListener(string registrationId)
        {
            var request = new RemoveListenerRequest(name, registrationId);
            return StopListening(request,registrationId);
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
            Data data = GetContext().GetSerializationService().ToData(e);
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


        public bool Remove(object o)
        {
            Data data = GetContext().GetSerializationService().ToData(o);
            var request = new RemoveRequest(name, data);
            var result = Invoke<bool>(request);
            return result;
        }

        public bool Contains(object o)
        {
            ICollection<Data> list = new List<Data>(1);
            list.Add(GetContext().GetSerializationService().ToData(o));
            var request = new ContainsRequest(name, list);
            var result = Invoke<bool>(request);
            return result;
        }

        public int DrainTo<T>(ICollection<T> c) where T : E
        {
            return DrainTo(c, -1);
        }

        public int DrainTo<T>(ICollection<T> c, int maxElements) where T : E
        {
            var request = new DrainRequest(name, maxElements);
            var result = Invoke<PortableCollection>(request);
            ICollection<Data> coll = result.GetCollection();
            foreach (Data data in coll)
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

        public bool Remove(E item)
        {
            return Remove((object)item);
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
            ICollection<Data> coll = GetAll();
            return new QueueIterator<E>(coll.GetEnumerator(), GetContext().GetSerializationService());
        }


        public E[] ToArray()
        {
            ICollection<Data> coll = GetAll();
            int i = 0;
            var array = new E[coll.Count];
            foreach (Data data in coll)
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
            IList<Data> list = GetDataList(c);
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

        public bool Contains(E item)
        {
            ICollection<Data> list = new List<Data>(1);
            list.Add(GetContext().GetSerializationService().ToData(item));
            var request = new ContainsRequest(name, list);
            var result = Invoke<bool>(request);
            return result;
        }

        public void CopyTo(E[] array, int arrayIndex)
        {
            GetAll().ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void HandleItemListener(PortableItemEvent portableItemEvent, IItemListener<E> listener,
            bool includeValue)
        {
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

        private ICollection<Data> GetAll()
        {
            var request = new IteratorRequest(name);
            var result = Invoke<PortableCollection>(request);
            ICollection<Data> coll = result.GetCollection();
            return coll;
        }

        protected override void OnDestroy()
        {
        }

        private IList<Data> GetDataList<T>(ICollection<T> objects)
        {
            IList<Data> dataList = new List<Data>(objects.Count);
            foreach (object o in objects)
            {
                dataList.Add(GetContext().GetSerializationService().ToData(o));
            }
            return dataList;
        }
    }

  
}