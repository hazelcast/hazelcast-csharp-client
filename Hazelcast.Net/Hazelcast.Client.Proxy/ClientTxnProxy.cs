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
            var request = ClientDestroyProxyCodec.EncodeRequest(ObjectName, GetServiceName());
            Invoke(request);
        }

        public virtual string GetPartitionKey()
        {
            return StringPartitioningStrategy.GetPartitionKey(GetName());
        }

        public virtual string GetName()
        {
            return ObjectName;
        }

        public abstract string GetServiceName();
        internal abstract void OnDestroy();

        protected virtual IClientMessage Invoke(IClientMessage request)
        {
            var rpc = Proxy.GetClient().GetInvocationService();
            try
            {
                var task = rpc.InvokeOnMember(request, Proxy.TxnOwnerNode);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        protected virtual T Invoke<T>(IClientMessage request, Func<IClientMessage, T> decodeResponse)
        {
            var response = Invoke(request);
            return decodeResponse(response);
        }

        protected virtual string GetTransactionId()
        {
            return Proxy.GetTxnId();
        }

        protected virtual long GetThreadId()
        {
            return ThreadUtil.GetThreadId();
        }

        protected virtual IData ToData(object obj)
        {
            return Proxy.GetClient().GetSerializationService().ToData(obj);
        }

        protected virtual E ToObject<E>(IData data)
        {
            return Proxy.GetClient().GetSerializationService().ToObject<E>(data);
        }

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

    }
}