using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Transaction;

namespace Hazelcast.Client
{
    public sealed class HazelcastClientProxy : IHazelcastInstance
    {
        internal volatile HazelcastClient client;

        internal HazelcastClientProxy(HazelcastClient client)
        {
            //import com.hazelcast.core.PartitionService;
            this.client = client;
        }

        //    public Config getConfig() {
        //        return getClient().getConfig();
        //    }
        public string GetName()
        {
            return GetClient().GetName();
        }

        public IQueue<E> GetQueue<E>(string name)
        {
            return GetClient().GetQueue<E>(name);
        }

        public ITopic<E> GetTopic<E>(string name)
        {
            return GetClient().GetTopic<E>(name);
        }

        public IHazelcastSet<E> GetSet<E>(string name)
        {
            return GetClient().GetSet<E>(name);
        }

        public IHazelcastList<E> GetList<E>(string name)
        {
            return GetClient().GetList<E>(name);
        }

        public IHazelcastMap<K, V> GetMap<K, V>(string name)
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

        public IClient GetLocalEndpoint()
        {
            return GetClient().GetLocalEndpoint();
        }

        public IExecutorService GetExecutorService(string name)
        {
            return GetClient().GetExecutorService(name);
        }

        /// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        public T ExecuteTransaction<T>(ITransactionalTask<T> task)
        {
            return GetClient().ExecuteTransaction(task);
        }

        /// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        public T ExecuteTransaction<T>(TransactionOptions options, ITransactionalTask<T> task)
        {
            return GetClient().ExecuteTransaction(options, task);
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

        //    public PartitionService getPartitionService() {
        //        return getClient().getPartitionService();
        //    }
        public IClientService GetClientService()
        {
            return GetClient().GetClientService();
        }

        //    public LoggingService getLoggingService() {
        //        return getClient().getLoggingService();
        //    }
        public ILifecycleService GetLifecycleService()
        {
            HazelcastClient hz = client;
            return hz != null ? hz.GetLifecycleService() : new TerminatedLifecycleService();
        }

        public T GetDistributedObject<T>(string serviceName, string name) where T : IDistributedObject
        {
            return GetClient().GetDistributedObject<T>(serviceName, name);
        }

        //[Obsolete]
        //public T GetDistributedObject<T>(string serviceName, object id) where T:IDistributedObject
        //{
        //    return GetClient().GetDistributedObject(serviceName, id);
        //}

        public ConcurrentDictionary<string, object> GetUserContext()
        {
            return GetClient().GetUserContext();
        }

        public void Shutdown()
        {
            GetLifecycleService().Shutdown();
        }

        public ILock GetLock(object key)
        {
            //TODO FIXME
            throw new NotSupportedException("Not implemented yet");
            //return GetClient().GetLock(key);
        }

        public ClientConfig GetClientConfig()
        {
            return GetClient().GetClientConfig();
        }

        public ISerializationService GetSerializationService()
        {
            return GetClient().GetSerializationService();
        }

        private HazelcastClient GetClient()
        {
            HazelcastClient c = client;
            if (c == null || !c.GetLifecycleService().IsRunning())
            {
                throw new HazelcastInstanceNotActiveException();
            }
            return c;
        }
    }
}