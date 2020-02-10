// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    //.NET reviewed
    internal class ClientListProxy<T> : AbstractClientCollectionProxy<T>, IHList<T>
    {
        public ClientListProxy(string serviceName, string objectName, HazelcastClient client) : base(serviceName, objectName, client)
        {
        }

        public int IndexOf(T item)
        {
            ValidationUtil.ThrowExceptionIfNull(item);
            var value = ToData(item);
            var request = ListIndexOfCodec.EncodeRequest(Name, value);
            var response = Invoke(request);
            return ListIndexOfCodec.DecodeResponse(response).Response;
        }

        public void Insert(int index, T item)
        {
            Add(index, item);
        }

        public void RemoveAt(int index)
        {
            Remove(index);
        }

        public T this[int index]
        {
            get { return Get(index); }
            set { Set(index, value); }
        }

        public T Get(int index)
        {
            var request = ListGetCodec.EncodeRequest(Name, index);
            var response = Invoke(request);
            return ToObject<T>(ListGetCodec.DecodeResponse(response).Response);
        }

        public T Set(int index, T element)
        {
            ValidationUtil.ThrowExceptionIfNull(element);
            var value = ToData(element);
            var request = ListSetCodec.EncodeRequest(Name, index, value);
            var response = Invoke(request);
            return ToObject<T>(ListSetCodec.DecodeResponse(response).Response);
        }

        public void Add(int index, T element)
        {
            ValidationUtil.ThrowExceptionIfNull(element);
            var value = ToData(element);
            var request = ListAddWithIndexCodec.EncodeRequest(Name, index, value);
            Invoke(request);
        }

        public T Remove(int index)
        {
            var request = ListRemoveWithIndexCodec.EncodeRequest(Name, index);
            var result = Invoke(request, m => ListRemoveWithIndexCodec.DecodeResponse(m).Response);
            return ToObject<T>(result);
        }

        public int LastIndexOf(T item)
        {
            ValidationUtil.ThrowExceptionIfNull(item);
            var value = ToData(item);
            var request = ListLastIndexOfCodec.EncodeRequest(Name, value);
            var response = Invoke(request);
            return ListLastIndexOfCodec.DecodeResponse(response).Response;
        }

        public bool AddAll<TE>(int index, ICollection<TE> c) where TE : T
        {
            var valueList = ToDataList(c);
            var request = ListAddAllWithIndexCodec.EncodeRequest(Name, index, valueList);
            var response = Invoke(request);
            return ListAddAllWithIndexCodec.DecodeResponse(response).Response;
        }

        public virtual IList<T> SubList(int fromIndex, int toIndex)
        {
            var request = ListSubCodec.EncodeRequest(Name, fromIndex, toIndex);
            var response = Invoke(request);
            ICollection<IData> collection = ListSubCodec.DecodeResponse(response).Response;
            return ToList<T>(collection);
        }

        public override Guid AddItemListener(IItemListener<T> listener, bool includeValue)
        {
            var request = ListAddListenerCodec.EncodeRequest(Name, includeValue, IsSmart());

            DistributedEventHandler handler = message => 
                ListAddListenerCodec.EventHandler.HandleEvent(message,
                    (item, uuid, type) =>
                    {
                        HandleItemListener(item, uuid, (ItemEventType) type, listener, includeValue);
                    });

            return RegisterListener(request, m => ListAddListenerCodec.DecodeResponse(m).Response,
                id => ListRemoveListenerCodec.EncodeRequest(Name, id), handler);
        }

        public override bool RemoveItemListener(Guid registrationId)
        {
            return DeregisterListener(registrationId);
        }

        public override bool Add(T item)
        {
            var request = ListAddCodec.EncodeRequest(Name, ToData(item));
            var response = Invoke(request);
            return ListAddCodec.DecodeResponse(response).Response;
        }

        public override int Size()
        {
            var request = ListSizeCodec.EncodeRequest(Name);
            return Invoke(request, m => ListSizeCodec.DecodeResponse(m).Response);
        }

        public override bool IsEmpty()
        {
            var request = ListIsEmptyCodec.EncodeRequest(Name);
            return Invoke(request, m => ListIsEmptyCodec.DecodeResponse(m).Response);
        }

        public override bool ContainsAll<TE>(ICollection<TE> c)
        {
            var valueSet = ToDataList(c);
            var request = ListContainsAllCodec.EncodeRequest(Name, valueSet);
            return Invoke(request, m => ListContainsAllCodec.DecodeResponse(m).Response);
        }

        public override bool RemoveAll<TE>(ICollection<TE> c)
        {
            var valueSet = ToDataList(c);
            var request = ListCompareAndRemoveAllCodec.EncodeRequest(Name, valueSet);
            return Invoke(request, m => ListCompareAndRemoveAllCodec.DecodeResponse(m).Response);
        }

        public override bool RetainAll<TE>(ICollection<TE> c)
        {
            var valueSet = ToDataList(c);
            var request = ListCompareAndRetainAllCodec.EncodeRequest(Name, valueSet);
            return Invoke(request, m => ListCompareAndRetainAllCodec.DecodeResponse(m).Response);
        }

        public override bool AddAll<TE>(ICollection<TE> c)
        {
            var values = ToDataList(c);
            var request = ListAddAllCodec.EncodeRequest(Name, values);
            return Invoke(request, m => ListAddAllCodec.DecodeResponse(m).Response);
        }

        public override void Clear()
        {
            var request = ListClearCodec.EncodeRequest(Name);
            Invoke(request);
        }

        public override bool Contains(T item)
        {
            var request = ListContainsCodec.EncodeRequest(Name, ToData(item));
            return Invoke(request, m => ListContainsCodec.DecodeResponse(m).Response);
        }

        public override void CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(T item)
        {
            var request = ListRemoveCodec.EncodeRequest(Name, ToData(item));
            return Invoke(request, m => ListRemoveCodec.DecodeResponse(m).Response);
        }

        protected override ICollection<T> GetAll()
        {
            var request = ListGetAllCodec.EncodeRequest(Name);
            var result = Invoke(request, m => ListGetAllCodec.DecodeResponse(m).Response);
            return ToList<T>(result);
        }
    }
}