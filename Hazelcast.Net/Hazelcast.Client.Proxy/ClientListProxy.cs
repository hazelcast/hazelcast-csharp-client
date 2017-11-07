// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    //.NET reviewed
    internal class ClientListProxy<T> : AbstractClientCollectionProxy<T>, IHList<T>
    {
        public ClientListProxy(string serviceName, string objectName) : base(serviceName, objectName)
        {
        }

        public int IndexOf(T item)
        {
            ThrowExceptionIfNull(item);
            var value = ToData(item);
            var request = ListIndexOfCodec.EncodeRequest(GetName(), value);
            var response = Invoke(request);
            return ListIndexOfCodec.DecodeResponse(response).response;
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
            var request = ListGetCodec.EncodeRequest(GetName(), index);
            var response = Invoke(request);
            return ToObject<T>(ListGetCodec.DecodeResponse(response).response);
        }

        public T Set(int index, T element)
        {
            ThrowExceptionIfNull(element);
            var value = ToData(element);
            var request = ListSetCodec.EncodeRequest(GetName(), index, value);
            var response = Invoke(request);
            return ToObject<T>(ListSetCodec.DecodeResponse(response).response);
        }

        public void Add(int index, T element)
        {
            ThrowExceptionIfNull(element);
            var value = ToData(element);
            var request = ListAddWithIndexCodec.EncodeRequest(GetName(), index, value);
            Invoke(request);
        }

        public T Remove(int index)
        {
            var request = ListRemoveWithIndexCodec.EncodeRequest(GetName(), index);
            var result = Invoke(request, m => ListRemoveWithIndexCodec.DecodeResponse(m).response);
            return ToObject<T>(result);
        }

        public int LastIndexOf(T item)
        {
            ThrowExceptionIfNull(item);
            var value = ToData(item);
            var request = ListLastIndexOfCodec.EncodeRequest(GetName(), value);
            var response = Invoke(request);
            return ListLastIndexOfCodec.DecodeResponse(response).response;
        }

        public bool AddAll<TE>(int index, ICollection<TE> c) where TE : T
        {
            var valueList = ToDataList(c);
            var request = ListAddAllWithIndexCodec.EncodeRequest(GetName(), index, valueList);
            var response = Invoke(request);
            return ListAddAllWithIndexCodec.DecodeResponse(response).response;
        }

        public virtual IList<T> SubList(int fromIndex, int toIndex)
        {
            var request = ListSubCodec.EncodeRequest(GetName(), fromIndex, toIndex);
            var response = Invoke(request);
            ICollection<IData> collection = ListSubCodec.DecodeResponse(response).response;
            return ToList<T>(collection);
        }

        public override string AddItemListener(IItemListener<T> listener, bool includeValue)
        {
            var request = ListAddListenerCodec.EncodeRequest(GetName(), includeValue, IsSmart());

            DistributedEventHandler handler = message => 
                ListAddListenerCodec.AbstractEventHandler.Handle(message,
                    (item, uuid, type) =>
                    {
                        HandleItemListener(item, uuid, (ItemEventType) type, listener, includeValue);
                    });

            return RegisterListener(request, m => ListAddListenerCodec.DecodeResponse(m).response,
                id => ListRemoveListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public override bool RemoveItemListener(string registrationId)
        {
            return DeregisterListener(registrationId, id => ListRemoveListenerCodec.EncodeRequest(GetName(), id));
        }

        public override bool Add(T item)
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

        public override bool ContainsAll<TE>(ICollection<TE> c)
        {
            var valueSet = ToDataList(c);
            var request = ListContainsAllCodec.EncodeRequest(GetName(), valueSet);
            return Invoke(request, m => ListContainsAllCodec.DecodeResponse(m).response);
        }

        public override bool RemoveAll<TE>(ICollection<TE> c)
        {
            var valueSet = ToDataList(c);
            var request = ListCompareAndRemoveAllCodec.EncodeRequest(GetName(), valueSet);
            return Invoke(request, m => ListCompareAndRemoveAllCodec.DecodeResponse(m).response);
        }

        public override bool RetainAll<TE>(ICollection<TE> c)
        {
            var valueSet = ToDataList(c);
            var request = ListCompareAndRetainAllCodec.EncodeRequest(GetName(), valueSet);
            return Invoke(request, m => ListCompareAndRetainAllCodec.DecodeResponse(m).response);
        }

        public override bool AddAll<TE>(ICollection<TE> c)
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

        public override bool Contains(T item)
        {
            var request = ListContainsCodec.EncodeRequest(GetName(), ToData(item));
            return Invoke(request, m => ListContainsCodec.DecodeResponse(m).response);
        }

        public override void CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(T item)
        {
            var request = ListRemoveCodec.EncodeRequest(GetName(), ToData(item));
            return Invoke(request, m => ListRemoveCodec.DecodeResponse(m).response);
        }

        protected override ICollection<T> GetAll()
        {
            var request = ListGetAllCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => ListGetAllCodec.DecodeResponse(m).response);
            return ToList<T>(result);
        }
    }
}