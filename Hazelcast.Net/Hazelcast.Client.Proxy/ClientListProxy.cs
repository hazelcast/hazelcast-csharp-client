using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    //.NET reviewed
    internal class ClientListProxy<E> : ClientProxy, IHList<E>
    {
        public ClientListProxy(string serviceName, string objectName) : base(serviceName, objectName)
        {
        }

        public int IndexOf(E item)
        {
            ThrowExceptionIfNull(item);
            var value = ToData(item);
            var request = ListIndexOfCodec.EncodeRequest(GetName(), value);
            var response = Invoke(request);
            return ListIndexOfCodec.DecodeResponse(response).response;
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
            var request = ListGetCodec.EncodeRequest(GetName(), index);
            var response = Invoke(request);
            return ToObject<E>(ListGetCodec.DecodeResponse(response).response);
        }

        public E Set(int index, E element)
        {
            ThrowExceptionIfNull(element);
            var value = ToData(element);
            var request = ListSetCodec.EncodeRequest(GetName(), index, value);
            var response = Invoke(request);
            return ToObject<E>(ListSetCodec.DecodeResponse(response).response);
        }

        public void Add(int index, E element)
        {
            ThrowExceptionIfNull(element);
            var value = ToData(element);
            var request = ListAddWithIndexCodec.EncodeRequest(GetName(), index, value);
            Invoke(request);
        }

        public E Remove(int index)
        {
            var request = ListRemoveWithIndexCodec.EncodeRequest(GetName(), index);
            var response = Invoke(request);
            return ToObject<E>(ListRemoveWithIndexCodec.DecodeResponse(response).response);
        }

        public int LastIndexOf(E item)
        {
            ThrowExceptionIfNull(item);
            var value = ToData(item);
            var request = ListLastIndexOfCodec.EncodeRequest(GetName(), value);
            var response = Invoke(request);
            return ListLastIndexOfCodec.DecodeResponse(response).response;
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
            var request = ListAddAllWithIndexCodec.EncodeRequest(GetName(), index, valueList);
            var response = Invoke(request);
            return ListAddAllWithIndexCodec.DecodeResponse(response).response;
        }

        public virtual IList<E> SubList(int fromIndex, int toIndex)
        {
            var request = ListSubCodec.EncodeRequest(GetName(), fromIndex, toIndex);
            var response = Invoke(request);
            ICollection<IData> collection = ListSubCodec.DecodeResponse(response).list;
            IList<E> list = new List<E>(collection.Count);
            foreach (var value in collection)
            {
                list.Add(ToObject<E>(value));
            }
            return list;
        }

        public IEnumerator<E> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string AddItemListener(IItemListener<E> listener, bool includeValue)
        {
            var request = ListAddListenerCodec.EncodeRequest(GetName(), includeValue);

            DistributedEventHandler handler = message => ListAddListenerCodec.AbstractEventHandler.Handle(message,
                ((item, uuid, type) => { HandleItemListener(item, listener, includeValue); }));

            return Listen(request,
                m => ListAddListenerCodec.DecodeResponse(m).response, GetPartitionKey(), handler);
        }

        public bool RemoveItemListener(string registrationId)
        {
            var request = ListRemoveListenerCodec.EncodeRequest(GetName(), registrationId);
            return StopListening(request, message => ListRemoveListenerCodec.DecodeResponse(message).response,
                registrationId);
        }

        public bool Add(E item)
        {
            var request = ListAddCodec.EncodeRequest(GetName(), ToData(item));
            var response = Invoke(request);
            return ListAddCodec.DecodeResponse(response).response;
        }

        public int Size()
        {
            var request = ListSizeCodec.EncodeRequest(GetName());
            var response = Invoke(request);
            return ListSizeCodec.DecodeResponse(response).response;
        }

        public bool IsEmpty()
        {
            var request = ListIsEmptyCodec.EncodeRequest(GetName());
            var response = Invoke(request);
            return ListIsEmptyCodec.DecodeResponse(response).response;
        }

        public E[] ToArray()
        {
            return GetAll().ToArray();
        }

        public T[] ToArray<T>(T[] a)
        {
            E[] array = GetAll().ToArray();
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

        public bool ContainsAll<T>(ICollection<T> c)
        {
            HashSet<IData> valueSet = new HashSet<IData>();
            foreach (var o in c)
            {
                ThrowExceptionIfNull(o);
                valueSet.Add(ToData(o));
            }
            var request = ListContainsAllCodec.EncodeRequest(GetName(), valueSet);
            var response = Invoke(request);
            return ListContainsAllCodec.DecodeResponse(response).response;
        }

        public bool RemoveAll<T>(ICollection<T> c)
        {
            HashSet<IData> valueSet = new HashSet<IData>();
            foreach (var o in c)
            {
                ThrowExceptionIfNull(o);
                valueSet.Add(ToData(o));
            }
            var request = ListCompareAndRemoveAllCodec.EncodeRequest(GetName(), valueSet);
            var response = Invoke(request);
            return ListCompareAndRemoveAllCodec.DecodeResponse(response).response;
        }

        public bool RetainAll<T>(ICollection<T> c)
        {
            throw new NotImplementedException();
        }

        public bool AddAll<T>(ICollection<T> c)
        {
            ThrowExceptionIfNull(c);
            List<IData> values = new List<IData>();
            foreach (var o in c)
            {
                ThrowExceptionIfNull(o);
                values.Add(ToData(o));
            }

            var request = ListAddAllCodec.EncodeRequest(GetName(), values);
            var response = Invoke(request);
            return ListAddAllCodec.DecodeResponse(response).response;
        }

        void ICollection<E>.Add(E item)
        {
            Add(item);
        }

        public void Clear()
        {
            var request = ListClearCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public bool Contains(E item)
        {
            var request = ListContainsCodec.EncodeRequest(GetName(), ToData(item));
            var response = Invoke(request);
            return ListContainsCodec.DecodeResponse(response).response;
        }

        public void CopyTo(E[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(E item)
        {
            var request = ListRemoveCodec.EncodeRequest(GetName(), ToData(item));
            var response = Invoke(request);
            return ListRemoveCodec.DecodeResponse(response).response;
        }

        public int Count
        {
            get { return Size(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        protected override void OnDestroy()
        {
        }

        private ICollection<E> GetAll()
        {
            var request = ListGetAllCodec.EncodeRequest(GetName());
            var response = Invoke(request);
            var resultParameters = ListGetAllCodec.DecodeResponse(response).list;
            return resultParameters.Select(ToObject<E>).ToList();
        }

        private void HandleItemListener(IData eventData, IItemListener<E> listener, bool includeValue)
        {
            var portableItemEvent = ToObject<PortableItemEvent>(eventData);
            var item = includeValue
                ? GetContext().GetSerializationService().ToObject<E>(portableItemEvent.GetItem())
                : default(E);
            var member = GetContext().GetClusterService().GetMember(portableItemEvent.GetUuid());
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
    }
}