using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Core;

namespace Hazelcast.Client.Test
{
    public class LifecycleListener : ILifecycleListener
    {
        private readonly Action<LifecycleEvent> _action;

        public LifecycleListener(Action<LifecycleEvent> action)
        {
            _action = action;
        }

        public void StateChanged(LifecycleEvent lifecycleEvent)
        {
            _action(lifecycleEvent);
        }
    }

    public class EntryListener<K, V> : IEntryListener<K, V>
    {
        public Action<EntryEvent<K, V>> EntryAddedAction { get; set; }
        public Action<EntryEvent<K, V>> EntryUpdatedAction { get; set; }
        public Action<EntryEvent<K, V>> EntryRemovedAction { get; set; }
        public Action<EntryEvent<K, V>> EntryEvictedAction { get; set; }
        public Action<MapEvent> MapEvictedAction { get; set; }
        public Action<MapEvent> MapClearedAction { get; set; }

        public void EntryAdded(EntryEvent<K, V> @event)
        {
            if (EntryAddedAction != null)
            {
                EntryAddedAction(@event);
            }
        }

        public void EntryRemoved(EntryEvent<K, V> @event)
        {
            if (EntryRemovedAction != null)
            {
                EntryRemovedAction(@event);
            }
        }

        public void EntryUpdated(EntryEvent<K, V> @event)
        {
            if (EntryUpdatedAction != null)
            {
                EntryUpdatedAction(@event);
            }
        }

        public void EntryEvicted(EntryEvent<K, V> @event)
        {
            if (EntryEvictedAction != null)
            {
                EntryEvictedAction(@event);
            }
        }

        public void MapEvicted(MapEvent @event)
        {
            if (MapEvictedAction != null)
            {
                MapEvictedAction(@event);
            }
        }

        public void MapCleared(MapEvent @event)
        {
            if (MapClearedAction != null)
            {
                MapClearedAction(@event);
            }
        }
    }
}
