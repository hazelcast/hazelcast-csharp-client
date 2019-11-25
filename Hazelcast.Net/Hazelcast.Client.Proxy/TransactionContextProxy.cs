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
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Transaction;

namespace Hazelcast.Client.Proxy
{
    internal class TransactionContextProxy : ITransactionContext
    {
        private readonly IDictionary<TransactionalObjectKey, ITransactionalObject> _txnObjectMap =
            new Dictionary<TransactionalObjectKey, ITransactionalObject>(2);

        private readonly HazelcastClient _client;
        private readonly TransactionProxy _transaction;

        internal readonly IMember TxnOwnerNode;

        public TransactionContextProxy(HazelcastClient client, TransactionOptions options)
        {
            _client = client;
            var clusterService = (ClientClusterService) client.GetClientClusterService();

            TxnOwnerNode = client.GetClientConfig().GetNetworkConfig().IsSmartRouting() ?
                client.GetLoadBalancer().Next():
                clusterService.GetMember(clusterService.GetOwnerConnectionAddress());

            if (TxnOwnerNode == null)
            {
                throw new HazelcastException("Could not find matching member");
            }
            _transaction = new TransactionProxy(client, options, TxnOwnerNode);
        }

        public virtual Guid GetTxnId()
        {
            return _transaction.GetTxnId();
        }

        public virtual void BeginTransaction()
        {
            _transaction.Begin();
        }

        /// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        public virtual void CommitTransaction()
        {
            _transaction.Commit(true);
        }

        public virtual void RollbackTransaction()
        {
            _transaction.Rollback();
        }

        public virtual ITransactionalMap<TKey, TValue> GetMap<TKey, TValue>(string name)
        {
            return GetTransactionalObject<ITransactionalMap<TKey, TValue>>(ServiceNames.Map, name);
        }

        public virtual ITransactionalQueue<T> GetQueue<T>(string name) //where E : ITransactionalObject
        {
            return GetTransactionalObject<ITransactionalQueue<T>>(ServiceNames.Queue, name);
        }

        public virtual ITransactionalMultiMap<TKey, TValue> GetMultiMap<TKey, TValue>(string name)
        {
            return GetTransactionalObject<ITransactionalMultiMap<TKey, TValue>>(ServiceNames.MultiMap, name);
        }

        public virtual ITransactionalList<T> GetList<T>(string name)
        {
            return GetTransactionalObject<ITransactionalList<T>>(ServiceNames.List, name);
        }

        public virtual ITransactionalSet<T> GetSet<T>(string name)
        {
            return GetTransactionalObject<ITransactionalSet<T>>(ServiceNames.Set, name);
        }

        public virtual T GetTransactionalObject<T>(string serviceName, string name) where T : ITransactionalObject
        {
            if (_transaction.GetState() != TransactionState.Active)
            {
                throw new TransactionNotActiveException("No transaction is found while accessing " +
                                                        "transactional object -> " + serviceName + "[" + name + "]!");
            }
            var key = new TransactionalObjectKey(serviceName, name);
            ITransactionalObject obj;
            _txnObjectMap.TryGetValue(key, out obj);
            if (obj == null)
            {
                obj = CreateProxy<T>(serviceName, name);
                if (obj == null)
                {
                    throw new ArgumentException("Service[" + serviceName + "] is not transactional!");
                }
                _txnObjectMap.Add(key, obj);
            }
            return (T) obj;
        }

        public virtual HazelcastClient GetClient()
        {
            return _client;
        }

        private ClientTxnProxy CreateProxy<T>(string serviceName, string name)
        {
            Type proxyType;
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

            var genericTypeArguments = typeof (T).GetGenericArguments();
            var mgType = proxyType.MakeGenericType(genericTypeArguments);
            return Activator.CreateInstance(mgType, name, this) as ClientTxnProxy;
        }
    }

    internal class TransactionalObjectKey
    {
        private readonly string _name;
        private readonly string _serviceName;

        internal TransactionalObjectKey(string serviceName, string name)
        {
            _serviceName = serviceName;
            _name = name;
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
            if (!_name.Equals(that._name))
            {
                return false;
            }
            if (!_serviceName.Equals(that._serviceName))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var result = _serviceName.GetHashCode();
            result = 31*result + _name.GetHashCode();
            return result;
        }
    }
}