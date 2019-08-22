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
    class TransactionContextProxy : ITransactionContext
    {
        readonly Dictionary<ValueTuple<string,string>, ITransactionalObject> _txnObjectMap =
            new Dictionary<ValueTuple<string, string>, ITransactionalObject>(2);

        readonly HazelcastClient _client;
        readonly TransactionProxy _transaction;

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

        public string Id => _transaction.Id;

        public ITransaction BeginTransaction()
        {
            _transaction.Begin();
            return new Transaction(_transaction);
        }

        public void CommitTransaction() => throw new NotImplementedException("Obsoleted");
        public string GetTxnId() => throw new NotImplementedException("Obsoleted");
        public void RollbackTransaction() => throw new NotImplementedException("Obsoleted");

        public ITransactionalMap<TKey, TValue> GetMap<TKey, TValue>(string name)
        {
            return GetTransactionalObject<ITransactionalMap<TKey, TValue>>(ServiceNames.Map, name);
        }

        public ITransactionalQueue<T> GetQueue<T>(string name) //where E : ITransactionalObject
        {
            return GetTransactionalObject<ITransactionalQueue<T>>(ServiceNames.Queue, name);
        }

        public ITransactionalMultiMap<TKey, TValue> GetMultiMap<TKey, TValue>(string name)
        {
            return GetTransactionalObject<ITransactionalMultiMap<TKey, TValue>>(ServiceNames.MultiMap, name);
        }

        public ITransactionalList<T> GetList<T>(string name)
        {
            return GetTransactionalObject<ITransactionalList<T>>(ServiceNames.List, name);
        }

        public ITransactionalSet<T> GetSet<T>(string name)
        {
            return GetTransactionalObject<ITransactionalSet<T>>(ServiceNames.Set, name);
        }

        public T GetTransactionalObject<T>(string serviceName, string name) where T : ITransactionalObject
        {
            if (_transaction.GetState() != TransactionState.Active)
            {
                throw new TransactionNotActiveException("No transaction is found while accessing " +
                                                        "transactional object -> " + serviceName + "[" + name + "]!");
            }

            var key = ValueTuple.Create(serviceName, name);

            _txnObjectMap.TryGetValue(key, out var obj);

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

        public HazelcastClient GetClient()
        {
            return _client;
        }

        ClientTxnProxy CreateProxy<T>(string serviceName, string name)
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

        class Transaction : ITransaction
        {
            readonly TransactionProxy _transaction;
            bool _committed;

            public Transaction(TransactionProxy transaction)
            {
                _transaction = transaction;
            }

            public void Dispose()
            {
                if (_committed == false)
                {
                    Rollback();
                }
            }

            public void Commit()
            {
                _transaction.Commit(true);
                _committed = true;
            }

            public void Rollback()
            {
                _transaction.Rollback();
            }

            public string Id => _transaction.Id;
        }
    }
}