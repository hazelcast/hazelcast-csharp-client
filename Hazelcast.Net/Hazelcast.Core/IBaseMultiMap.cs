using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>Base interface for Hazelcast distributed multi-maps.</summary>
    /// <remarks>Base interface for Hazelcast distributed multi-maps.</remarks>
    /// <seealso cref="IMultiMap{K,V}">IMultiMap&lt;K, V&gt;</seealso>
    /// <seealso cref="ITransactionalMultiMap{K,V}">ITransactionalMultiMap&lt;K, V&gt;</seealso>
    /// <?></?>
    /// <?></?>
    public interface IBaseMultiMap<K, V> : IDistributedObject
    {
        /// <summary>Stores a key-value pair in the multimap.</summary>
        /// <remarks>Stores a key-value pair in the multimap.</remarks>
        /// <param name="key">the key to be stored</param>
        /// <param name="value">the value to be stored</param>
        /// <returns>
        ///     true if size of the multimap is increased, false if the multimap
        ///     already contains the key-value pair.
        /// </returns>
        bool Put(K key, V value);

        /// <summary>Returns the collection of values associated with the key.</summary>
        /// <remarks>Returns the collection of values associated with the key.</remarks>
        /// <param name="key">the key whose associated values are to be returned</param>
        /// <returns>the collection of the values associated with the key.</returns>
        ICollection<V> Get(K key);

        /// <summary>Removes the given key value pair from the multimap.</summary>
        /// <remarks>Removes the given key value pair from the multimap.</remarks>
        /// <param name="key">the key of the entry to remove</param>
        /// <param name="value">the value of the entry to remove</param>
        /// <returns>true if the size of the multimap changed after the remove operation, false otherwise.</returns>
        bool Remove(object key, object value);

        /// <summary>Removes all the entries with the given key.</summary>
        /// <remarks>Removes all the entries with the given key.</remarks>
        /// <param name="key">the key of the entries to remove</param>
        /// <returns>
        ///     the collection of removed values associated with the given key. Returned collection
        ///     might be modifiable but it has no effect on the multimap
        /// </returns>
        ICollection<V> Remove(object key);

        /// <summary>Returns number of values matching to given key in the multimap.</summary>
        /// <remarks>Returns number of values matching to given key in the multimap.</remarks>
        /// <param name="key">the key whose values count are to be returned</param>
        /// <returns>number of values matching to given key in the multimap.</returns>
        int ValueCount(K key);

        /// <summary>Returns the number of key-value pairs in the multimap.</summary>
        /// <remarks>Returns the number of key-value pairs in the multimap.</remarks>
        /// <returns>the number of key-value pairs in the multimap.</returns>
        int Size();
    }
}