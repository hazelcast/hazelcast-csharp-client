// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientSetProxy<T> : AbstractClientCollectionProxy<T>, IHSet<T>
    {
        public ClientSetProxy(string serviceName, string name) : base(serviceName, name)
        {
        }

        public override string AddItemListener(IItemListener<T> listener, bool includeValue)
        {
            var request = SetAddListenerCodec.EncodeRequest(GetName(), includeValue, IsSmart());

            DistributedEventHandler handler = message => SetAddListenerCodec.AbstractEventHandler.Handle(message,
                (item, uuid, type) =>
                {
                    HandleItemListener(item, uuid, (ItemEventType) type, listener, includeValue);
                });

            return RegisterListener(request, m => SetAddListenerCodec.DecodeResponse(m).response,
                id => SetRemoveListenerCodec.EncodeRequest(GetName(), id), handler);
        }

        public override bool RemoveItemListener(string registrationId)
        {
            return DeregisterListener(registrationId, id => SetRemoveListenerCodec.EncodeRequest(GetName(), id));
        }

        public override bool Add(T item)
        {
            ThrowExceptionIfNull(item);
            var value = ToData(item);
            var request = SetAddCodec.EncodeRequest(GetName(), value);
            return Invoke(request, m => SetAddCodec.DecodeResponse(m).response);
        }

        public override void Clear()
        {
            var request = SetClearCodec.EncodeRequest(GetName());
            Invoke(request);
        }

        public override bool Contains(T item)
        {
            var request = SetContainsCodec.EncodeRequest(GetName(), ToData(item));
            return Invoke(request, m => SetContainsCodec.DecodeResponse(m).response);
        }

        public override bool Remove(T item)
        {
            var request = SetRemoveCodec.EncodeRequest(GetName(), ToData(item));
            return Invoke(request, m => SetRemoveCodec.DecodeResponse(m).response);
        }

        public override int Size()
        {
            var request = SetSizeCodec.EncodeRequest(GetName());
            return Invoke(request, m => SetSizeCodec.DecodeResponse(m).response);
        }

        public override bool IsEmpty()
        {
            var request = SetIsEmptyCodec.EncodeRequest(GetName());
            return Invoke(request, m => SetIsEmptyCodec.DecodeResponse(m).response);
        }

        public override bool ContainsAll<TE>(ICollection<TE> c)
        {
            var values = ToDataList(c);
            var request = SetContainsAllCodec.EncodeRequest(GetName(), values);
            return Invoke(request, m => SetContainsAllCodec.DecodeResponse(m).response);
        }

        public override bool RemoveAll<TE>(ICollection<TE> c)
        {
            var values = ToDataList(c);
            var request = SetCompareAndRemoveAllCodec.EncodeRequest(GetName(), values);
            return Invoke(request, m => SetCompareAndRemoveAllCodec.DecodeResponse(m).response);
        }

        public override bool RetainAll<TE>(ICollection<TE> c)
        {
            var values = ToDataList(c);
            var request = SetCompareAndRetainAllCodec.EncodeRequest(GetName(), values);
            return Invoke(request, m => SetCompareAndRetainAllCodec.DecodeResponse(m).response);
        }

        public override bool AddAll<TE>(ICollection<TE> c)
        {
            var values = ToDataList(c);
            var request = SetAddAllCodec.EncodeRequest(GetName(), values);
            return Invoke(request, m => SetAddAllCodec.DecodeResponse(m).response);
        }

        protected override ICollection<T> GetAll()
        {
            var request = SetGetAllCodec.EncodeRequest(GetName());
            var result = Invoke(request, m => SetGetAllCodec.DecodeResponse(m).response);
            return ToList<T>(result);
        }
    }
}