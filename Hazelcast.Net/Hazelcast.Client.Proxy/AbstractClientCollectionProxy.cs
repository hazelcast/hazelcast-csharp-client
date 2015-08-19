using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    internal abstract class AbstractClientCollectionProxy<E> : ClientProxy, IHCollection<E>
    {
        protected AbstractClientCollectionProxy(string serviceName, string objectName) : base(serviceName, objectName)
        {
        }

        public abstract string AddItemListener(IItemListener<E> listener, bool includeValue);
        public abstract bool RemoveItemListener(string registrationId);
        public abstract int Size();
        public abstract bool IsEmpty();
        public abstract bool ContainsAll<T>(ICollection<T> c);
        public abstract bool RemoveAll<T>(ICollection<T> c);
        public abstract bool RetainAll<T>(ICollection<T> c);
        public abstract bool AddAll<T>(ICollection<T> c);
        public abstract bool Add(E item);
        public abstract void Clear();
        public abstract bool Contains(E item);
        public abstract bool Remove(E item);

        public virtual IEnumerator<E> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual E[] ToArray()
        {
            return GetAll().ToArray();
        }

        public virtual T[] ToArray<T>(T[] a)
        {
            var array = GetAll().ToArray();
            if (a.Length < array.Length)
            {
                a = new T[array.Length];
                Array.Copy(array, 0, a, 0, array.Length);
                return a;
            }

            Array.Copy(array, 0, a, 0, array.Length);
            if (a.Length > array.Length)
            {
                a[array.Length] = default(T);
            }
            return a;
        }

        public virtual void CopyTo(E[] array, int index)
        {
            ThrowExceptionIfNull(array);
            if (index < 0) throw new IndexOutOfRangeException("Index cannot be negative.");

            var all = GetAll();
            if ((array.Length - index) < all.Count)
            {
                throw new ArgumentException("The number of source elements is greater" +
                                            " than the available space from index to the end of the destination array.");
            }

            var i = index;
            foreach (var item in all)
            {
                array[i++] = item;
            }
        }

        void ICollection<E>.Add(E item)
        {
            Add(item);
        }

        public virtual int Count
        {
            get { return Size(); }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        protected abstract ICollection<E> GetAll();

        protected override T Invoke<T>(IClientMessage request, Func<IClientMessage, T> decodeResponse)
        {
            return base.Invoke(request, GetPartitionKey(), decodeResponse);
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetPartitionKey());
        }

        protected void HandleItemListener(IData itemData, String uuid, ItemEventType eventType,
            IItemListener<E> listener, bool includeValue)
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
    }
}