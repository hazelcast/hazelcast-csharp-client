using System;
using System.Runtime.Remoting;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Client.Spi;
using Hazelcast.IO.Serialization;
using Hazelcast.Partition.Strategy;
using Hazelcast.Transaction;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal abstract class ClientTxnProxy : ITransactionalObject
    {
        internal readonly string objectName;

        internal readonly TransactionContextProxy proxy;

        internal ClientTxnProxy(string objectName, TransactionContextProxy proxy)
        {
            this.objectName = objectName;
            this.proxy = proxy;
        }

        public void Destroy()
        {
            OnDestroy();
            var request = new ClientDestroyRequest(objectName, GetServiceName());
            Invoke<object>(request);
        }

        public virtual string GetPartitionKey()
        {
            return StringPartitioningStrategy.GetPartitionKey(GetName());
        }

        public abstract string GetName();

        public abstract string GetServiceName();

        public virtual object GetId()
        {
            return objectName;
        }

        protected virtual T Invoke<T>(ClientRequest request)
        {
            return proxy.transaction.Invoke<T>(request);
        }

        internal abstract void OnDestroy();

        internal virtual IData ToData(object obj)
        {
            return proxy.GetClient().GetSerializationService().ToData(obj);
        }

        internal virtual E ToObject<E>(IData data)
        {
            return proxy.GetClient().GetSerializationService().ToObject<E>(data);
        }
    }
}