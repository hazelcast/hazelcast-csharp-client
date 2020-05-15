using System;
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Networking;

namespace Hazelcast.Clustering
{
    // partial: event triggers
    public partial class Cluster
    {
        private void OnObjectLifecycleEvent(ClusterObjectLifecycleEventType eventType, ClusterObjectLifecycleEventArgs args)
        {
            ForEachHandler<ClusterObjectLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == eventType)
                    handler.Handle(this, args);
            });
        }

        private void OnMemberLifecycleEvent(ClusterMemberLifecycleEventType eventType, ClusterMemberLifecycleEventArgs args)
        {
            ForEachHandler<ClusterMemberLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == eventType)
                    handler.Handle(this, args);
            });
        }

        // FIXME not invoked!
        // should be invoked by the cluster...
        private void OnClientLifecycleEvent(ClientLifecycleState state)
        {
            ForEachHandler<ClientLifecycleEventHandler>(handler =>
            {
                handler.Handle(this, state);
            });
        }

        private void OnPartitionsUpdated(EventArgs args)
        {
            ForEachHandler<PartitionsUpdatedEventHandler>(handler =>
            {
                handler.Handle(this, args);
            });
        }

        private void OnPartitionLost(PartitionLostEventArgs args)
        {
            ForEachHandler<PartitionLostEventHandler>(handler =>
            {
                handler.Handle(this, args);
            });
        }

        // TODO: see ListenerService:165 to understand what we do with this
        // - register stuff ??? explain!
        // and also cluster
        // - connection added = try to register
        // - connection removed = re-register to random
        // maybe we don't need this to be an event at all?

        private void OnConnectionAdded(Client client)
        {
            ForEachHandler<ConnectionLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == ConnectionLifecycleEventType.Added)
                    handler.Handle(this, (ConnectionLifecycleEventArgs) null); // FIXME: client);
            });
        }

        // FIXME not invoked!
        private void OnConnectionRemoved(Client client)
        {
            ForEachHandler<ConnectionLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == ConnectionLifecycleEventType.Removed)
                    handler.Handle(this, (ConnectionLifecycleEventArgs)null); // FIXME: client);
            });
        }

        private void ForEachHandler<THandler>(Action<THandler> action)
        {
            // FIXME could handling be async? w/ controlled scheduler?

            foreach (var (_, clusterEvents) in _clusterEvents)
            foreach (var handler in clusterEvents.Handlers.OfType<THandler>())
            {
                try
                {
                    action(handler);
                }
                catch (Exception e)
                {
                    // FIXME log, or something!
                }
            }
        }

        private void OnEventMessage(ClientMessage message)
        {
            XConsole.WriteLine(this, "Handle event message");

            // FIXME could handling be async? w/ controlled scheduler?

            if (!_correlatedSubscriptions.TryGetValue(message.CorrelationId, out var subscription))
            {
                // TODO: log a warning
                // TODO: instrumentation, keep track of missed events
                XConsole.WriteLine(this, $"No event handler for [{message.CorrelationId}]");
                return;
            }

            // FIXME try/catch?
            subscription.Handle(message);
        }
    }
}
