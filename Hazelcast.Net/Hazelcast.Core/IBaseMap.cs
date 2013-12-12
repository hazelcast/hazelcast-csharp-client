namespace Hazelcast.Core
{
    /// <summary>Base interface for Hazelcast distributed maps.</summary>
    /// <remarks>Base interface for Hazelcast distributed maps.</remarks>
    /// <seealso cref="IHazelcastMap{K, V}">IHazelcastMap&lt;K, V&gt;</seealso>
    /// <seealso cref="ITransactionalMap{K,V}">ITransactionalMap&lt;K, V&gt;</seealso>
    public interface IBaseMap<K, V> : IDistributedObject
    {
        /// <summary>
        ///     Returns
        ///     <code>true</code>
        ///     if this map contains an entry for the specified
        ///     key.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>
        ///     <code>true</code>
        ///     if this map contains an entry for the specified key
        /// </returns>
        bool ContainsKey(object key);

        /// <summary>
        ///     Returns the value for the specified key, or
        ///     <code>null</code>
        ///     if this map does not contain this key.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        V Get(object key);

        /// <summary>
        ///     Associates the specified value with the specified key in this map
        ///     If the map previously contained a mapping for
        ///     the key, the old value is replaced by the specified value.
        /// </summary>
        /// <remarks>
        ///     Associates the specified value with the specified key in this map
        ///     If the map previously contained a mapping for
        ///     the key, the old value is replaced by the specified value.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>
        ///     previous value associated with
        ///     <code>key</code>
        ///     or
        ///     <code>null</code>
        ///     if there was no mapping for
        ///     <code>key</code>
        ///     .
        /// </returns>
        V Put(K key, V value);

        /// <summary>
        ///     Associates the specified value with the specified key in this map
        ///     If the map previously contained a mapping for
        ///     the key, the old value is replaced by the specified value.
        /// </summary>
        /// <remarks>
        ///     Associates the specified value with the specified key in this map
        ///     If the map previously contained a mapping for
        ///     the key, the old value is replaced by the specified value.
        ///     <p /> This method is preferred to
        ///     <see cref="IBaseMap{K,V}.Put(K, V)">BaseMap&lt;K, V&gt;.Put(K, V)</see>
        ///     if the old value is not needed.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        void Set(K key, V value);

        /// <summary>
        ///     If the specified key is not already associated
        ///     with a value, associate it with the given value.
        /// </summary>
        /// <remarks>
        ///     If the specified key is not already associated
        ///     with a value, associate it with the given value.
        ///     This is equivalent to
        ///     <pre>
        ///         if (!map.containsKey(key))
        ///         return map.put(key, value);
        ///         else
        ///         return map.get(key);
        ///     </pre>
        ///     except that the action is performed atomically.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>
        ///     previous value associated with
        ///     <code>key</code>
        ///     or
        ///     <code>null</code>
        ///     if there was no mapping for
        ///     <code>key</code>
        ///     .
        /// </returns>
        V PutIfAbsent(K key, V value);

        /// <summary>Replaces the entry for a key only if currently mapped to some value.</summary>
        /// <remarks>
        ///     Replaces the entry for a key only if currently mapped to some value.
        ///     This is equivalent to
        ///     <pre>
        ///         if (map.containsKey(key)) {
        ///         return map.put(key, value);
        ///         } else return null;
        ///     </pre>
        ///     except that the action is performed atomically.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>
        ///     previous value associated with
        ///     <code>key</code>
        ///     or
        ///     <code>null</code>
        ///     if there was no mapping for
        ///     <code>key</code>
        ///     .
        /// </returns>
        V Replace(K key, V value);

        /// <summary>Replaces the entry for a key only if currently mapped to a given value.</summary>
        /// <remarks>
        ///     Replaces the entry for a key only if currently mapped to a given value.
        ///     This is equivalent to
        ///     <pre>
        ///         if (map.containsKey(key) &amp;&amp; map.get(key).equals(oldValue)) {
        ///         map.put(key, newValue);
        ///         return true;
        ///         } else return false;
        ///     </pre>
        ///     except that the action is performed atomically.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="oldValue">old value</param>
        /// <param name="newValue">new value</param>
        /// <returns>
        ///     <code>true</code>
        ///     if the value was replaced
        /// </returns>
        bool Replace(K key, V oldValue, V newValue);

        /// <summary>Removes the mapping for a key from this map if it is present.</summary>
        /// <remarks>
        ///     Removes the mapping for a key from this map if it is present.
        ///     <p>
        ///         The map will not contain a mapping for the specified key once the
        ///         call returns.
        ///     </p>
        /// </remarks>
        /// <param name="key">key</param>
        /// <returns>
        ///     previous value associated with
        ///     <code>key</code>
        ///     or
        ///     <code>null</code>
        ///     if there was no mapping for
        ///     <code>key</code>
        ///     .
        /// </returns>
        V Remove(object key);

        /// <summary>Removes the mapping for a key from this map if it is present.</summary>
        /// <remarks>
        ///     Removes the mapping for a key from this map if it is present.
        ///     <p>
        ///         The map will not contain a mapping for the specified key once the
        ///         call returns.
        ///         *
        ///     </p>
        ///     <p>
        ///         This method is preferred to
        ///         <see cref="IBaseMap{K,V}.Remove(object)">BaseMap&lt;K, V&gt;.Remove(object)</see>
        ///         if the old value is not needed.
        ///     </p>
        /// </remarks>
        /// <param name="key">key</param>
        void Delete(object key);

        /// <summary>Removes the entry for a key only if currently mapped to a given value.</summary>
        /// <remarks>
        ///     Removes the entry for a key only if currently mapped to a given value.
        ///     This is equivalent to
        ///     <pre>
        ///         if (map.containsKey(key) &amp;&amp; map.get(key).equals(value)) {
        ///         map.remove(key);
        ///         return true;
        ///         } else return false;
        ///     </pre>
        ///     except that the action is performed atomically.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>
        ///     <code>true</code>
        ///     if the value was removed
        /// </returns>
        bool Remove(object key, object value);

        /// <summary>Returns <tt>true</tt> if this map contains no entries.</summary>
        /// <remarks>Returns <tt>true</tt> if this map contains no entries.</remarks>
        /// <returns><tt>true</tt> if this map contains no entries</returns>
        bool IsEmpty();

        /// <summary>Returns the number of entries in this map.</summary>
        /// <remarks>Returns the number of entries in this map.</remarks>
        /// <returns>the number of entries in this map</returns>
        int Size();
    }
}