using System;
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Microsoft.Extensions.Logging;

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

        // FIXME: invoke!
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

        private void OnConnectionAdded(Client client)
        {
            var args = new ConnectionLifecycleEventArgs(client);
            ForEachHandler<ConnectionLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == ConnectionLifecycleEventType.Added)
                    handler.Handle(this, args);
            });
        }

        // FIXME: invoke!
        private void OnConnectionRemoved(Client client)
        {
            var args = new ConnectionLifecycleEventArgs(client);
            ForEachHandler<ConnectionLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == ConnectionLifecycleEventType.Removed)
                    handler.Handle(this, args);
            });
        }

        private void ForEachHandler<THandler>(Action<THandler> action)
        {
            // TODO: consider async handlers + running on background threads + limiting concurrency

            foreach (var (_, clusterEvents) in _clusterHandlers)
            foreach (var handler in clusterEvents.OfType<THandler>())
            {
                try
                {
                    action(handler);
                }
                catch (Exception e)
                {
                    // TODO: instrumentation, keep track of exceptions
                    _logger.LogError(e, "Caught exception in event handler.");
                }
            }
        }

        private void OnEventMessage(ClientMessage message)
        {
            XConsole.WriteLine(this, "Handle event message");

            // FIXME: consider async handlers + running on background threads + limiting concurrency

            if (!_correlatedSubscriptions.TryGetValue(message.CorrelationId, out var subscription))
            {
                // TODO: instrumentation, keep track of missed events
                _logger.LogWarning($"No event handler for [{message.CorrelationId}]");
                XConsole.WriteLine(this, $"No event handler for [{message.CorrelationId}]");
                return;
            }

            // exceptions are handled by caller (see Client.ReceiveEvent)
            subscription.Handle(message);
        }
    }
}
