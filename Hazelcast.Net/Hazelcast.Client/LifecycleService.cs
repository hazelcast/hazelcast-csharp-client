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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client
{
    internal sealed class LifecycleService : ILifecycleService
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ILifecycleService));
        private readonly HazelcastClient _client;
        private readonly AtomicBoolean _active = new AtomicBoolean(false);

        private readonly ConcurrentDictionary<Guid, ILifecycleListener> _lifecycleListeners =
            new ConcurrentDictionary<Guid, ILifecycleListener>();

        //Monitor which ensures that all client components are down when shutdown() is finished.
        private readonly object _shutdownLock = new object();

        private readonly StripedTaskScheduler _eventExecutor;

        public LifecycleService(HazelcastClient client)
        {
            _client = client;
            _eventExecutor = new StripedTaskScheduler(1, 1000, client.Name + ".lifecycle.event");
        }

        public void Start(ICollection<IEventListener> configuredListeners)
        {
            ListenerService.RegisterConfigListeners<ILifecycleListener>(configuredListeners, AddLifecycleListener);
            FireLifecycleEvent(LifecycleEvent.LifecycleState.Starting);
            _active.Set(true);
            FireLifecycleEvent(LifecycleEvent.LifecycleState.Started);
        }

        public Guid AddLifecycleListener(ILifecycleListener lifecycleListener)
        {
            var id = Guid.NewGuid();
            _lifecycleListeners.TryAdd(id, lifecycleListener);
            return id;
        }

        public bool RemoveLifecycleListener(Guid registrationId)
        {
            _lifecycleListeners.TryRemove(registrationId, out var returned);
            return returned != null;
        }

        public async void FireLifecycleEvent(LifecycleEvent.LifecycleState lifecycleState)
        {
            var lifecycleEvent = new LifecycleEvent(lifecycleState);
            Logger.Info($"HazelcastClient[{_client.Name}] {VersionUtil.GetDllVersion()} is {lifecycleEvent.GetState()}");

            await Task.Factory.StartNew(() =>
            {
                foreach (var entry in _lifecycleListeners)
                {
                    try
                    {
                        entry.Value.StateChanged(lifecycleEvent);
                    }
                    catch (Exception e)
                    {
                        if (Logger.IsFinestEnabled)
                        {
                            Logger.Finest("Exception occured in a Lifecycle listeners", e);
                        }
                    }
                }
            }, Task.Factory.CancellationToken, Task.Factory.CreationOptions, _eventExecutor).IgnoreExceptions();
        }

        public bool IsRunning()
        {
            return _active.Get();
        }

        public void Shutdown()
        {
            _client.OnGracefulShutdown();
            DoShutdown();
        }

        public void Terminate()
        {
            DoShutdown();
        }

        private void DoShutdown()
        {
            lock (_shutdownLock)
            {
                if (!_active.CompareAndSet(true, false))
                {
                    return;
                }
                FireLifecycleEvent(LifecycleEvent.LifecycleState.ShuttingDown);
                _client.DoShutdown();
                FireLifecycleEvent(LifecycleEvent.LifecycleState.Shutdown);
            }
        }
    }
}