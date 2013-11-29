using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client
{
    public sealed class LifecycleService : ILifecycleService
    {
        private readonly AtomicBoolean active = new AtomicBoolean(false);
        private readonly HazelcastClient client;

        private readonly ConcurrentDictionary<string, ILifecycleListener> lifecycleListeners =
            new ConcurrentDictionary<string, ILifecycleListener>();

        private readonly object lifecycleLock = new object();

        public LifecycleService(HazelcastClient client)
        {
            this.client = client;
            IList<ListenerConfig> listenerConfigs = client.GetClientConfig().GetListenerConfigs();
            if (listenerConfigs != null && listenerConfigs.Count > 0)
            {
                foreach (ListenerConfig listenerConfig in listenerConfigs)
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
            string id = Guid.NewGuid().ToString();
            lifecycleListeners.TryAdd(id, lifecycleListener);
            return id;
        }

        public bool RemoveLifecycleListener(string registrationId)
        {
            ILifecycleListener returned;
            lifecycleListeners.TryRemove(registrationId, out returned);
            return returned != null;
        }

        public bool IsRunning()
        {
            return active.Get();
        }

        public void Shutdown()
        {
            active.Set(false);
            lock (lifecycleLock)
            {
                FireLifecycleEvent(LifecycleEvent.LifecycleState.ShuttingDown);
                client.DoShutdown();
                FireLifecycleEvent(LifecycleEvent.LifecycleState.Shutdown);
            }
        }

        public void Terminate()
        {
            Shutdown();
        }

        private ILogger GetLogger()
        {
            return Logger.GetLogger(typeof (ILifecycleService));
        }

        public void FireLifecycleEvent(LifecycleEvent.LifecycleState lifecycleState)
        {
            var lifecycleEvent = new LifecycleEvent(lifecycleState);
            GetLogger().Info("HazelcastClient[" + client.GetName() + "] is " + lifecycleEvent.GetState());
            foreach (ILifecycleListener lifecycleListener in lifecycleListeners.Values)
            {
                lifecycleListener.StateChanged(lifecycleEvent);
            }
        }

        internal void SetStarted()
        {
            active.Set(true);
            FireLifecycleEvent(LifecycleEvent.LifecycleState.Started);
        }
    }
}