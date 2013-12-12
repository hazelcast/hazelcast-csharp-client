using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Map;
using Hazelcast.Net.Ext;

namespace Hazelcast.Core
{
    /// <summary>Concurrent, distributed, observable and queryable map.</summary>
    /// <remarks>
    ///     Concurrent, distributed, observable and queryable map.
    ///     <p />
    ///     <p>
    ///         <b>
    ///             This class is <i>not</i> a general-purpose <tt>ConcurrentMap</tt> implementation! While this class
    ///             implements
    ///             the <tt>Map</tt> interface, it intentionally violates <tt>Map's</tt> general contract, which mandates the
    ///             use of the <tt>Equals</tt> method when comparing objects. Instead of the <tt>Equals</tt> method this
    ///             implementation
    ///             compares the serialized byte version of the objects.
    ///         </b>
    ///     </p>
    ///     <p />
    ///     <p>
    ///         <b>Gotchas:</b>
    ///         <ul>
    ///             <li>
    ///                 Methods, including but not limited to <tt>Get</tt>, <tt>ContainsKey</tt>,
    ///                 <tt>ContainsValue</tt>, <tt>Evict</tt>, <tt>Remove</tt>, <tt>Put</tt>,
    ///                 <tt>PutIfAbsent</tt>, <tt>Replace</tt>, <tt>Lock</tt>,
    ///                 <tt>Unlock</tt>, do not use <tt>GetHashCode</tt> and <tt>Equals</tt> implementations of keys,
    ///                 instead they use <tt>GetHashCode</tt> and <tt>Equals</tt> of binary (serialized) forms of the objects.
    ///             </li>
    ///             <li>
    ///                 <tt>get</tt> method returns a clone of original values, modifying the returned value does not
    ///                 change
    ///                 the actual value in the map. One should put modified value back to make changes visible to all
    ///                 nodes.
    ///                 For additional info see
    ///                 <see cref="IHazelcastMap{K, V}.Get(object)">IHazelcastMap&lt;K, V&gt;.Get(object)</see>
    ///             </li>
    ///             <li>
    ///                 Methods, including but not limited to <tt>keySet</tt>, <tt>values</tt>, <tt>entrySet</tt>,
    ///                 return a collection clone of the values. The collection is <b>NOT</b> backed by the map,
    ///                 so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
    ///             </li>
    ///         </ul>
    ///     </p>
    /// </remarks>
    public interface IHazelcastMap<K, V> : IBaseMap<K, V>
    {
        //IBaseMap
        //bool ContainsKey(object key);
        //V Get(object key);
        //V Put(K key, V value);
        //V Remove(object key);
        //bool Remove(object key, object value);
        //void Delete(object key);
        //V PutIfAbsent(K key, V value);
        //bool Replace(K key, V oldValue, V newValue);
        //V Replace(K key, V value);
        //void Set(K key, V value);
        //bool IsEmpty();

        bool ContainsValue(object value);
        void Flush();
        Task<V> GetAsync(K key);
        Task<V> PutAsync(K key, V value);
        Task<V> PutAsync(K key, V value, long ttl, TimeUnit timeunit);
        Task<V> RemoveAsync(K key);
        bool TryRemove(K key, long timeout, TimeUnit timeunit);
        bool TryPut(K key, V value, long timeout, TimeUnit timeunit);
        V Put(K key, V value, long ttl, TimeUnit timeunit);
        void PutTransient(K key, V value, long ttl, TimeUnit timeunit);

        V PutIfAbsent(K key, V value, long ttl, TimeUnit timeunit);
        void Set(K key, V value, long ttl, TimeUnit timeunit);
        void Lock(K key);
        void Lock(K key, long leaseTime, TimeUnit timeUnit);
        bool IsLocked(K key);
        bool TryLock(K key);

        /// <exception cref="System.Exception"></exception>
        bool TryLock(K key, long time, TimeUnit timeunit);

        void Unlock(K key);
        void ForceUnlock(K key);
        string AddLocalEntryListener(IEntryListener<K, V> listener);
        string AddInterceptor(MapInterceptor interceptor);
        void RemoveInterceptor(string id);
        string AddEntryListener(IEntryListener<K, V> listener, bool includeValue);
        bool RemoveEntryListener(string id);
        string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue);
        string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, K key, bool includeValue);
        string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, bool includeValue);
        IEntryView<K, V> GetEntryView(K key);
        bool Evict(K key);
        ICollection<K> Keys();
        IDictionary<K, V> GetAll(ICollection<K> keys);
        ICollection<V> Values();
        ICollection<KeyValuePair<K, V>> EntrySet();
        ICollection<K> KeySet(IPredicate<K, V> predicate);
        ICollection<KeyValuePair<K, V>> EntrySet(IPredicate<K, V> predicate);
        ICollection<V> Values(IPredicate<K, V> predicate);
        //ICollection<K> LocalKeySet();
        //ICollection<K> LocalKeySet(IPredicate<K, V> predicate);
        void AddIndex(string attribute, bool ordered);
        //object ExecuteOnKey(K key, EntryProcessor<K, V> entryProcessor);
        //IDictionary<K, object> ExecuteOnEntries(EntryProcessor<K, V> entryProcessor);
        //IDictionary<K, object> ExecuteOnEntries(EntryProcessor<K, V> entryProcessor, IPredicate<K, V> predicate);
        void PutAll<K>(IDictionary<K, V> m);
        void Clear();
    }


    //internal interface IMap<K, V> : IBaseMap<K, V>
    //{
    //    /// <summary>Returns a set clone of the keys contained in this map.</summary>
    //    /// <remarks>
    //    ///     Returns a set clone of the keys contained in this map.
    //    ///     The set is <b>NOT</b> backed by the map,
    //    ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
    //    /// </remarks>
    //    /// <returns>a set clone of the keys contained in this map</returns>
    //    ICollection<K> Keys { get; }

    //    /// <summary>
    //    ///     <inheritDoc></inheritDoc>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         Ë†
    //    ///         This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///         the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///         defined in <tt>key</tt>'s class.
    //    ///     </p>
    //    /// </summary>
    //    bool ContainsKey(object key);

    //    /// <summary>
    //    ///     <inheritDoc></inheritDoc>
    //    /// </summary>
    //    bool ContainsValue(object value);

    //    /// <summary>
    //    ///     <inheritDoc></inheritDoc>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method returns a clone of original value, modifying the returned value does not change
    //    ///         the actual value in the map. One should put modified value back to make changes visible to all nodes.
    //    ///         <pre>
    //    ///             V value = map.get(key);
    //    ///             value.updateSomeProperty();
    //    ///             map.put(key, value);
    //    ///         </pre>
    //    ///     </p>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    ///     <p />
    //    /// </summary>
    //    V Get(object key);

    //    /// <summary>
    //    ///     <inheritDoc></inheritDoc>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method returns a clone of previous value, not the original (identically equal) value
    //    ///         previously put into map.
    //    ///     </p>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </summary>
    //    V Put(K key, V value);

    //    /// <summary>
    //    ///     <inheritDoc></inheritDoc>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///         the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///         defined in <tt>key</tt>'s class.
    //    ///     </p>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method returns a clone of previous value, not the original (identically equal) value
    //    ///         previously put into map.
    //    ///     </p>
    //    /// </summary>
    //    V Remove(object key);

    //    /// <summary>
    //    ///     <inheritDoc></inheritDoc>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </summary>
    //    bool Remove(object key, object value);

    //    /// <summary>
    //    ///     Removes the mapping for a key from this map if it is present
    //    ///     (optional operation).
    //    /// </summary>
    //    /// <remarks>
    //    ///     Removes the mapping for a key from this map if it is present
    //    ///     (optional operation).
    //    ///     <p>
    //    ///         Differently from
    //    ///         <see cref="Sharpen.Collections.Remove(object)">Sharpen.Collections.Remove(object)</see>
    //    ///         ; this operation does not return
    //    ///         removed value to avoid serialization cost of returned value.
    //    ///         If the removed value will not be used, delete operation
    //    ///         should be preferred over remove operation for a better performance.
    //    ///         <p>
    //    ///             The map will not contain a mapping for the specified key once the
    //    ///             call returns.
    //    /// </remarks>
    //    /// <param name="key">key whose mapping is to be removed from the map</param>
    //    /// <exception cref="System.InvalidCastException">
    //    ///     if the key is of an inappropriate type for
    //    ///     this map (optional)
    //    /// </exception>
    //    /// <exception cref="System.ArgumentNullException">
    //    ///     if the specified key is null and this
    //    ///     map does not permit null keys (optional)
    //    /// </exception>
    //    void Delete(object key);

    //    /// <summary>
    //    ///     If this map has a MapStore this method flushes
    //    ///     all the local dirty entries by calling MapStore.storeAll() and/or MapStore.deleteAll()
    //    /// </summary>
    //    void Flush();

    //    /// <summary>Returns the entries for the given keys.</summary>
    //    /// <remarks>
    //    ///     Returns the entries for the given keys.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     The returned map is <b>NOT</b> backed by the original map,
    //    ///     so changes to the original map are <b>NOT</b> reflected in the returned map, and vice-versa.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>keys</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="keys">keys to get</param>
    //    /// <returns>map of entries</returns>
    //    IDictionary<K, V> GetAll(ICollection<K> keys);

    //    /// <summary>Asynchronously gets the given key.</summary>
    //    /// <remarks>
    //    ///     Asynchronously gets the given key.
    //    ///     <code>
    //    /// Future future = map.getAsync(key);
    //    /// // do some other stuff, when ready get the result
    //    /// Object value = future.get();
    //    /// </code>
    //    ///     Future.get() will block until the actual map.get() completes.
    //    ///     If the application requires timely response,
    //    ///     then Future.get(timeout, timeunit) can be used.
    //    ///     <code>
    //    /// try{
    //    /// Future future = map.getAsync(key);
    //    /// Object value = future.get(40, TimeUnit.MILLISECOND);
    //    /// }catch (TimeoutException t) {
    //    /// // time wasn't enough
    //    /// }
    //    /// </code>
    //    ///     ExecutionException is never thrown.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">the key of the map entry</param>
    //    /// <returns>Future from which the value of the key can be retrieved.</returns>
    //    /// <seealso cref="Hazelcast.Net.Ext.Future{V}">Hazelcast.Net.Ext.Future&lt;V&gt;</seealso>
    //    Task<V> GetAsync(K key);

    //    /// <summary>Asynchronously puts the given key and value.</summary>
    //    /// <remarks>
    //    ///     Asynchronously puts the given key and value.
    //    ///     <code>
    //    /// Future future = map.putAsync(key, value);
    //    /// // do some other stuff, when ready get the result
    //    /// Object oldValue = future.get();
    //    /// </code>
    //    ///     Future.get() will block until the actual map.get() completes.
    //    ///     If the application requires timely response,
    //    ///     then Future.get(timeout, timeunit) can be used.
    //    ///     <code>
    //    /// try{
    //    /// Future future = map.putAsync(key, newValue);
    //    /// Object oldValue = future.get(40, TimeUnit.MILLISECOND);
    //    /// }catch (TimeoutException t) {
    //    /// // time wasn't enough
    //    /// }
    //    /// </code>
    //    ///     ExecutionException is never thrown.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">the key of the map entry</param>
    //    /// <param name="value">the new value of the map entry</param>
    //    /// <returns>Future from which the old value of the key can be retrieved.</returns>
    //    /// <seealso cref="Hazelcast.Net.Ext.Future{V}">Hazelcast.Net.Ext.Future&lt;V&gt;</seealso>
    //    Task<V> PutAsync(K key, V value);

    //    /// <summary>Asynchronously puts the given key and value into this map with a given ttl (time to live) value.</summary>
    //    /// <remarks>
    //    ///     Asynchronously puts the given key and value into this map with a given ttl (time to live) value.
    //    ///     Entry will expire and get evicted after the ttl. If ttl is 0, then
    //    ///     the entry lives forever.
    //    ///     <code>
    //    /// Future future = map.putAsync(key, value, ttl, timeunit);
    //    /// // do some other stuff, when ready get the result
    //    /// Object oldValue = future.get();
    //    /// </code>
    //    ///     Future.get() will block until the actual map.get() completes.
    //    ///     If the application requires timely response,
    //    ///     then Future.get(timeout, timeunit) can be used.
    //    ///     <code>
    //    /// try{
    //    /// Future future = map.putAsync(key, newValue, ttl, timeunit);
    //    /// Object oldValue = future.get(40, TimeUnit.MILLISECOND);
    //    /// }catch (TimeoutException t) {
    //    /// // time wasn't enough
    //    /// }
    //    /// </code>
    //    ///     ExecutionException is never thrown.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">the key of the map entry</param>
    //    /// <param name="value">the new value of the map entry</param>
    //    /// <param name="ttl">
    //    ///     maximum time for this entry to stay in the map
    //    ///     0 means infinite.
    //    /// </param>
    //    /// <param name="timeunit">time unit for the ttl</param>
    //    /// <returns>Future from which the old value of the key can be retrieved.</returns>
    //    /// <seealso cref="Hazelcast.Net.Ext.Future{V}">Hazelcast.Net.Ext.Future&lt;V&gt;</seealso>
    //    Task<V> PutAsync(K key, V value, long ttl, TimeUnit timeunit);

    //    /// <summary>Asynchronously removes the given key.</summary>
    //    /// <remarks>
    //    ///     Asynchronously removes the given key.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">The key of the map entry to remove.</param>
    //    /// <returns>
    //    ///     A
    //    ///     <see cref="Hazelcast.Net.Ext.Future{V}">Hazelcast.Net.Ext.Future&lt;V&gt;</see>
    //    ///     from which the value
    //    ///     removed from the map can be retrieved.
    //    /// </returns>
    //    Task<V> RemoveAsync(K key);

    //    /// <summary>
    //    ///     Tries to remove the entry with the given key from this map
    //    ///     within specified timeout value.
    //    /// </summary>
    //    /// <remarks>
    //    ///     Tries to remove the entry with the given key from this map
    //    ///     within specified timeout value. If the key is already locked by another
    //    ///     thread and/or member, then this operation will wait timeout
    //    ///     amount for acquiring the lock.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method returns a clone of previous value, not the original (identically equal) value
    //    ///         previously put into map.
    //    ///     </p>
    //    /// </remarks>
    //    /// <param name="key">key of the entry</param>
    //    /// <param name="timeout">
    //    ///     maximum time to wait for acquiring the lock
    //    ///     for the key
    //    /// </param>
    //    /// <param name="timeunit">time unit for the timeout</param>
    //    /// <returns>
    //    ///     <tt>true</tt> if the remove is successful, <tt>false</tt>
    //    ///     otherwise.
    //    /// </returns>
    //    bool TryRemove(K key, long timeout, TimeUnit timeunit);

    //    /// <summary>
    //    ///     Tries to put the given key, value into this map within specified
    //    ///     timeout value.
    //    /// </summary>
    //    /// <remarks>
    //    ///     Tries to put the given key, value into this map within specified
    //    ///     timeout value. If this method returns false, it means that
    //    ///     the caller thread couldn't acquire the lock for the key within
    //    ///     timeout duration, thus put operation is not successful.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key of the entry</param>
    //    /// <param name="value">value of the entry</param>
    //    /// <param name="timeout">maximum time to wait</param>
    //    /// <param name="timeunit">time unit for the timeout</param>
    //    /// <returns>
    //    ///     <tt>true</tt> if the put is successful, <tt>false</tt>
    //    ///     otherwise.
    //    /// </returns>
    //    bool TryPut(K key, V value, long timeout, TimeUnit timeunit);

    //    /// <summary>Puts an entry into this map with a given ttl (time to live) value.</summary>
    //    /// <remarks>
    //    ///     Puts an entry into this map with a given ttl (time to live) value.
    //    ///     Entry will expire and get evicted after the ttl. If ttl is 0, then
    //    ///     the entry lives forever.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method returns a clone of previous value, not the original (identically equal) value
    //    ///         previously put into map.
    //    ///     </p>
    //    /// </remarks>
    //    /// <param name="key">key of the entry</param>
    //    /// <param name="value">value of the entry</param>
    //    /// <param name="ttl">
    //    ///     maximum time for this entry to stay in the map
    //    ///     0 means infinite.
    //    /// </param>
    //    /// <param name="timeunit">time unit for the ttl</param>
    //    /// <returns>old value of the entry</returns>
    //    V Put(K key, V value, long ttl, TimeUnit timeunit);

    //    /// <summary>
    //    ///     Same as
    //    ///     <see cref="IHazelcastMap{K, V}.Put(object, object, long, TimeUnit)">
    //    ///         IMap&lt;K, V&gt;.Put(object, object, long,
    //    ///         Hazelcast.Net.Ext.TimeUnit)
    //    ///     </see>
    //    ///     but MapStore, if defined,
    //    ///     will not be called to store/persist the entry.  If ttl is 0, then
    //    ///     the entry lives forever.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </summary>
    //    /// <param name="key">key of the entry</param>
    //    /// <param name="value">value of the entry</param>
    //    /// <param name="ttl">
    //    ///     maximum time for this entry to stay in the map.
    //    ///     0 means infinite.
    //    /// </param>
    //    /// <param name="timeunit">time unit for the ttl</param>
    //    void PutTransient(K key, V value, long ttl, TimeUnit timeunit);

    //    /// <summary>
    //    ///     <inheritDoc></inheritDoc>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method returns a clone of previous value, not the original (identically equal) value
    //    ///         previously put into map.
    //    ///     </p>
    //    /// </summary>
    //    V PutIfAbsent(K key, V value);

    //    /// <summary>
    //    ///     Puts an entry into this map with a given ttl (time to live) value
    //    ///     if the specified key is not already associated with a value.
    //    /// </summary>
    //    /// <remarks>
    //    ///     Puts an entry into this map with a given ttl (time to live) value
    //    ///     if the specified key is not already associated with a value.
    //    ///     Entry will expire and get evicted after the ttl.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method returns a clone of previous value, not the original (identically equal) value
    //    ///         previously put into map.
    //    ///     </p>
    //    /// </remarks>
    //    /// <param name="key">key of the entry</param>
    //    /// <param name="value">value of the entry</param>
    //    /// <param name="ttl">maximum time for this entry to stay in the map</param>
    //    /// <param name="timeunit">time unit for the ttl</param>
    //    /// <returns>old value of the entry</returns>
    //    V PutIfAbsent(K key, V value, long ttl, TimeUnit timeunit);

    //    /// <summary>
    //    ///     <inheritDoc></inheritDoc>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </summary>
    //    bool Replace(K key, V oldValue, V newValue);

    //    /// <summary>
    //    ///     <inheritDoc></inheritDoc>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method returns a clone of previous value, not the original (identically equal) value
    //    ///         previously put into map.
    //    ///     </p>
    //    /// </summary>
    //    V Replace(K key, V value);

    //    /// <summary>Puts an entry into this map with a given ttl (time to live) value.</summary>
    //    /// <remarks>
    //    ///     Puts an entry into this map with a given ttl (time to live) value.
    //    ///     Similar to put operation except that set
    //    ///     doesn't return the old value which is more efficient.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key of the entry</param>
    //    /// <param name="value">value of the entry</param>
    //    void Set(K key, V value);

    //    /// <summary>Puts an entry into this map with a given ttl (time to live) value.</summary>
    //    /// <remarks>
    //    ///     Puts an entry into this map with a given ttl (time to live) value.
    //    ///     Entry will expire and get evicted after the ttl. If ttl is 0, then
    //    ///     the entry lives forever. Similar to put operation except that set
    //    ///     doesn't return the old value which is more efficient.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key of the entry</param>
    //    /// <param name="value">value of the entry</param>
    //    /// <param name="ttl">
    //    ///     maximum time for this entry to stay in the map
    //    ///     0 means infinite.
    //    /// </param>
    //    /// <param name="timeunit">time unit for the ttl</param>
    //    /// <returns>old value of the entry</returns>
    //    void Set(K key, V value, long ttl, TimeUnit timeunit);

    //    /// <summary>Acquires the lock for the specified key.</summary>
    //    /// <remarks>
    //    ///     Acquires the lock for the specified key.
    //    ///     <p>
    //    ///         If the lock is not available then
    //    ///         the current thread becomes disabled for thread scheduling
    //    ///         purposes and lies dormant until the lock has been acquired.
    //    ///         <p />
    //    ///         Scope of the lock is this map only.
    //    ///         Acquired lock is only for the key in this map.
    //    ///         <p />
    //    ///         Locks are re-entrant so if the key is locked N times then
    //    ///         it should be unlocked N times before another thread can acquire it.
    //    ///         <p />
    //    ///         <p>
    //    ///             <b>Warning:</b>
    //    ///         </p>
    //    ///         This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///         the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///         defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key to lock.</param>
    //    void Lock(K key);

    //    /// <summary>Acquires the lock for the specified key for the specified lease time.</summary>
    //    /// <remarks>
    //    ///     Acquires the lock for the specified key for the specified lease time.
    //    ///     <p>
    //    ///         After lease time, lock will be released..
    //    ///         <p />
    //    ///         <p>
    //    ///             If the lock is not available then
    //    ///             the current thread becomes disabled for thread scheduling
    //    ///             purposes and lies dormant until the lock has been acquired.
    //    ///             <p />
    //    ///             Scope of the lock is this map only.
    //    ///             Acquired lock is only for the key in this map.
    //    ///             <p />
    //    ///             Locks are re-entrant so if the key is locked N times then
    //    ///             it should be unlocked N times before another thread can acquire it.
    //    ///             <p />
    //    ///             <p>
    //    ///                 <b>Warning:</b>
    //    ///             </p>
    //    ///             This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///             the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///             defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key to lock.</param>
    //    /// <param name="leaseTime">time to wait before releasing the lock.</param>
    //    /// <param name="timeUnit">unit of time to specify lease time.</param>
    //    void Lock(K key, long leaseTime, TimeUnit timeUnit);

    //    /// <summary>Checks the lock for the specified key.</summary>
    //    /// <remarks>
    //    ///     Checks the lock for the specified key.
    //    ///     <p>
    //    ///         If the lock is acquired then returns true, else false.
    //    ///         <p />
    //    ///         <p>
    //    ///             <b>Warning:</b>
    //    ///         </p>
    //    ///         This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///         the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///         defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key to lock to be checked.</param>
    //    /// <returns><tt>true</tt> if lock is acquired, <tt>false</tt> otherwise.</returns>
    //    bool IsLocked(K key);

    //    /// <summary>Tries to acquire the lock for the specified key.</summary>
    //    /// <remarks>
    //    ///     Tries to acquire the lock for the specified key.
    //    ///     <p>
    //    ///         If the lock is not available then the current thread
    //    ///         doesn't wait and returns false immediately.
    //    ///         <p />
    //    ///         <p>
    //    ///             <b>Warning:</b>
    //    ///         </p>
    //    ///         This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///         the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///         defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key to lock.</param>
    //    /// <returns><tt>true</tt> if lock is acquired, <tt>false</tt> otherwise.</returns>
    //    bool TryLock(K key);

    //    /// <summary>Tries to acquire the lock for the specified key.</summary>
    //    /// <remarks>
    //    ///     Tries to acquire the lock for the specified key.
    //    ///     <p>
    //    ///         If the lock is not available then
    //    ///         the current thread becomes disabled for thread scheduling
    //    ///         purposes and lies dormant until one of two things happens:
    //    ///         <ul>
    //    ///             <li>
    //    ///                 The lock is acquired by the current thread; or
    //    ///                 <li>The specified waiting time elapses
    //    ///         </ul>
    //    ///         <p />
    //    ///         <p>
    //    ///             <b>Warning:</b>
    //    ///         </p>
    //    ///         This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///         the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///         defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key to lock in this map</param>
    //    /// <param name="time">maximum time to wait for the lock</param>
    //    /// <param name="timeunit">time unit of the <tt>time</tt> argument.</param>
    //    /// <returns>
    //    ///     <tt>true</tt> if the lock was acquired and <tt>false</tt>
    //    ///     if the waiting time elapsed before the lock was acquired.
    //    /// </returns>
    //    /// <exception cref="System.Exception"></exception>
    //    bool TryLock(K key, long time, TimeUnit timeunit);

    //    /// <summary>Releases the lock for the specified key.</summary>
    //    /// <remarks>
    //    ///     Releases the lock for the specified key. It never blocks and
    //    ///     returns immediately.
    //    ///     <p />
    //    ///     <p>
    //    ///         If the current thread is the holder of this lock then the hold
    //    ///         count is decremented.  If the hold count is now zero then the lock
    //    ///         is released.  If the current thread is not the holder of this
    //    ///         lock then
    //    ///         <see cref="Hazelcast.Net.Ext.IllegalMonitorStateException">Hazelcast.Net.Ext.IllegalMonitorStateException</see>
    //    ///         is thrown.
    //    ///         <p />
    //    ///         <p />
    //    ///         <p>
    //    ///             <b>Warning:</b>
    //    ///         </p>
    //    ///         This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///         the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///         defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key to lock.</param>
    //    /// <exception cref="Hazelcast.Net.Ext.IllegalMonitorStateException">if the current thread does not hold this lock</exception>
    //    void Unlock(K key);

    //    /// <summary>Releases the lock for the specified key regardless of the lock owner.</summary>
    //    /// <remarks>
    //    ///     Releases the lock for the specified key regardless of the lock owner.
    //    ///     It always successfully unlocks the key, never blocks
    //    ///     and returns immediately.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key to lock.</param>
    //    void ForceUnlock(K key);

    //    /// <summary>Adds a local entry listener for this map.</summary>
    //    /// <remarks>
    //    ///     Adds a local entry listener for this map. Added listener will be only
    //    ///     listening for the events (add/remove/update/evict) of the locally owned entries.
    //    ///     <p />
    //    ///     Note that entries in distributed map are partitioned across
    //    ///     the cluster members; each member owns and manages the some portion of the
    //    ///     entries. Owned entries are called local entries. This
    //    ///     listener will be listening for the events of local entries. Let's say
    //    ///     your cluster has member1 and member2. On member2 you added a local listener and from
    //    ///     member1, you call <code>map.put(key2, value2)</code>.
    //    ///     If the key2 is owned by member2 then the local listener will be
    //    ///     notified for the add/update event. Also note that entries can migrate to
    //    ///     other nodes for load balancing and/or membership change.
    //    /// </remarks>
    //    /// <param name="listener">entry listener</param>
    //    /// <seealso cref="IHazelcastMap{K, V}.LocalKeySet()">IMap&lt;K, V&gt;.LocalKeySet()</seealso>
    //    string AddLocalEntryListener(IEntryListener<K, V> listener);

    //    /// <summary>Adds an interceptor for this map.</summary>
    //    /// <remarks>
    //    ///     Adds an interceptor for this map. Added interceptor will intercept operations
    //    ///     and execute user defined methods and will cancel operations if user defined method throw exception.
    //    ///     <p />
    //    /// </remarks>
    //    /// <param name="interceptor">map interceptor</param>
    //    /// <returns>id of registered interceptor</returns>
    //    string AddInterceptor(MapInterceptor interceptor);

    //    /// <summary>Removes the given interceptor for this map.</summary>
    //    /// <remarks>
    //    ///     Removes the given interceptor for this map. So it will not intercept operations anymore.
    //    ///     <p />
    //    /// </remarks>
    //    /// <param name="id">registration id of map interceptor</param>
    //    void RemoveInterceptor(string id);

    //    /// <summary>Adds an entry listener for this map.</summary>
    //    /// <remarks>
    //    ///     Adds an entry listener for this map. Listener will get notified
    //    ///     for all map add/remove/update/evict events.
    //    /// </remarks>
    //    /// <param name="listener">entry listener</param>
    //    /// <param name="includeValue">
    //    ///     <tt>true</tt> if <tt>EntryEvent</tt> should
    //    ///     contain the value.
    //    /// </param>
    //    string AddEntryListener(IEntryListener<K, V> listener, bool includeValue);

    //    /// <summary>
    //    ///     Removes the specified entry listener
    //    ///     Returns silently if there is no such listener added before.
    //    /// </summary>
    //    /// <remarks>
    //    ///     Removes the specified entry listener
    //    ///     Returns silently if there is no such listener added before.
    //    /// </remarks>
    //    /// <param name="id">id of registered listener</param>
    //    /// <returns>true if registration is removed, false otherwise</returns>
    //    bool RemoveEntryListener(string id);

    //    /// <summary>Adds the specified entry listener for the specified key.</summary>
    //    /// <remarks>
    //    ///     Adds the specified entry listener for the specified key.
    //    ///     The listener will get notified for all
    //    ///     add/remove/update/evict events of the specified key only.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="listener">entry listener</param>
    //    /// <param name="key">key to listen</param>
    //    /// <param name="includeValue">
    //    ///     <tt>true</tt> if <tt>EntryEvent</tt> should
    //    ///     contain the value.
    //    /// </param>
    //    string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue);

    //    /// <summary>Adds an continuous entry listener for this map.</summary>
    //    /// <remarks>
    //    ///     Adds an continuous entry listener for this map. Listener will get notified
    //    ///     for map add/remove/update/evict events filtered by given predicate.
    //    /// </remarks>
    //    /// <param name="listener">entry listener</param>
    //    /// <param name="predicate">predicate for filtering entries</param>
    //    /// <param name="includeValue">
    //    ///     <tt>true</tt> if <tt>EntryEvent</tt> should
    //    ///     contain the value.
    //    /// </param>
    //    string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, bool includeValue);

    //    /// <summary>Adds an continuous entry listener for this map.</summary>
    //    /// <remarks>
    //    ///     Adds an continuous entry listener for this map. Listener will get notified
    //    ///     for map add/remove/update/evict events filtered by given predicate.
    //    /// </remarks>
    //    /// <param name="listener">entry listener</param>
    //    /// <param name="predicate">predicate for filtering entries</param>
    //    /// <param name="key">key to listen</param>
    //    /// <param name="includeValue">
    //    ///     <tt>true</tt> if <tt>EntryEvent</tt> should
    //    ///     contain the value.
    //    /// </param>
    //    string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, K key, bool includeValue);

    //    /// <summary>Returns the <tt>IEntryView</tt> for the specified key.</summary>
    //    /// <remarks>
    //    ///     Returns the <tt>IEntryView</tt> for the specified key.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     <p>
    //    ///         This method returns a clone of original mapping, modifying the returned value does not change
    //    ///         the actual value in the map. One should put modified value back to make changes visible to all nodes.
    //    ///     </p>
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning-2:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key of the entry</param>
    //    /// <returns><tt>IEntryView</tt> of the specified key</returns>
    //    /// <seealso cref="IEntryView{K,V}">IEntryView&lt;K, V&gt;</seealso>
    //    IEntryView<K, V> GetEntryView(K key);

    //    /// <summary>Evicts the specified key from this map.</summary>
    //    /// <remarks>
    //    ///     Evicts the specified key from this map. If
    //    ///     a <tt>MapStore</tt> defined for this map, then the entry is not
    //    ///     deleted from the underlying <tt>MapStore</tt>, evict only removes
    //    ///     the entry from the memory.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     This method uses <tt>GetHashCode</tt> and <tt>Equals</tt> of binary form of
    //    ///     the <tt>key</tt>, not the actual implementations of <tt>GetHashCode</tt> and <tt>Equals</tt>
    //    ///     defined in <tt>key</tt>'s class.
    //    /// </remarks>
    //    /// <param name="key">key to evict</param>
    //    /// <returns><tt>true</tt> if the key is evicted, <tt>false</tt> otherwise.</returns>
    //    bool Evict(K key);

    //    /// <summary>Returns a collection clone of the values contained in this map.</summary>
    //    /// <remarks>
    //    ///     Returns a collection clone of the values contained in this map.
    //    ///     The collection is <b>NOT</b> backed by the map,
    //    ///     so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
    //    /// </remarks>
    //    /// <returns>a collection clone of the values contained in this map</returns>
    //    ICollection<V> Values();


    //    /// <summary>
    //    ///     Returns a
    //    ///     <see cref="Hazelcast.Net.Ext.Set{E}">Hazelcast.Net.Ext.Set&lt;E&gt;</see>
    //    ///     clone of the mappings contained in this map.
    //    ///     The set is <b>NOT</b> backed by the map,
    //    ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
    //    /// </summary>
    //    /// <returns>a set clone of the keys mappings in this map</returns>
    //    ICollection<KeyValuePair<K, V>> EntrySet();

    //    /// <summary>
    //    ///     Queries the map based on the specified predicate and
    //    ///     returns the keys of matching entries.
    //    /// </summary>
    //    /// <remarks>
    //    ///     Queries the map based on the specified predicate and
    //    ///     returns the keys of matching entries.
    //    ///     <p />
    //    ///     Specified predicate runs on all members in parallel.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     The set is <b>NOT</b> backed by the map,
    //    ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
    //    /// </remarks>
    //    /// <param name="predicate">query criteria</param>
    //    /// <returns>result key set of the query</returns>
    //    ICollection<K> KeySet(IPredicate<K, V> predicate);

    //    /// <summary>
    //    ///     Queries the map based on the specified predicate and
    //    ///     returns the matching entries.
    //    /// </summary>
    //    /// <remarks>
    //    ///     Queries the map based on the specified predicate and
    //    ///     returns the matching entries.
    //    ///     <p />
    //    ///     Specified predicate runs on all members in parallel.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     The set is <b>NOT</b> backed by the map,
    //    ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
    //    /// </remarks>
    //    /// <param name="predicate">query criteria</param>
    //    /// <returns>result entry set of the query</returns>
    //    ICollection<KeyValuePair<K, V>> EntrySet(IPredicate<K, V> predicate);

    //    /// <summary>
    //    ///     Queries the map based on the specified predicate and
    //    ///     returns the values of matching entries.
    //    /// </summary>
    //    /// <remarks>
    //    ///     Queries the map based on the specified predicate and
    //    ///     returns the values of matching entries.
    //    ///     <p />
    //    ///     Specified predicate runs on all members in parallel.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     The collection is <b>NOT</b> backed by the map,
    //    ///     so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
    //    /// </remarks>
    //    /// <param name="predicate">query criteria</param>
    //    /// <returns>result value collection of the query</returns>
    //    ICollection<V> Values(IPredicate<K, V> predicate);

    //    /// <summary>Returns the locally owned set of keys.</summary>
    //    /// <remarks>
    //    ///     Returns the locally owned set of keys.
    //    ///     <p />
    //    ///     Each key in this map is owned and managed by a specific
    //    ///     member in the cluster.
    //    ///     <p />
    //    ///     Note that ownership of these keys might change over time
    //    ///     so that key ownerships can be almost evenly distributed
    //    ///     in the cluster.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     The set is <b>NOT</b> backed by the map,
    //    ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
    //    /// </remarks>
    //    /// <returns>locally owned keys.</returns>
    //    ICollection<K> LocalKeySet();

    //    /// <summary>Returns the keys of matching locally owned entries.</summary>
    //    /// <remarks>
    //    ///     Returns the keys of matching locally owned entries.
    //    ///     <p />
    //    ///     Each key in this map is owned and managed by a specific
    //    ///     member in the cluster.
    //    ///     <p />
    //    ///     Note that ownership of these keys might change over time
    //    ///     so that key ownerships can be almost evenly distributed
    //    ///     in the cluster.
    //    ///     <p />
    //    ///     <p>
    //    ///         <b>Warning:</b>
    //    ///     </p>
    //    ///     The set is <b>NOT</b> backed by the map,
    //    ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
    //    /// </remarks>
    //    /// <param name="predicate">query criteria</param>
    //    /// <returns>keys of matching locally owned entries.</returns>
    //    ICollection<K> LocalKeySet(IPredicate<K, V> predicate);

    //    /// <summary>
    //    ///     Adds an index to this map for the specified entries so
    //    ///     that queries can run faster.
    //    /// </summary>
    //    /// <remarks>
    //    ///     Adds an index to this map for the specified entries so
    //    ///     that queries can run faster.
    //    ///     <p />
    //    ///     Let's say your map values are Employee objects.
    //    ///     <pre>
    //    ///         public class Employee implements Serializable {
    //    ///         private boolean active = false;
    //    ///         private int age;
    //    ///         private String name = null;
    //    ///         // other fields.
    //    ///         // getters setter
    //    ///         }
    //    ///     </pre>
    //    ///     <p />
    //    ///     If you are querying your values mostly based on age and active then
    //    ///     you should consider indexing these fields.
    //    ///     <pre>
    //    ///         IMap imap = Hazelcast.getMap("employees");
    //    ///         imap.addIndex("age", true);        // ordered, since we have ranged queries for this field
    //    ///         imap.addIndex("active", false);    // not ordered, because boolean field cannot have range
    //    ///     </pre>
    //    ///     <p />
    //    ///     Index attribute should either have a getter method or be public.
    //    ///     You should also make sure to add the indexes before adding
    //    ///     entries to this map.
    //    /// </remarks>
    //    /// <param name="attribute">attribute of value</param>
    //    /// <param name="ordered">
    //    ///     <tt>true</tt> if index should be ordered,
    //    ///     <tt>false</tt> otherwise.
    //    /// </param>
    //    void AddIndex(string attribute, bool ordered);

    //    //    /**
    //    //     * Returns LocalMapStats for this map.
    //    //     * LocalMapStats is the statistics for the local portion of this
    //    //     * distributed map and contains information such as ownedEntryCount
    //    //     * backupEntryCount, lastUpdateTime, lockedEntryCount.
    //    //     * <p/>
    //    //     * Since this stats are only for the local portion of this map, if you
    //    //     * need the cluster-wide MapStats then you need to get the LocalMapStats
    //    //     * from all members of the cluster and combine them.
    //    //     *
    //    //     * @return this map's local statistics.
    //    //     */
    //    //    LocalMapStats getLocalMapStats();
    //    /// <summary>Applies the user defined EntryProcessor to the entry mapped by the key.</summary>
    //    /// <remarks>
    //    ///     Applies the user defined EntryProcessor to the entry mapped by the key.
    //    ///     Returns the the object which is result of the process() method of EntryProcessor.
    //    ///     <p />
    //    /// </remarks>
    //    /// <returns>result of entry process.</returns>
    //    object ExecuteOnKey(K key, EntryProcessor<K, V> entryProcessor);

    //    /// <summary>Applies the user defined EntryProcessor to the all entries in the map.</summary>
    //    /// <remarks>
    //    ///     Applies the user defined EntryProcessor to the all entries in the map.
    //    ///     Returns the results mapped by each key in the map.
    //    ///     <p />
    //    /// </remarks>
    //    IDictionary<K, object> ExecuteOnEntries(EntryProcessor<K, V> entryProcessor);

    //    /// <summary>Applies the user defined EntryProcessor to the entries in the map which satisfies provided predicate.</summary>
    //    /// <remarks>
    //    ///     Applies the user defined EntryProcessor to the entries in the map which satisfies provided predicate.
    //    ///     Returns the results mapped by each key in the map.
    //    ///     <p />
    //    /// </remarks>
    //    IDictionary<K, object> ExecuteOnEntries(EntryProcessor<K, V> entryProcessor, IPredicate<K, V> predicate);
    //}
}