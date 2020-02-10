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
using Hazelcast.Client.Proxy;
using Hazelcast.Core;
using Hazelcast.Transaction;

namespace Hazelcast.Client
{
    public sealed partial class HazelcastClient : IHazelcastInstance
    {
        string IHazelcastInstance.Name => Name;

        Guid IHazelcastInstance.ClientGuid => ClientGuid;

        ICluster IHazelcastInstance.Cluster => ClusterService;

        ILifecycleService IHazelcastInstance.LifecycleService => LifecycleService;

        IPartitionService IHazelcastInstance.PartitionService => PartitionService;

        Guid IHazelcastInstance.AddDistributedObjectListener(IDistributedObjectListener distributedObjectListener)
        {
            return ProxyManager.AddDistributedObjectListener(distributedObjectListener);
        }

        bool IHazelcastInstance.RemoveDistributedObjectListener(Guid registrationId)
        {
            return ProxyManager.RemoveDistributedObjectListener(registrationId);
        }

        T IHazelcastInstance.GetDistributedObject<T>(string serviceName, string name)
        {
            return GetDistributedObject<T>(serviceName, name);
        }

        ICollection<IDistributedObject> IHazelcastInstance.GetDistributedObjects()
        {
            return ProxyManager.GetDistributedObjects();
        }

        IPNCounter IHazelcastInstance.GetPNCounter(string name)
        {
            return GetDistributedObject<IPNCounter>(ServiceNames.PNCounter, name);
        }

        IHList<T> IHazelcastInstance.GetList<T>(string name)
        {
            return GetDistributedObject<IHList<T>>(ServiceNames.List, name);
        }

        IMap<TKey, TValue> IHazelcastInstance.GetMap<TKey, TValue>(string name)
        {
            return GetDistributedObject<IMap<TKey, TValue>>(ServiceNames.Map, name);
        }

        IMultiMap<TKey, TValue> IHazelcastInstance.GetMultiMap<TKey, TValue>(string name)
        {
            return GetDistributedObject<IMultiMap<TKey, TValue>>(ServiceNames.MultiMap, name);
        }

        IReplicatedMap<TKey, TValue> IHazelcastInstance.GetReplicatedMap<TKey, TValue>(string name)
        {
            return GetDistributedObject<IReplicatedMap<TKey, TValue>>(ServiceNames.ReplicatedMap, name);
        }

        IQueue<T> IHazelcastInstance.GetQueue<T>(string name)
        {
            return GetDistributedObject<IQueue<T>>(ServiceNames.Queue, name);
        }

        IRingbuffer<T> IHazelcastInstance.GetRingbuffer<T>(string name)
        {
            return GetDistributedObject<IRingbuffer<T>>(ServiceNames.Ringbuffer, name);
        }

        IHSet<T> IHazelcastInstance.GetSet<T>(string name)
        {
            return GetDistributedObject<IHSet<T>>(ServiceNames.Set, name);
        }

        ITopic<T> IHazelcastInstance.GetTopic<T>(string name)
        {
            return GetDistributedObject<ITopic<T>>(ServiceNames.Topic, name);
        }

        ITransactionContext IHazelcastInstance.NewTransactionContext()
        {
            return NewTransactionContext(TransactionOptions.GetDefault());
        }

        ITransactionContext IHazelcastInstance.NewTransactionContext(TransactionOptions options)
        {
            return NewTransactionContext(options);
        }

        void IHazelcastInstance.Shutdown()
        {
            LifecycleService.Shutdown();
        }

        void IDisposable.Dispose()
        {
            LifecycleService.Shutdown();
        }

        private T GetDistributedObject<T>(string serviceName, string name)
        {
            var clientProxy = ProxyManager.GetOrCreateProxy<T>(serviceName, name);
            return (T) ((IDistributedObject) clientProxy);
        }

        private ITransactionContext NewTransactionContext(TransactionOptions options)
        {
            return new TransactionContextProxy(this, options);
        }
    }
}