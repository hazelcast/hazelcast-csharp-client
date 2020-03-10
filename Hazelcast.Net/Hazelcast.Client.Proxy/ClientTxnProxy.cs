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

using System;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.IO.Serialization;
using Hazelcast.Partition.Strategy;
using Hazelcast.Transaction;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal abstract class ClientTxnProxy : ITransactionalObject
    {
        internal readonly string ObjectName;
        internal readonly TransactionContextProxy Proxy;

        internal ClientTxnProxy(string objectName, TransactionContextProxy proxy)
        {
            ObjectName = objectName;
            Proxy = proxy;
        }

        public void Destroy()
        {
            OnDestroy();
            var request = ClientDestroyProxyCodec.EncodeRequest(ObjectName, ServiceName);
            Invoke(request);
        }

        public virtual string GetPartitionKey()
        {
            return StringPartitioningStrategy.GetPartitionKey(Name);
        }

        public virtual string Name
        {
            get { return ObjectName; }
        }

        public abstract string ServiceName { get; }

        protected internal virtual IList<T> ToList<T>(ICollection<IData> dataList)
        {
            var list = new List<T>(dataList.Count);
            foreach (var data in dataList)
            {
                list.Add(ToObject<T>(data));
            }
            return list;
        }

        protected internal virtual ISet<T> ToSet<T>(ICollection<IData> dataList)
        {
            var set = new HashSet<T>();
            foreach (var data in dataList)
            {
                set.Add(ToObject<T>(data));
            }
            return set;
        }

        protected virtual long GetThreadId()
        {
            return ThreadUtil.GetThreadId();
        }

        protected virtual Guid GetTransactionId()
        {
            return Proxy.GetTxnId();
        }

        protected virtual ClientMessage Invoke(ClientMessage request)
        {
            var rpc = Proxy.GetClient().InvocationService;
            try
            {
                var task = rpc.InvokeOnConnection(request, Proxy.TxnConnection);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e, exception => new TransactionException(exception));
            }
        }

        protected virtual T Invoke<T>(ClientMessage request, Func<ClientMessage, T> decodeResponse)
        {
            var response = Invoke(request);
            return decodeResponse(response);
        }

        protected virtual IData ToData(object obj)
        {
            return Proxy.GetClient().SerializationService.ToData(obj);
        }

        protected virtual TE ToObject<TE>(IData data)
        {
            return Proxy.GetClient().SerializationService.ToObject<TE>(data);
        }

        internal abstract void OnDestroy();
    }
}