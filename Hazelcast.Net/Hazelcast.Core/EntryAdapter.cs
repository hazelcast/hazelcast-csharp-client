using System;

namespace Hazelcast.Core
{
    public class EntryAdapter<K, V> : IEntryListener<K, V>
    {
        private readonly Action<EntryEvent<K, V>> fAdded;
        private readonly Action<EntryEvent<K, V>> fEvicted;
        private readonly Action<EntryEvent<K, V>> fRemoved;
        private readonly Action<EntryEvent<K, V>> fUpdated;

        public EntryAdapter(Action<EntryEvent<K, V>> fAdded, Action<EntryEvent<K, V>> fRemoved,
            Action<EntryEvent<K, V>> fUpdated, Action<EntryEvent<K, V>> fEvicted)
        {
            this.fAdded = fAdded;
            this.fRemoved = fRemoved;
            this.fUpdated = fUpdated;
            this.fEvicted = fEvicted;
        }


        public void EntryAdded(EntryEvent<K, V> @event)
        {
            fAdded(@event);
        }

        public void EntryRemoved(EntryEvent<K, V> @event)
        {
            fRemoved(@event);
        }

        public void EntryUpdated(EntryEvent<K, V> @event)
        {
            fUpdated(@event);
        }

        public void EntryEvicted(EntryEvent<K, V> @event)
        {
            fEvicted(@event);
        }
    }
}