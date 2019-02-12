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

using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Transaction;

namespace Hazelcast.Client
{
    internal sealed class HazelcastClientProxy : IHazelcastInstance
    {
        private volatile HazelcastClient _client;

        internal HazelcastClientProxy(HazelcastClient client)
        {
            _client = client;
        }

        public string GetName()
        {
            return GetClient().GetName();
        }

        public IQueue<T> GetQueue<T>(string name)
        {
            return GetClient().GetQueue<T>(name);
        }

        public IRingbuffer<T> GetRingbuffer<T>(string name)
        {
            return GetClient().GetRingbuffer<T>(name);
        }

        public ITopic<T> GetTopic<T>(string name)
        {
            return GetClient().GetTopic<T>(name);
        }

        public IHSet<T> GetSet<T>(string name)
        {
            return GetClient().GetSet<T>(name);
        }

        public IHList<T> GetList<T>(string name)
        {
            return GetClient().GetList<T>(name);
        }

        public IMap<TKey, TValue> GetMap<TKey, TValue>(string name)
        {
            return GetClient().GetMap<TKey, TValue>(name);
        }

        public IMultiMap<TKey, TValue> GetMultiMap<TKey, TValue>(string name)
        {
            return GetClient().GetMultiMap<TKey, TValue>(name);
        }

        public IReplicatedMap<TKey, TValue> GetReplicatedMap<TKey, TValue>(string name)
        {
            return GetClient().GetReplicatedMap<TKey, TValue>(name);
        }

        public ILock GetLock(string key)
        {
            return GetClient().GetLock(key);
        }

        public ICluster GetCluster()
        {
            return GetClient().GetCluster();
        }

        public IEndpoint GetLocalEndpoint()
        {
            return GetClient().GetLocalEndpoint();
        }

        public ITransactionContext NewTransactionContext()
        {
            return GetClient().NewTransactionContext();
        }

        public ITransactionContext NewTransactionContext(TransactionOptions options)
        {
            return GetClient().NewTransactionContext(options);
        }

        public IIdGenerator GetIdGenerator(string name)
        {
            return GetClient().GetIdGenerator(name);
        }

        public IAtomicLong GetAtomicLong(string name)
        {
            return GetClient().GetAtomicLong(name);
        }

        public ICountDownLatch GetCountDownLatch(string name)
        {
            return GetClient().GetCountDownLatch(name);
        }

        public IPNCounter GetPNCounter(string name)
        {
            return GetClient().GetPNCounter(name);
        }

        public ISemaphore GetSemaphore(string name)
        {
            return GetClient().GetSemaphore(name);
        }

        public ICollection<IDistributedObject> GetDistributedObjects()
        {
            return GetClient().GetDistributedObjects();
        }

        public string AddDistributedObjectListener(IDistributedObjectListener distributedObjectListener)
        {
            return GetClient().AddDistributedObjectListener(distributedObjectListener);
        }

        public bool RemoveDistributedObjectListener(string registrationId)
        {
            return GetClient().RemoveDistributedObjectListener(registrationId);
        }

        public IClientService GetClientService()
        {
            return GetClient().GetClientService();
        }

        public ILifecycleService GetLifecycleService()
        {
            var hz = _client;
            return hz != null ? hz.GetLifecycleService() : new TerminatedLifecycleService();
        }

        public T GetDistributedObject<T>(string serviceName, string name) where T : IDistributedObject
        {
            return GetClient().GetDistributedObject<T>(serviceName, name);
        }

        public ConcurrentDictionary<string, object> GetUserContext()
        {
            return GetClient().GetUserContext();
        }

        public void Shutdown()
        {
            GetLifecycleService().Shutdown();
        }

        public ClientConfig GetClientConfig()
        {
            return GetClient().GetClientConfig();
        }

        public ISerializationService GetSerializationService()
        {
            return GetClient().GetSerializationService();
        }

        internal HazelcastClient GetClient()
        {
            var c = _client;
            if (c == null || !c.GetLifecycleService().IsRunning())
            {
                throw new HazelcastInstanceNotActiveException();
            }
            return c;
        }
    }
}