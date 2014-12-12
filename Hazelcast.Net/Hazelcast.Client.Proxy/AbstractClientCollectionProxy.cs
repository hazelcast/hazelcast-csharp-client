using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Collection;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    //.Net reviewed
    internal class AbstractClientCollectionProxy<E> : ClientProxy, IHCollection<E>
    {
        protected internal readonly string partitionKey;

        public AbstractClientCollectionProxy(string serviceName, string objectName) : base(serviceName, objectName)
        {
            partitionKey = GetPartitionKey();
        }

        public virtual IEnumerator<E> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual bool Add(E item)
        {
            ThrowExceptionIfNull(item);
            var request = new CollectionAddRequest(GetName(), ToData(item));
            return Invoke<bool>(request);
        }

        void ICollection<E>.Add(E item)
        {
            Add(item);
        }

        public void Clear()
        {
            var request = new CollectionClearRequest(GetName());
            Invoke<object>(request);
        }

        public bool Contains(E item)
        {
            ThrowExceptionIfNull(item);
            var request = new CollectionContainsRequest(GetName(), ToData(item));
            var result = Invoke<bool>(request);
            return result;
        }

        public void CopyTo(E[] array, int arrayIndex)
        {
            GetAll().ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(E item)
        {
            ThrowExceptionIfNull(item);
            var request = new CollectionRemoveRequest(GetName(), ToData(item));
            var result = Invoke<bool>(request);
            return result;
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
            var request = new CollectionSizeRequest(GetName());
            var result = Invoke<int>(request);
            return result;
        }

        public bool IsEmpty()
        {
            return Size() == 0;
        }

        public E[] ToArray()
        {
            E[] array = GetAll().ToArray();
            var tmp = new E[array.Length];
            Array.Copy(array, 0, tmp, 0, array.Length);
            return tmp;
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

        public bool ContainsAll<T>(ICollection<T> c)
        {
            ThrowExceptionIfNull(c);
            ICollection<IData> valueSet = new HashSet<IData>();
            foreach (object o in c)
            {
                ThrowExceptionIfNull(o);
                valueSet.Add(ToData(o));
            }
            var request = new CollectionContainsRequest(GetName(), valueSet);
            var result = Invoke<bool>(request);
            return result;
        }

        public bool RemoveAll<T>(ICollection<T> c)
        {
            return CompareAndRemove(false, c);
        }

        public bool RetainAll<T>(ICollection<T> c)
        {
            return CompareAndRemove(true, c);
        }

        public bool AddAll<T>(ICollection<T> c)
        {
            ThrowExceptionIfNull(c);
            IList<IData> valueList = new List<IData>();
            foreach (T e in c)
            {
                ThrowExceptionIfNull(e);
                valueList.Add(ToData(e));
            }
            var request = new CollectionAddAllRequest(GetName(), valueList);
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual string AddItemListener(IItemListener<E> listener, bool includeValue)
        {
            var request = new CollectionAddListenerRequest(GetName(), includeValue);
            request.SetServiceName(GetServiceName());
            return Listen(request, GetPartitionKey(), args => HandleItemListener(args, listener, includeValue));
        }

        public bool RemoveItemListener(string registrationId)
        {
            var request = new CollectionRemoveListenerRequest(GetName(), registrationId, GetServiceName());
            return StopListening(request,registrationId);
        }

        protected override void OnDestroy()
        {
        }

        private void HandleItemListener(IData eventData, IItemListener<E> listener, bool includeValue)
        {
            var portableItemEvent = ToObject<PortableItemEvent>(eventData);
            E item = includeValue ? GetContext().GetSerializationService().ToObject<E>(portableItemEvent.GetItem()) : default(E);
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

        private bool CompareAndRemove<T>(bool retain, ICollection<T> c)
        {
            ThrowExceptionIfNull(c);
            ICollection<IData> valueSet = new HashSet<IData>();
            foreach (object o in c)
            {
                ThrowExceptionIfNull(o);
                valueSet.Add(ToData(o));
            }
            var request = new CollectionCompareAndRemoveRequest(GetName(), valueSet, retain);
            var result = Invoke<bool>(request);
            return result;
        }

        protected override T Invoke<T>(ClientRequest req)
        {
            var collectionRequest = req as CollectionRequest;
            if (collectionRequest != null)
            {
                CollectionRequest request = collectionRequest;
                request.SetServiceName(GetServiceName());
            }
            return base.Invoke<T>(req,GetPartitionKey());
        }

        protected IEnumerable<E> GetAll()
        {
            var request = new CollectionGetAllRequest(GetName());
            var result = Invoke<SerializableCollection>(request);
            ICollection<IData> collection = result.GetCollection();
            var list = new List<E>(collection.Count);
            foreach (IData value in collection)
            {
                list.Add(ToObject<E>(value));
            }
            return list;
        }
    }
}