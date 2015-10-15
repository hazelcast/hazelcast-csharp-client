using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
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

        public IQueue<E> GetQueue<E>(string name)
        {
            return GetClient().GetQueue<E>(name);
        }

        public IRingbuffer<E> GetRingbuffer<E>(string name)
        {
            return GetClient().GetRingbuffer<E>(name);
        }

        public ITopic<E> GetTopic<E>(string name)
        {
            return GetClient().GetTopic<E>(name);
        }

        public IHSet<E> GetSet<E>(string name)
        {
            return GetClient().GetSet<E>(name);
        }

        public IHList<E> GetList<E>(string name)
        {
            return GetClient().GetList<E>(name);
        }

        public IMap<K, V> GetMap<K, V>(string name)
        {
            return GetClient().GetMap<K, V>(name);
        }

        public IMultiMap<K, V> GetMultiMap<K, V>(string name)
        {
            return GetClient().GetMultiMap<K, V>(name);
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
            HazelcastClient hz = _client;
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
            HazelcastClient c = _client;
            if (c == null || !c.GetLifecycleService().IsRunning())
            {
                throw new HazelcastInstanceNotActiveException();
            }
            return c;
        }
    }
}