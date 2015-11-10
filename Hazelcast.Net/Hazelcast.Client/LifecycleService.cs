// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client
{
    internal sealed class LifecycleService : ILifecycleService
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ILifecycleService));
        private readonly AtomicBoolean _active = new AtomicBoolean(false);
        private readonly HazelcastClient _client;

        private readonly ConcurrentDictionary<string, ILifecycleListener> _lifecycleListeners =
            new ConcurrentDictionary<string, ILifecycleListener>();

        private readonly object _lifecycleLock = new object();

        public LifecycleService(HazelcastClient client)
        {
            _client = client;
            var listenerConfigs = client.GetClientConfig().GetListenerConfigs();
            if (listenerConfigs != null && listenerConfigs.Count > 0)
            {
                foreach (var listenerConfig in listenerConfigs)
                {
                    if (listenerConfig.GetImplementation() is ILifecycleListener)
                    {
                        AddLifecycleListener((ILifecycleListener) listenerConfig.GetImplementation());
                    }
                }
            }
            FireLifecycleEvent(LifecycleEvent.LifecycleState.Starting);
        }

        public string AddLifecycleListener(ILifecycleListener lifecycleListener)
        {
            var id = Guid.NewGuid().ToString();
            _lifecycleListeners.TryAdd(id, lifecycleListener);
            return id;
        }

        public bool RemoveLifecycleListener(string registrationId)
        {
            ILifecycleListener returned;
            _lifecycleListeners.TryRemove(registrationId, out returned);
            return returned != null;
        }

        public bool IsRunning()
        {
            return _active.Get();
        }

        public void Shutdown()
        {
            _active.Set(false);
            lock (_lifecycleLock)
            {
                FireLifecycleEvent(LifecycleEvent.LifecycleState.ShuttingDown);
                _client.DoShutdown();
                FireLifecycleEvent(LifecycleEvent.LifecycleState.Shutdown);
            }
        }

        public void Terminate()
        {
            Shutdown();
        }

        public void FireLifecycleEvent(LifecycleEvent.LifecycleState lifecycleState)
        {
            var lifecycleEvent = new LifecycleEvent(lifecycleState);
            Logger.Info("HazelcastClient[" + _client.GetName() + "] is " + lifecycleEvent.GetState());
            foreach (var entry in _lifecycleListeners)
            {
                entry.Value.StateChanged(lifecycleEvent);
            }
        }

        internal void SetStarted()
        {
            _active.Set(true);
            FireLifecycleEvent(LifecycleEvent.LifecycleState.Started);
        }
    }
}