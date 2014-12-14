using System;
using System.Collections.Generic;
using System.IO;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Proxy;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Transaction;

namespace Hazelcast.Client.Request.Transaction
{
    internal class TransactionContextProxy : ITransactionContext
    {
        internal const int txOwnerNodeTryCount = 5;

        internal readonly HazelcastClient client;

        internal Address _txnOwnerNode;
        internal readonly TransactionProxy transaction;

        private readonly IDictionary<TransactionalObjectKey, ITransactionalObject> txnObjectMap =
            new Dictionary<TransactionalObjectKey, ITransactionalObject>(2);

        public TransactionContextProxy(HazelcastClient client, TransactionOptions options)
        {
            this.client = client;
            _txnOwnerNode = client.GetConnectionManager().BindToRandomAddress();

            if (_txnOwnerNode == null)
            {
                throw new HazelcastException("Could not obtain Connection!!!");
            }
            transaction = new TransactionProxy(client, options, _txnOwnerNode);
        }

        public virtual string GetTxnId()
        {
            return transaction.GetTxnId();
        }

        public virtual void BeginTransaction()
        {
            transaction.Begin();
        }

        /// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        public virtual void CommitTransaction()
        {
            transaction.Commit(true);
        }

        public virtual void RollbackTransaction()
        {
            transaction.Rollback();
        }

        public virtual ITransactionalMap<K, V> GetMap<K, V>(string name)
        {
            return GetTransactionalObject<ITransactionalMap<K, V>>(ServiceNames.Map, name);
        }

        public virtual ITransactionalQueue<E> GetQueue<E>(string name) //where E : ITransactionalObject
        {
            return GetTransactionalObject<ITransactionalQueue<E>>(ServiceNames.Queue, name);
        }

        public virtual ITransactionalMultiMap<K, V> GetMultiMap<K, V>(string name)
        {
            return GetTransactionalObject<ITransactionalMultiMap<K, V>>(ServiceNames.MultiMap, name);
        }

        public virtual ITransactionalList<E> GetList<E>(string name)
        {
            return GetTransactionalObject<ITransactionalList<E>>(ServiceNames.List, name);
        }

        public virtual ITransactionalSet<E> GetSet<E>(string name)
        {
            return GetTransactionalObject<ITransactionalSet<E>>(ServiceNames.Set, name);
        }

        public virtual T GetTransactionalObject<T>(string serviceName, string name) where T : ITransactionalObject
        {
            if (transaction.GetState() != TransactionState.Active)
            {
                throw new TransactionNotActiveException("No transaction is found while accessing " +
                                                        "transactional object -> " + serviceName + "[" + name + "]!");
            }
            var key = new TransactionalObjectKey(this, serviceName, name);
            ITransactionalObject obj = null;
            txnObjectMap.TryGetValue(key, out obj);
            if (obj == null)
            {
                obj = CreateProxy<T>(serviceName, name);
                if (obj == null)
                {
                    throw new ArgumentException("Service[" + serviceName + "] is not transactional!");
                }
                txnObjectMap.Add(key, obj);
            }
            return (T) obj;
        }

        private ClientTxnProxy CreateProxy<T>(String serviceName, string name)
        {
            Type proxyType = null;
            switch (serviceName)
            {
                case ServiceNames.Queue:
                    proxyType = typeof (ClientTxnQueueProxy<>);
                    break;
                case ServiceNames.Map:
                    proxyType = typeof (ClientTxnMapProxy<,>);
                    break;
                case ServiceNames.MultiMap:
                    proxyType = typeof (ClientTxnMultiMapProxy<,>);
                    break;
                case ServiceNames.List:
                    proxyType = typeof (ClientTxnListProxy<>);
                    break;
                case ServiceNames.Set:
                    proxyType = typeof (ClientTxnSetProxy<>);
                    break;
                default:
                    throw new ArgumentException("Service[" + serviceName + "] is not transactional!");
            }
            if (proxyType != null)
            {
                Type[] genericTypeArguments = typeof (T).GetGenericArguments();
                Type mgType = proxyType.MakeGenericType(genericTypeArguments);
                return Activator.CreateInstance(mgType, new object[] {name, this}) as ClientTxnProxy;
            }
            return null;
        }

        public virtual HazelcastClient GetClient()
        {
            return client;
        }

        public Address TxnOwner
        {
            get { return _txnOwnerNode; }
        }

    }

    internal class TransactionalObjectKey
    {
        private readonly TransactionContextProxy _enclosing;
        private readonly string name;
        private readonly string serviceName;

        internal TransactionalObjectKey(TransactionContextProxy _enclosing, string serviceName, string name)
        {
            this._enclosing = _enclosing;
            this.serviceName = serviceName;
            this.name = name;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (!(o is TransactionalObjectKey))
            {
                return false;
            }
            var that = (TransactionalObjectKey) o;
            if (!name.Equals(that.name))
            {
                return false;
            }
            if (!serviceName.Equals(that.serviceName))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result = serviceName.GetHashCode();
            result = 31*result + name.GetHashCode();
            return result;
        }
    }
}