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
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Client.Network;
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.NearCache;
using Hazelcast.Partition.Strategy;
using Hazelcast.Security;
using Hazelcast.Util;

namespace Hazelcast.Client
{
    public sealed partial class HazelcastClient
    {
        private readonly int _id = ClientId.GetAndIncrement();
        private readonly string _instanceName;

        private readonly ConcurrentQueue<IDisposable> _onClusterChangeDisposables = new ConcurrentQueue<IDisposable>();
        private readonly ConcurrentQueue<IDisposable> _onClientShutdownDisposables = new ConcurrentQueue<IDisposable>();

        internal string Name => _instanceName;
        
        internal LifecycleService LifecycleService { get; }

        internal Guid ClientGuid { get; } = Guid.NewGuid();
        internal ClusterService ClusterService { get; }

        internal ClientConfig ClientConfig { get; }

        internal ExecutionService ExecutionService { get; }

        internal PartitionService PartitionService { get; }

        internal ConnectionManager ConnectionManager { get; }

        internal InvocationService InvocationService { get; }

        internal ListenerService ListenerService { get; }

        internal ISerializationService SerializationService { get; }

        internal ClientLockReferenceIdGenerator LockReferenceIdGenerator { get; }

        internal NearCacheManager NearCacheManager { get; }

        internal ProxyManager ProxyManager { get; }

        // internal Statistics Statistics { get; }

        internal AddressProvider AddressProvider { get; }

        internal ICredentialsFactory CredentialsFactory { get; }

        internal ILoadBalancer LoadBalancer { get; }

        private HazelcastClient(ClientConfig config)
        {
            ClientConfig = config;
            if (config.InstanceName != null)
            {
                _instanceName = config.InstanceName;
            }
            else
            {
                _instanceName = "hz.client_" + _id;
            }
            LifecycleService = new LifecycleService(this);
            try
            {
                //TODO make partition strategy parametric                
                var partitioningStrategy = new DefaultPartitioningStrategy();
                SerializationService = new SerializationServiceBuilder().SetConfig(config.GetSerializationConfig())
                    .SetPartitioningStrategy(partitioningStrategy)
                    .SetVersion(IO.Serialization.SerializationService.SerializerVersion).Build();
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            ProxyManager = new ProxyManager(this);
            //TODO EXECUTION SERVICE
            ExecutionService = new ExecutionService(_instanceName, config.GetExecutorPoolSize());
            LoadBalancer = config.GetLoadBalancer() ?? new RoundRobinLB();
            PartitionService = new PartitionService(this);
            AddressProvider = new AddressProvider(ClientConfig);
            ConnectionManager = new ConnectionManager(this);
            InvocationService = new InvocationService(this);
            ListenerService = new ListenerService(this);
            ClusterService = new ClusterService(this);
            LockReferenceIdGenerator = new ClientLockReferenceIdGenerator();
            // Statistics = new Statistics(this);
            NearCacheManager = new NearCacheManager(this);
            CredentialsFactory = config.GetSecurityConfig().CredentialsFactoryConfig.GetCredentialsFactory();
        }

        private void Start()
        {
            try
            {
                var configuredListeners = InstantiateConfiguredListenerObjects();
                LifecycleService.Start(configuredListeners);
                InvocationService.Start();
                ClusterService.Start(configuredListeners);
                ConnectionManager.Start();
                ClusterService.WaitInitialMemberListFetched();
                ConnectionManager.ConnectToAllClusterMembers();
                ListenerService.Start();
                ProxyManager.Init(ClientConfig);
                LoadBalancer.Init(((IHazelcastInstance) this).Cluster, ClientConfig);
                // Statistics.Start();
                AddClientConfigAddedListeners(configuredListeners);
            }
            catch (Exception e)
            {
                try
                {
                    LifecycleService.Terminate();
                }
                catch (Exception)
                {
                    //ignore
                }
                throw ExceptionUtil.Rethrow(e);
            }
        }

        internal void OnGracefulShutdown()
        {
            //proxySessionManager.shutdownAndAwait();
        }

        internal void DoShutdown()
        {
            Clients.TryRemove(_instanceName, out _);
            DisposeAll(_onClientShutdownDisposables);
            // Statistics.Destroy();
            ExecutionService.Shutdown();
            ConnectionManager.Shutdown();
            ProxyManager.Destroy();
            InvocationService.Shutdown();
            NearCacheManager.Shutdown();
            ListenerService.Dispose();
            SerializationService.Destroy();
            CredentialsFactory.Dispose();
        }
        
        internal void OnClusterRestart() {
            DisposeAll(_onClusterChangeDisposables);
            //clear the member list version
            ClusterService.ClearMemberListVersion();
        }

        internal void DisposeOnClusterChange(IDisposable disposable) {
            _onClusterChangeDisposables.Enqueue(disposable);
        }

        internal void DisposeOnClientShutdown(IDisposable disposable) {
            _onClientShutdownDisposables.Enqueue(disposable);
        }

        internal void SendStateToCluster()
        {
            ProxyManager.CreateDistributedObjectsOnCluster();
        }

        private static void DisposeAll(ConcurrentQueue<IDisposable> queue) {
            while (queue.TryDequeue(out var disposable)) disposable.Dispose();
        }


        private ICollection<IEventListener> InstantiateConfiguredListenerObjects()
        {
            var listeners = new List<IEventListener>();
            var listenerConfigs = ClientConfig.GetListenerConfigs();
            foreach (var listenerConfig in listenerConfigs)
            {
                var listener = listenerConfig.GetImplementation();
                if (listener == null)
                {
                    try
                    {
                        var className = listenerConfig.GetClassName();
                        var type = Type.GetType(className);
                        if (type != null)
                        {
                            listener = Activator.CreateInstance(type) as IEventListener;
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Logger.GetLogger(typeof(IHazelcastInstance)).Severe(e);
                    }
                }
                listeners.Add(listener);
            }
            return listeners;
        }

        private void AddClientConfigAddedListeners(ICollection<IEventListener> configuredListeners)
        {
            ListenerService.RegisterConfigListeners<IDistributedObjectListener>(configuredListeners,
                ProxyManager.AddDistributedObjectListener);
            ListenerService.RegisterConfigListeners<IPartitionLostListener>(configuredListeners,
                PartitionService.AddPartitionLostListener);
        }
    }
}