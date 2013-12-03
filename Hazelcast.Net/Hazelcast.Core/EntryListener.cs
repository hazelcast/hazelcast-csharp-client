using System;

namespace Hazelcast.Core
{
	
	public interface EntryListener<K, V>: EventListener {

    /**
     * Invoked when an entry is added.
     *
     * @param event entry event
     */
    void entryAdded(EntryEvent<K, V> e);

    /**
     * Invoked when an entry is removed.
     *
     * @param event entry event
     */
    void entryRemoved(EntryEvent<K, V> e);

    /**
     * Invoked when an entry is updated.
     *
     * @param event entry event
     */
    void entryUpdated(EntryEvent<K, V> e);

    /**
     * Invoked when an entry is evicted.
     *
     * @param event entry event
     */
    void entryEvicted(EntryEvent<K, V> e);
	}
}

