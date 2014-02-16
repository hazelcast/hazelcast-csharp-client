using System;
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
    ///                 <see cref="IMapMap{K,V}.Get(object)">IMap&lt;K, V&gt;.Get(object)</see>
    ///             </li>
    ///             <li>
    ///                 Methods, including but not limited to <tt>keySet</tt>, <tt>values</tt>, <tt>entrySet</tt>,
    ///                 return a collection clone of the values. The collection is <b>NOT</b> backed by the map,
    ///                 so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
    ///             </li>
    ///         </ul>
    ///     </p>
    /// </remarks>
    public interface IMap<K, V> : IDistributedObject
    {

        string AddLocalEntryListener(IEntryListener<K, V> listener);
        string AddInterceptor(MapInterceptor interceptor);
        void RemoveInterceptor(string id);
        string AddEntryListener(IEntryListener<K, V> listener, bool includeValue);
        bool RemoveEntryListener(string id);
        string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue);
        string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, K key, bool includeValue);
        string AddEntryListener(IEntryListener<K, V> listener, IPredicate<K, V> predicate, bool includeValue);
        void AddIndex(string attribute, bool ordered);
 
        bool ContainsKey(object key);
        bool ContainsValue(object value);
 
        void Delete(object key);

        V Get(object key);
        Task<V> GetAsync(K key);
        IDictionary<K, V> GetAll(ICollection<K> keys);

        V Put(K key, V value);
        V Put(K key, V value, long ttl, TimeUnit timeunit);
        Task<V> PutAsync(K key, V value);
        Task<V> PutAsync(K key, V value, long ttl, TimeUnit timeunit);
        V PutIfAbsent(K key, V value);
        bool TryPut(K key, V value, long timeout, TimeUnit timeunit);
        void PutTransient(K key, V value, long ttl, TimeUnit timeunit);
        V PutIfAbsent(K key, V value, long ttl, TimeUnit timeunit);
        void PutAll<K>(IDictionary<K, V> m);

        V Replace(K key, V value);
        bool Replace(K key, V oldValue, V newValue);
        bool TryRemove(K key, long timeout, TimeUnit timeunit);
        V Remove(object key);
        bool Remove(object key, object value);
        Task<V> RemoveAsync(K key);

        void Set(K key, V value);
        void Set(K key, V value, long ttl, TimeUnit timeunit);

        void Lock(K key);
        void Lock(K key, long leaseTime, TimeUnit timeUnit);
        bool IsLocked(K key);
        bool TryLock(K key);
        bool TryLock(K key, long time, TimeUnit timeunit);
        void Unlock(K key);
        void ForceUnlock(K key);

        ICollection<KeyValuePair<K, V>> EntrySet();
        ICollection<KeyValuePair<K, V>> EntrySet(IPredicate<K, V> predicate);
        ISet<K> KeySet();
        ICollection<K> KeySet(IPredicate<K, V> predicate);
        ICollection<V> Values();
        ICollection<V> Values(IPredicate<K, V> predicate);
        IEntryView<K, V> GetEntryView(K key);


        void Clear();
        bool IsEmpty();
        int Size();
        void Flush();
        bool Evict(K key);


        //ICollection<K> LocalKeySet();
        //ICollection<K> LocalKeySet(IPredicate<K, V> predicate);
        //object ExecuteOnKey(K key, EntryProcessor<K, V> entryProcessor);
        //IDictionary<K, object> ExecuteOnEntries(EntryProcessor<K, V> entryProcessor);
        //IDictionary<K, object> ExecuteOnEntries(EntryProcessor<K, V> entryProcessor, IPredicate<K, V> predicate);
    }
}