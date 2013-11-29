using System.Collections.Generic;
using Hazelcast.Net.Ext;

namespace Hazelcast.Core
{
    /// <summary>A specialized map whose keys can be associated with multiple values.</summary>
    /// <remarks>
    ///     A specialized map whose keys can be associated with multiple values.
    ///     <p />
    ///     <p>
    ///         <b>Gotchas:</b>
    ///         <ul>
    ///             <li>
    ///                 Methods, including but not limited to <tt>get</tt>, <tt>containsKey</tt>,
    ///                 <tt>containsValue</tt>, <tt>remove</tt>, <tt>put</tt>,
    ///                 <tt>lock</tt>, <tt>unlock</tt>, do not use <tt>hashCode</tt> and <tt>equals</tt>
    ///                 implementations of keys,
    ///                 instead they use <tt>hashCode</tt> and <tt>equals</tt> of binary (serialized) forms of the objects.
    ///             </li>
    ///             <li>
    ///                 Methods, including but not limited to <tt>get</tt>, <tt>remove</tt>,
    ///                 <tt>keySet</tt>, <tt>values</tt>, <tt>entrySet</tt>,
    ///                 return a collection clone of the values. The collection is <b>NOT</b> backed by the map,
    ///                 so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
    ///             </li>
    ///         </ul>
    ///     </p>
    /// </remarks>
    /// <seealso cref="IMap{K, V}">IMap&lt;K, V&gt;</seealso>
    public interface IMultiMap<K, V> : IBaseMultiMap<K, V>, IDistributedObject
    {
        //string GetName();
        /// <summary>Returns the name of this multimap.</summary>
        /// <remarks>Returns the name of this multimap.</remarks>
        /// <returns>the name of this multimap</returns>
        /// <summary>Stores a key-value pair in the multimap.</summary>
        /// <remarks>
        ///     Stores a key-value pair in the multimap.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///         the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///         defined in <tt>key</tt>'s class.
        ///     </p>
        /// </remarks>
        /// <param name="key">the key to be stored</param>
        /// <param name="value">the value to be stored</param>
        /// <returns>
        ///     true if size of the multimap is increased, false if the multimap
        ///     already contains the key-value pair.
        /// </returns>
        bool Put(K key, V value);

        /// <summary>Returns the collection of values associated with the key.</summary>
        /// <remarks>
        ///     Returns the collection of values associated with the key.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///         the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///         defined in <tt>key</tt>'s class.
        ///     </p>
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     The collection is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </remarks>
        /// <param name="key">the key whose associated values are to be returned</param>
        /// <returns>the collection of the values associated with the key.</returns>
        ICollection<V> Get(K key);

        /// <summary>Removes the given key value pair from the multimap.</summary>
        /// <remarks>
        ///     Removes the given key value pair from the multimap.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///     the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///     defined in <tt>key</tt>'s class.
        /// </remarks>
        /// <param name="key">the key of the entry to remove</param>
        /// <param name="value">the value of the entry to remove</param>
        /// <returns>true if the size of the multimap changed after the remove operation, false otherwise.</returns>
        bool Remove(object key, object value);

        /// <summary>Removes all the entries with the given key.</summary>
        /// <remarks>
        ///     Removes all the entries with the given key.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///         the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///         defined in <tt>key</tt>'s class.
        ///     </p>
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     The collection is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </remarks>
        /// <param name="key">the key of the entries to remove</param>
        /// <returns>
        ///     the collection of removed values associated with the given key. Returned collection
        ///     might be modifiable but it has no effect on the multimap
        /// </returns>
        ICollection<V> Remove(object key);

        /// <summary>Returns the locally owned set of keys.</summary>
        /// <remarks>
        ///     Returns the locally owned set of keys.
        ///     <p />
        ///     Each key in this map is owned and managed by a specific
        ///     member in the cluster.
        ///     <p />
        ///     Note that ownership of these keys might change over time
        ///     so that key ownerships can be almost evenly distributed
        ///     in the cluster.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <returns>locally owned keys.</returns>
        ICollection<K> LocalKeySet();

        /// <summary>Returns the set of keys in the multimap.</summary>
        /// <remarks>
        ///     Returns the set of keys in the multimap.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <returns>
        ///     the set of keys in the multimap. Returned set might be modifiable
        ///     but it has no effect on the multimap
        /// </returns>
        ICollection<K> KeySet();

        /// <summary>Returns the collection of values in the multimap.</summary>
        /// <remarks>
        ///     Returns the collection of values in the multimap.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     The collection is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </remarks>
        /// <returns>
        ///     the collection of values in the multimap. Returned collection might be modifiable
        ///     but it has no effect on the multimap
        /// </returns>
        ICollection<V> Values();

        /// <summary>Returns the set of key-value pairs in the multimap.</summary>
        /// <remarks>
        ///     Returns the set of key-value pairs in the multimap.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <returns>
        ///     the set of key-value pairs in the multimap. Returned set might be modifiable
        ///     but it has no effect on the multimap
        /// </returns>
        ICollection<KeyValuePair<K, V>> EntrySet();

        /// <summary>Returns whether the multimap contains an entry with the key.</summary>
        /// <remarks>
        ///     Returns whether the multimap contains an entry with the key.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///         the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///         defined in <tt>key</tt>'s class.
        ///     </p>
        /// </remarks>
        /// <param name="key">the key whose existence is checked.</param>
        /// <returns>true if the multimap contains an entry with the key, false otherwise.</returns>
        bool ContainsKey(K key);

        /// <summary>Returns whether the multimap contains an entry with the value.</summary>
        /// <remarks>
        ///     Returns whether the multimap contains an entry with the value.
        ///     <p />
        /// </remarks>
        /// <param name="value">the value whose existence is checked.</param>
        /// <returns>true if the multimap contains an entry with the value, false otherwise.</returns>
        bool ContainsValue(object value);

        /// <summary>Returns whether the multimap contains the given key-value pair.</summary>
        /// <remarks>
        ///     Returns whether the multimap contains the given key-value pair.
        ///     <p />
        ///     This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///     the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///     defined in <tt>key</tt>'s class.
        /// </remarks>
        /// <param name="key">the key whose existence is checked.</param>
        /// <param name="value">the value whose existence is checked.</param>
        /// <returns>true if the multimap contains the key-value pair, false otherwise.</returns>
        bool ContainsEntry(K key, V value);

        /// <summary>Returns the number of key-value pairs in the multimap.</summary>
        /// <remarks>Returns the number of key-value pairs in the multimap.</remarks>
        /// <returns>the number of key-value pairs in the multimap.</returns>
        int Size();

        /// <summary>Clears the multimap.</summary>
        /// <remarks>Clears the multimap. Removes all key-value pairs.</remarks>
        void Clear();

        /// <summary>Returns number of values matching to given key in the multimap.</summary>
        /// <remarks>
        ///     Returns number of values matching to given key in the multimap.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///         the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///         defined in <tt>key</tt>'s class.
        ///     </p>
        /// </remarks>
        /// <param name="key">the key whose values count are to be returned</param>
        /// <returns>number of values matching to given key in the multimap.</returns>
        int ValueCount(K key);

        /// <summary>Adds a local entry listener for this multimap.</summary>
        /// <remarks>
        ///     Adds a local entry listener for this multimap. Added listener will be only
        ///     listening for the events (add/remove/update) of the locally owned entries.
        ///     <p />
        ///     Note that entries in distributed multimap are partitioned across
        ///     the cluster members; each member owns and manages the some portion of the
        ///     entries. Owned entries are called local entries. This
        ///     listener will be listening for the events of local entries. Let's say
        ///     your cluster has member1 and member2. On member2 you added a local listener and from
        ///     member1, you call <code>multimap.put(key2, value2)</code>.
        ///     If the key2 is owned by member2 then the local listener will be
        ///     notified for the add/update event. Also note that entries can migrate to
        ///     other nodes for load balancing and/or membership change.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <seealso cref="IMultiMap{K,V}.LocalKeySet()">IMultiMap&lt;K, V&gt;.LocalKeySet()</seealso>
        /// <returns>returns registration id.</returns>
        string AddLocalEntryListener(IEntryListener<K, V> listener);

        /// <summary>Adds an entry listener for this multimap.</summary>
        /// <remarks>
        ///     Adds an entry listener for this multimap. Listener will get notified
        ///     for all multimap add/remove/update/evict events.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <param name="includeValue">
        ///     <tt>true</tt> if <tt>EntryEvent</tt> should
        ///     contain the value.
        /// </param>
        /// <returns>returns registration id.</returns>
        string AddEntryListener(IEntryListener<K, V> listener, bool includeValue);

        /// <summary>
        ///     Removes the specified entry listener
        ///     Returns silently if there is no such listener added before.
        /// </summary>
        /// <remarks>
        ///     Removes the specified entry listener
        ///     Returns silently if there is no such listener added before.
        /// </remarks>
        /// <param name="registrationId">Id of listener registration</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveEntryListener(string registrationId);

        /// <summary>Adds the specified entry listener for the specified key.</summary>
        /// <remarks>
        ///     Adds the specified entry listener for the specified key.
        ///     The listener will get notified for all
        ///     add/remove/update/evict events of the specified key only.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///         the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///         defined in <tt>key</tt>'s class.
        ///     </p>
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <param name="key">the key to listen</param>
        /// <param name="includeValue">
        ///     <tt>true</tt> if <tt>EntryEvent</tt> should
        ///     contain the value.
        /// </param>
        /// <returns>returns registration id.</returns>
        string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue);

        /// <summary>Acquires the lock for the specified key.</summary>
        /// <remarks>
        ///     Acquires the lock for the specified key.
        ///     <p>
        ///         If the lock is not available then
        ///         the current thread becomes disabled for thread scheduling
        ///         purposes and lies dormant until the lock has been acquired.
        ///         <p />
        ///         Scope of the lock is this multimap only.
        ///         Acquired lock is only for the key in this multimap.
        ///         <p />
        ///         Locks are re-entrant so if the key is locked N times then
        ///         it should be unlocked N times before another thread can acquire it.
        ///         <p />
        ///         <p>
        ///             <b>Warning:</b>
        ///         </p>
        ///         <p>
        ///             This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///             the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///             defined in <tt>key</tt>'s class.
        ///         </p>
        /// </remarks>
        /// <param name="key">key to lock.</param>
        void Lock(K key);

        /// <summary>Acquires the lock for the specified key for the specified lease time.</summary>
        /// <remarks>
        ///     Acquires the lock for the specified key for the specified lease time.
        ///     <p>
        ///         After lease time, lock will be released..
        ///         <p />
        ///         <p>
        ///             If the lock is not available then
        ///             the current thread becomes disabled for thread scheduling
        ///             purposes and lies dormant until the lock has been acquired.
        ///             <p />
        ///             Scope of the lock is this map only.
        ///             Acquired lock is only for the key in this map.
        ///             <p />
        ///             Locks are re-entrant so if the key is locked N times then
        ///             it should be unlocked N times before another thread can acquire it.
        ///             <p />
        ///             <p>
        ///                 <b>Warning:</b>
        ///             </p>
        ///             This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///             the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///             defined in <tt>key</tt>'s class.
        /// </remarks>
        /// <param name="key">key to lock.</param>
        /// <param name="leaseTime">time to wait before releasing the lock.</param>
        /// <param name="timeUnit">unit of time to specify lease time.</param>
        void Lock(K key, long leaseTime, TimeUnit timeUnit);

        /// <summary>Checks the lock for the specified key.</summary>
        /// <remarks>
        ///     Checks the lock for the specified key.
        ///     <p>
        ///         If the lock is acquired then returns true, else false.
        ///         <p />
        ///         <p>
        ///             <b>Warning:</b>
        ///         </p>
        ///         This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///         the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///         defined in <tt>key</tt>'s class.
        /// </remarks>
        /// <param name="key">key to lock to be checked.</param>
        /// <returns><tt>true</tt> if lock is acquired, <tt>false</tt> otherwise.</returns>
        bool IsLocked(K key);

        /// <summary>Tries to acquire the lock for the specified key.</summary>
        /// <remarks>
        ///     Tries to acquire the lock for the specified key.
        ///     <p>
        ///         If the lock is not available then the current thread
        ///         doesn't wait and returns false immediately.
        ///         <p />
        ///         <p>
        ///             <b>Warning:</b>
        ///         </p>
        ///         <p>
        ///             This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///             the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///             defined in <tt>key</tt>'s class.
        ///         </p>
        /// </remarks>
        /// <param name="key">key to lock.</param>
        /// <returns><tt>true</tt> if lock is acquired, <tt>false</tt> otherwise.</returns>
        bool TryLock(K key);

        /// <summary>Tries to acquire the lock for the specified key.</summary>
        /// <remarks>
        ///     Tries to acquire the lock for the specified key.
        ///     <p>
        ///         If the lock is not available then
        ///         the current thread becomes disabled for thread scheduling
        ///         purposes and lies dormant until one of two things happens:
        ///         <ul>
        ///             <li>
        ///                 The lock is acquired by the current thread; or
        ///                 <li>The specified waiting time elapses
        ///         </ul>
        ///         <p />
        ///         <p>
        ///             <b>Warning:</b>
        ///         </p>
        ///         <p>
        ///             This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///             the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///             defined in <tt>key</tt>'s class.
        ///         </p>
        /// </remarks>
        /// <param name="time">the maximum time to wait for the lock</param>
        /// <param name="timeunit">the time unit of the <tt>time</tt> argument.</param>
        /// <returns>
        ///     <tt>true</tt> if the lock was acquired and <tt>false</tt>
        ///     if the waiting time elapsed before the lock was acquired.
        /// </returns>
        /// <exception cref="System.Exception"></exception>
        bool TryLock(K key, long time, TimeUnit timeunit);

        /// <summary>Releases the lock for the specified key.</summary>
        /// <remarks>
        ///     Releases the lock for the specified key. It never blocks and
        ///     returns immediately.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///         the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///         defined in <tt>key</tt>'s class.
        ///     </p>
        /// </remarks>
        /// <param name="key">key to lock.</param>
        void Unlock(K key);

        /// <summary>Releases the lock for the specified key regardless of the lock owner.</summary>
        /// <remarks>
        ///     Releases the lock for the specified key regardless of the lock owner.
        ///     It always successfully unlocks the key, never blocks
        ///     and returns immediately.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <tt>hashCode</tt> and <tt>equals</tt> of binary form of
        ///     the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///     defined in <tt>key</tt>'s class.
        /// </remarks>
        /// <param name="key">key to lock.</param>
        void ForceUnlock(K key);

        //    /**
        //     * Returns LocalMultiMapStats for this map.
        //     * LocalMultiMapStats is the statistics for the local portion of this
        //     * distributed multi map and contains information such as ownedEntryCount
        //     * backupEntryCount, lastUpdateTime, lockedEntryCount.
        //     * <p/>
        //     * Since this stats are only for the local portion of this multi map, if you
        //     * need the cluster-wide MultiMapStats then you need to get the LocalMapStats
        //     * from all members of the cluster and combine them.
        //     *
        //     * @return this multimap's local statistics.
        //     */
        //    LocalMultiMapStats getLocalMultiMapStats();
    }
}