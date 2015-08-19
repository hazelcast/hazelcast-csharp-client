using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

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
            var result = Invoke(request, m => ListRemoveWithIndexCodec.DecodeResponse(m).response);
            return ToObject<E>(result);
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
            var valueList = ToDataList(c);
            var request = ListAddAllWithIndexCodec.EncodeRequest(GetName(), index, valueList);
            var response = Invoke(request);
            return ListAddAllWithIndexCodec.DecodeResponse(response).response;
        }

        public virtual IList<E> SubList(int fromIndex, int toIndex)
        {
            var request = ListSubCodec.EncodeRequest(GetName(), fromIndex, toIndex);
            var response = Invoke(request);
            ICollection<IData> collection = ListSubCodec.DecodeResponse(response).list;
            return ToList<E>(collection);
        }

        public override string AddItemListener(IItemListener<E> listener, bool includeValue)
        {
            var request = ListAddListenerCodec.EncodeRequest(GetName(), includeValue);

            DistributedEventHandler handler = message => ListAddListenerCodec.AbstractEventHandler.Handle(message,
                ((item, uuid, type) => { HandleItemListener(item, uuid, (ItemEventType)type, listener, includeValue); }));

            return Listen(request,
                m => ListAddListenerCodec.DecodeResponse(m).response, GetPartitionKey(), handler);
        }

        public override bool RemoveItemListener(string registrationId)
        {
            var request = ListRemoveListenerCodec.EncodeRequest(GetName(), registrationId);
            return StopListening(request, message => ListRemoveListenerCodec.DecodeResponse(message).response,
                registrationId);
        }

        public override bool Add(E item)
        {
            var request = ListAddCodec.EncodeRequest(GetName(), ToData(item));
            var response = Invoke(request);
            return ListAddCodec.DecodeResponse(response).response;
        }

        public override int Size()
        {
            var request = ListSizeCodec.EncodeRequest(GetName());
            return Invoke(request, m => ListSizeCodec.DecodeResponse(m).response);
        }

        public override bool IsEmpty()
        {
            var request = ListIsEmptyCodec.EncodeRequest(GetName());
            return Invoke(request, m => ListIsEmptyCodec.DecodeResponse(m).response);
        }

        public override bool ContainsAll<T>(ICollection<T> c)
        {
            var valueSet = ToDataSet(c);
            var request = ListContainsAllCodec.EncodeRequest(GetName(), valueSet);
            return Invoke(request, m => ListContainsAllCodec.DecodeResponse(m).response);
        }

        public override bool RemoveAll<T>(ICollection<T> c)
        {
            var valueSet = ToDataSet(c);
            var request = ListCompareAndRemoveAllCodec.EncodeRequest(GetName(), valueSet);
            return Invoke(request, m => ListCompareAndRemoveAllCodec.DecodeResponse(m).response);
        }

        public override bool RetainAll<T>(ICollection<T> c)
        {
            var valueSet = ToDataSet(c);
            var request = ListCompareAndRetainAllCodec.EncodeRequest(GetName(), valueSet);
            return Invoke(request, m => ListCompareAndRetainAllCodec.DecodeResponse(m).response);
        }

        public override bool AddAll<T>(ICollection<T> c)
        {
            var values = ToDataList(c);
            var request = ListAddAllCodec.EncodeRequest(GetName(), values);
            return Invoke(request, m => ListAddAllCodec.DecodeResponse(m).response);
        }

        public override void Clear()
        {
            var request = ListClearCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public override bool Contains(E item)
        {
            var request = ListContainsCodec.EncodeRequest(GetName(), ToData(item));
            return Invoke(request, m=> ListContainsCodec.DecodeResponse(m).response);
        }

        public override void CopyTo(E[] array, int index)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(E item)
        {
            var request = ListRemoveCodec.EncodeRequest(GetName(), ToData(item));
            return Invoke(request, m => ListRemoveCodec.DecodeResponse(m).response);
        }

        protected override ICollection<E> GetAll()
        {
            var request = ListGetAllCodec.EncodeRequest(GetName());
            var result = Invoke(request, m=> ListGetAllCodec.DecodeResponse(m).list);
            return ToList<E>(result);
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
    }
}