// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTxnSetProxy<T> : AbstractClientTxnCollectionProxy, ITransactionalSet<T>
    {
        public ClientTxnSetProxy(string name, TransactionContextProxy proxy)
            : base(name, proxy)
        {
        }

        public virtual bool Add(T e)
        {
            ThrowExceptionIfNull(e);
            var value = ToData(e);
            var request = TransactionalSetAddCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), value);
            return Invoke(request, m => TransactionalSetAddCodec.DecodeResponse(m).response);
        }

        public virtual bool Remove(T e)
        {
            ThrowExceptionIfNull(e);
            var value = ToData(e);
            var request = TransactionalSetRemoveCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(), value);
            return Invoke(request, m => TransactionalSetRemoveCodec.DecodeResponse(m).response);
        }

        public virtual int Size()
        {
            var request = TransactionalSetSizeCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            return Invoke(request, m => TransactionalSetSizeCodec.DecodeResponse(m).response);
        }

        public override string GetServiceName()
        {
            return ServiceNames.Set;
        }
    }
}