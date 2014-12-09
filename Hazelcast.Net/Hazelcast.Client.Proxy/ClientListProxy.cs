using System.Collections.Generic;
using Hazelcast.Client.Request.Collection;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Proxy
{
    //.NET reviewed
    internal class ClientListProxy<E> : AbstractClientCollectionProxy<E>, IHList<E>
    {
        public ClientListProxy(string serviceName, string objectName) : base(serviceName, objectName)
        {
        }

        public int IndexOf(E item)
        {
            return IndexOfInternal(item, false);
        }

        public void Insert(int index, E item)
        {
            Add(index, item);
        }

        public void RemoveAt(int index)
        {
            Remove(index);
        }

        public E this[int index]
        {
            get { return Get(index); }
            set { Set(index, value); }
        }

        public E Get(int index)
        {
            var request = new ListGetRequest(GetName(), index);
            return Invoke<E>(request);
        }

        public E Set(int index, E element)
        {
            ThrowExceptionIfNull(element);
            IData value = ToData(element);
            var request = new ListSetRequest(GetName(), index, value);
            return Invoke<E>(request);
        }

        public void Add(int index, E element)
        {
            ThrowExceptionIfNull(element);
            IData value = ToData(element);
            var request = new ListAddRequest(GetName(), value, index);
            Invoke<object>(request);
        }

        public E Remove(int index)
        {
            var request = new ListRemoveRequest(GetName(), index);
            return Invoke<E>(request);
        }

        public int LastIndexOf(E o)
        {
            return IndexOfInternal(o, true);
        }

        public bool AddAll<_T0>(int index, ICollection<_T0> c) where _T0 : E
        {
            ThrowExceptionIfNull(c);
            IList<IData> valueList = new List<IData>(c.Count);
            foreach (E e in c)
            {
                ThrowExceptionIfNull(e);
                valueList.Add(ToData(e));
            }
            var request = new ListAddAllRequest(GetName(), valueList, index);
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual IList<E> SubList(int fromIndex, int toIndex)
        {
            var request = new ListSubRequest(GetName(), fromIndex, toIndex);
            var result = Invoke<SerializableCollection>(request);
            ICollection<IData> collection = result.GetCollection();
            IList<E> list = new List<E>(collection.Count);
            foreach (IData value in collection)
            {
                list.Add(ToObject<E>(value));
            }
            return list;
        }

        private int IndexOfInternal(object o, bool last)
        {
            ThrowExceptionIfNull(o);
            IData value = ToData(o);
            var request = new ListIndexOfRequest(GetName(), value, last);
            var result = Invoke<int>(request);
            return result;
        }
    }
}