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

using System.Collections.Generic;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;

namespace Hazelcast.Client.Proxy
{
    internal class ClientTxnMultiMapProxy<TKey, TValue> : ClientTxnProxy, ITransactionalMultiMap<TKey, TValue>
    {
        public ClientTxnMultiMapProxy(string name, TransactionContextProxy proxy) : base(name, proxy)
        {
        }

        /// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        public virtual bool Put(TKey key, TValue value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = TransactionalMultiMapPutCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData, valueData);

            return Invoke(request, m => TransactionalMultiMapPutCodec.DecodeResponse(m).response);
        }

        public virtual ICollection<TValue> Get(TKey key)
        {
            var keyData = ToData(key);
            var request = TransactionalMultiMapGetCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData);
            var list = Invoke(request, m => TransactionalMultiMapGetCodec.DecodeResponse(m).response);
            return ToList<TValue>(list);
        }

        public virtual bool Remove(object key, object value)
        {
            var keyData = ToData(key);
            var valueData = ToData(value);
            var request = TransactionalMultiMapRemoveEntryCodec.EncodeRequest(GetName(), GetTransactionId(),
                GetThreadId(),
                keyData, valueData);

            return Invoke(request, m => TransactionalMultiMapRemoveEntryCodec.DecodeResponse(m).response);
        }

        public virtual ICollection<TValue> Remove(object key)
        {
            var keyData = ToData(key);
            var request = TransactionalMultiMapRemoveCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId(),
                keyData);

            var result = Invoke(request, m => TransactionalMultiMapRemoveCodec.DecodeResponse(m).response);
            return ToList<TValue>(result);
        }

        public virtual int ValueCount(TKey key)
        {
            var keyData = ToData(key);
            var request = TransactionalMultiMapValueCountCodec.EncodeRequest(GetName(), GetTransactionId(),
                GetThreadId(),
                keyData);

            return Invoke(request, m => TransactionalMultiMapValueCountCodec.DecodeResponse(m).response);
        }

        public virtual int Size()
        {
            var request = TransactionalMultiMapSizeCodec.EncodeRequest(GetName(), GetTransactionId(), GetThreadId());
            return Invoke(request, m => TransactionalMultiMapSizeCodec.DecodeResponse(m).response);
        }

        public override string GetServiceName()
        {
            return ServiceNames.MultiMap;
        }

        internal override void OnDestroy()
        {
        }
    }
}