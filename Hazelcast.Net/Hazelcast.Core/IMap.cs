/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Map;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Concurrent, distributed, observable and queryable map.
    /// </summary>
    /// <remarks>
    ///     Concurrent, distributed, observable and queryable map.
    ///     <p />
    ///     <p>
    ///         <b>
    ///             This class is <i>not</i> a general-purpose <c>ConcurrentMap</c> implementation! While this class
    ///             implements
    ///             the <c>Map</c> interface, it intentionally violates <c>Map's</c> general contract, which mandates the
    ///             use of the <c>Equals</c> method when comparing objects. Instead of the <c>Equals</c> method this
    ///             implementation compares the serialized byte version of the objects.
    ///         </b>
    ///     </p>
    ///     <p />
    ///     <p>
    ///         <b>Gotchas:</b>
    ///         <ul>
    ///             <li>
    ///                 Methods, including but not limited to <c>Get</c>, <c>ContainsKey</c>,
    ///                 <c>ContainsValue</c>, <c>Evict</c>, <c>Remove</c>, <c>Put</c>,
    ///                 <c>PutIfAbsent</c>, <c>Replace</c>, <c>Lock</c>,
    ///                 <c>Unlock</c>, do not use <c>GetHashCode</c> and <c>Equals</c> implementations of keys,
    ///                 instead they use <c>GetHashCode</c> and <c>Equals</c> of binary (serialized) forms of the objects.
    ///             </li>
    ///             <li>
    ///                 <c>Get</c> method returns a clone of original values, modifying the returned value does not
    ///                 change the actual value in the map. One should put modified value back to make changes visible to all
    ///                 nodes.
    ///                 For additional info see
    ///                 <see cref="IMap{K,V}.Get(object)">IMap&lt;K, V&gt;.Get(object)</see>
    ///             </li>
    ///             <li>
    ///                 Methods, including but not limited to <c>keySet</c>, <c>values</c>, <c>entrySet</c>,
    ///                 return a collection clone of the values. The collection is <b>NOT</b> backed by the map,
    ///                 so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
    ///             </li>
    ///         </ul>
    ///     </p>
    /// </remarks>
    /// <typeparam name="TKey">type of the Key</typeparam>
    /// <typeparam name="TValue">type of the Value</typeparam>
    public interface IMap<TKey, TValue> : IDistributedObject
    {
        /// <summary>Adds an interceptor for this map.</summary>
        /// <remarks>
        ///     Adds an interceptor for this map. Added interceptor will intercept operations
        ///     and execute user defined methods and will cancel operations if user defined method throw exception.
        /// </remarks>
        /// <param name="interceptor">map interceptor</param>
        /// <returns>id of registered interceptor</returns>
        string AddInterceptor(IMapInterceptor interceptor);

        /// <summary>Removes the given interceptor for this map.</summary>
        /// <remarks>
        ///     Removes the given interceptor for this map. So it will not intercept operations anymore.
        /// </remarks>
        /// <param name="id">registration id of map interceptor</param>
        void RemoveInterceptor(string id);

        /// <summary>
        ///     Adds an entry listener for this map.
        /// </summary>
        /// <remarks>
        ///     Adds an entry listener for this map. Listener will get notified for all map add/remove/update/evict events.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <param name="includeValue"><c>true</c> if <c>EntryEvent</c> should contain the value.</param>
        /// <returns>returns registration id</returns>
        string AddEntryListener(IEntryListener<TKey, TValue> listener, bool includeValue);

        /// <summary>
        ///     Removes the specified entry listener
        ///     Returns silently if there is no such listener added before.
        /// </summary>
        /// <remarks>
        ///     Removes the specified entry listener
        ///     Returns silently if there is no such listener added before.
        /// </remarks>
        /// <param name="id">id of registered listener</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveEntryListener(string id);

        /// <summary>Adds the specified entry listener for the specified key.</summary>
        /// <remarks>
        ///     Adds the specified entry listener for the specified key.
        ///     The listener will get notified for all
        ///     add/remove/update/evict events of the specified key only.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>hashCode</c> and <c>equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>hashCode</c> and <c>equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <param name="key">key to listen</param>
        /// <param name="includeValue">
        ///     <c>true</c> if <c>EntryEvent</c> should
        ///     contain the value.
        /// </param>
        /// <returns>returns registration id</returns>
        string AddEntryListener(IEntryListener<TKey, TValue> listener, TKey key, bool includeValue);

        /// <summary>Adds an continuous entry listener for this map.</summary>
        /// <remarks>
        ///     Adds an continuous entry listener for this map. Listener will get notified
        ///     for map add/remove/update/evict events filtered by given predicate.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <param name="predicate">predicate for filtering entries</param>
        /// <param name="key">key to listen</param>
        /// <param name="includeValue">
        ///     <c>true</c> if <c>EntryEvent</c> should
        ///     contain the value.
        /// </param>
        /// <returns>returns registration id</returns>
        string AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate<TKey, TValue> predicate, TKey key, bool includeValue);

        /// <summary>Adds an continuous entry listener for this map.</summary>
        /// <remarks>
        ///     Adds an continuous entry listener for this map. Listener will get notified
        ///     for map add/remove/update/evict events filtered by given predicate.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <param name="predicate">predicate for filtering entries</param>
        /// <param name="includeValue">
        ///     <c>true</c> if <c>EntryEvent</c> should
        ///     contain the value.
        /// </param>
        /// <returns>returns registration id</returns>
        string AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate<TKey, TValue> predicate, bool includeValue);

        /// <summary>
        ///     Adds an index to this map for the specified entries so
        ///     that queries can run faster.
        /// </summary>
        /// <remarks>
        ///     Adds an index to this map for the specified entries so
        ///     that queries can run faster.
        ///     <p />
        ///     Let's say your map values are Employee objects.
        ///     <code>
        /// public class Employee :IPortable
        /// {
        ///     private bool active = false;
        ///     private int age;
        ///     private string name = null;
        ///     // other fields.
        ///     
        /// }
        /// </code>
        ///     <p />
        ///     If you are querying your values mostly based on age and active then
        ///     you should consider indexing these fields.
        ///     <code>
        /// var imap = Hazelcast.GetMap("employees");
        /// imap.AddIndex("age", true);        // ordered, since we have ranged queries for this field
        /// imap.AddIndex("active", false);    // not ordered, because boolean field cannot have range
        /// </code>
        ///     <p />
        ///     Index attribute should either have a getter method or be public.
        ///     You should also make sure to add the indexes before adding
        ///     entries to this map.
        /// </remarks>
        /// <param name="attribute">attribute of value</param>
        /// <param name="ordered">
        ///     <c>true</c> if index should be ordered,
        ///     <c>false</c> otherwise.
        /// </param>
        void AddIndex(string attribute, bool ordered);

        /// <summary>
        ///     Determines whether this map contains an entry for the specified key.
        /// </summary>
        /// <remarks>
        ///     Determines whether this map contains an entry for the specified key.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         Ë†
        ///         This method uses <c>hashCode</c> and <c>equals</c> of binary form of
        ///         the <c>key</c>, not the actual implementations of <c>hashCode</c> and <c>equals</c>
        ///         defined in <c>key</c>'s class.
        ///     </p>
        /// </remarks>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key contains key; otherwise, <c>false</c>.</returns>
        bool ContainsKey(object key);

        /// <summary>
        ///     Determines whether this map contains one or more keys to the specified value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns <c>true</c> if this map contains one or more keys to the specified value</returns>
        bool ContainsValue(object value);

        /// <summary>
        ///     Removes the mapping for a key from this map if it is present
        ///     (optional operation).
        /// </summary>
        /// <remarks>
        ///     Removes the mapping for a key from this map if it is present
        ///     (optional operation).
        ///     <p>
        ///         this operation does not return removed value to avoid serialization cost of returned value.
        ///         If the removed value will not be used, delete operation
        ///         should be preferred over remove operation for a better performance.
        ///     </p>
        ///     <p>
        ///         The map will not contain a mapping for the specified key once the call returns.
        ///     </p>
        /// </remarks>
        /// <param name="key">key whose mapping is to be removed from the map</param>
        /// <exception cref="System.InvalidCastException">
        ///     if the key is of an inappropriate type for
        ///     this map (optional)
        /// </exception>
        /// <exception cref="System.ArgumentNullException">if the specified key is null.</exception>
        void Delete(object key);

        /// <summary>
        ///     Returns the value for the specified key, or <c>null</c> if this map does not contain this key.
        /// </summary>
        /// <remarks>
        ///     Returns the value for the specified key, or <c>null</c> if this map does not contain this key.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns a clone of original value, modifying the returned value does not change
        ///         the actual value in the map. One should put modified value back to make changes visible to all nodes.
        ///         <code>
        /// var value = map.Get(key);
        /// value.UpdateSomeProperty();
        /// map.put(key, value);
        /// </code>
        ///     </p>
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        ///     <p />
        /// </remarks>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        TValue Get(object key);

        /// <summary>Asynchronously gets the given key.</summary>
        /// <remarks>
        ///     Asynchronously gets the given key.
        ///     <code>
        /// Task&lt;V&gt; task = map.GetAsync(key);
        /// // do some other stuff, when ready get the result
        /// V value = task.Result;
        /// </code>
        ///     Task.Result will block until the actual map.Get() completes.
        ///     If the application requires timely response,
        ///     then task.Wait(timeout) can be used.
        ///     <code>
        /// try
        /// {
        ///     Task&lt;V&gt; task = map.GetAsync(key);
        ///     if(task.Wait(TimeSpan.FromMilliseconds(40)))
        ///     {
        ///        V value = task.Result; 
        ///     }
        ///     else
        ///     {
        ///      //Result not ready
        ///     }
        /// </code>
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">the key of the map entry</param>
        /// <returns>Task&lt;V&gt; from which the value of the key can be retrieved.</returns>
        /// <seealso cref="Task{V}" />
        Task<TValue> GetAsync(TKey key);

        /// <summary>Returns the entries for the given keys.</summary>
        /// <remarks>
        ///     Returns the entries for the given keys.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     The returned map is <b>NOT</b> backed by the original map,
        ///     so changes to the original map are <b>NOT</b> reflected in the returned map, and vice-versa.
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="keys">keys to get</param>
        /// <returns>map of entries</returns>
        IDictionary<TKey, TValue> GetAll(ICollection<TKey> keys);

        /// <summary>
        ///     Associates the specified value with the specified key in this map
        ///     If the map previously contained a mapping for
        ///     the key, the old value is replaced by the specified value.
        /// </summary>
        /// <remarks>
        ///     Associates the specified value with the specified key in this map
        ///     If the map previously contained a mapping for
        ///     the key, the old value is replaced by the specified value.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns a clone of previous value, not the original (identically equal) value
        ///         previously put into map.
        ///     </p>
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>
        ///     previous value associated with
        ///     <c>key</c>
        ///     or
        ///     <c>null</c>
        ///     if there was no mapping for
        ///     <c>key</c>
        ///     .
        /// </returns>
        TValue Put(TKey key, TValue value);

        /// <summary>Puts an entry into this map with a given ttl (time to live) value.</summary>
        /// <remarks>
        ///     Puts an entry into this map with a given ttl (time to live) value.
        ///     Entry will expire and get evicted after the ttl. If ttl is 0, then
        ///     the entry lives forever.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     <p>
        ///         This method returns a clone of previous value, not the original (identically equal) value
        ///         previously put into map.
        ///     </p>
        /// </remarks>
        /// <param name="key">key of the entry</param>
        /// <param name="value">value of the entry</param>
        /// <param name="ttl">maximum time for this entry to stay in the map, 0 means infinite.</param>
        /// <param name="timeunit"><see cref="TimeUnit" />for the ttl</param>
        /// <returns>old value of the entry</returns>
        TValue Put(TKey key, TValue value, long ttl, TimeUnit timeunit);

        /// <summary>Asynchronously puts the given key and value.</summary>
        /// <remarks>
        ///     Asynchronously puts the given key and value.
        ///     <code>
        /// Task&lt;V&gt; task = map.PutAsync(key, value);
        /// // do some other stuff, when ready get the result
        /// V oldValue = task.Result;
        /// </code>
        ///     Task.Result will block until the actual map.Put() completes.
        ///     If the application requires timely response,
        ///     then task.Wait(timeout) can be used.
        ///     <code>
        /// try
        /// {
        ///     Task&lt;V&gt; task = map.PutAsync(key, value);
        ///     if(task.Wait(TimeSpan.FromMilliseconds(40)))
        ///     {
        ///        V value = task.Result; 
        ///     }
        ///     else
        ///     {
        ///      //Result not ready
        ///     }
        /// </code>
        ///     ExecutionException is never thrown.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">the key of the map entry</param>
        /// <param name="value">the new value of the map entry</param>
        /// <returns>Task&lt;V&gt; from which the old value of the key can be retrieved.</returns>
        /// <seealso cref="Task{V}" />
        Task<TValue> PutAsync(TKey key, TValue value);

        /// <summary>Asynchronously puts the given key and value into this map with a given ttl (time to live) value.</summary>
        /// <remarks>
        ///     Asynchronously puts the given key and value into this map with a given ttl (time to live) value.
        ///     Entry will expire and get evicted after the ttl. If ttl is 0, then
        ///     the entry lives forever.
        ///     <code>
        /// Task&lt;V&gt; task = map.PutAsync(key, value, ttl, timeunit);
        /// // do some other stuff, when ready get the result
        /// V oldValue = task.Result;
        /// </code>
        ///     Task.Result will block until the actual map.Put() completes.
        ///     If the application requires timely response,
        ///     then task.Wait(timeout) can be used.
        ///     <code>
        /// try
        /// {
        ///     Task&lt;V&gt; task = map.PutAsync(key, value, ttl, timeunit);
        ///     if(task.Wait(TimeSpan.FromMilliseconds(40)))
        ///     {
        ///        V value = task.Result; 
        ///     }
        ///     else
        ///     {
        ///      //Result not ready
        ///     }
        /// </code>
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">the key of the map entry</param>
        /// <param name="value">the new value of the map entry</param>
        /// <param name="ttl">
        ///     maximum time for this entry to stay in the map
        ///     0 means infinite.
        /// </param>
        /// <param name="timeunit">time unit for the ttl</param>
        /// <returns>Task&lt;V&gt; from which the old value of the key can be retrieved.</returns>
        /// <seealso cref="Task{V}" />
        Task<TValue> PutAsync(TKey key, TValue value, long ttl, TimeUnit timeunit);

        /// <summary>
        ///     If the specified key is not already associated
        ///     with a value, associate it with the given value.
        /// </summary>
        /// <remarks>
        ///     If the specified key is not already associated
        ///     with a value, associate it with the given value.
        ///     This is equivalent to
        ///     <code>
        /// if (!map.ContainsKey(key))
        ///     return map.Put(key, value);
        /// else
        ///     return map.Get(key);
        /// </code>
        ///     except that the action is performed atomically.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     <p>
        ///         This method returns a clone of previous value, not the original (identically equal) value
        ///         previously put into map.
        ///     </p>
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns> previous value associated with <c>key</c> or <c>null</c> if there was no mapping for <c>key</c> .</returns>
        TValue PutIfAbsent(TKey key, TValue value);

        /// <summary>
        ///     Puts an entry into this map with a given ttl (time to live) value
        ///     if the specified key is not already associated with a value.
        /// </summary>
        /// <remarks>
        ///     Puts an entry into this map with a given ttl (time to live) value
        ///     if the specified key is not already associated with a value.
        ///     Entry will expire and get evicted after the ttl.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     <p>
        ///         This method returns a clone of previous value, not the original (identically equal) value
        ///         previously put into map.
        ///     </p>
        /// </remarks>
        /// <param name="key">key of the entry</param>
        /// <param name="value">value of the entry</param>
        /// <param name="ttl">maximum time for this entry to stay in the map</param>
        /// <param name="timeunit"><see cref="TimeUnit" /> for the timeout</param>
        /// <returns>old value of the entry</returns>
        TValue PutIfAbsent(TKey key, TValue value, long ttl, TimeUnit timeunit);

        /// <summary>
        ///     Tries to put the given key, value into this map within specified timeout value.
        /// </summary>
        /// <remarks>
        ///     Tries to put the given key, value into this map within specified
        ///     timeout value. If this method returns false, it means that
        ///     the caller thread couldn't acquire the lock for the key within
        ///     timeout duration, thus put operation is not successful.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key of the entry</param>
        /// <param name="value">value of the entry</param>
        /// <param name="timeout">maximum time to wait</param>
        /// <param name="timeunit"><see cref="TimeUnit" /> for the timeout</param>
        /// <returns> <c>true</c> if the put is successful, <c>false</c> otherwise. </returns>
        bool TryPut(TKey key, TValue value, long timeout, TimeUnit timeunit);

        /// <summary>
        ///     Same as <see cref="IMap{K, V}.Put(TKey, TValue, long, TimeUnit)" />
        ///     but MapStore, if defined,
        ///     will not be called to store/persist the entry.  If ttl is 0, then
        ///     the entry lives forever.
        /// </summary>
        /// <remarks>
        ///     Same as <see cref="IMap{K, V}.Put(TKey, TValue, long, TimeUnit)" />
        ///     but MapStore, if defined,
        ///     will not be called to store/persist the entry.  If ttl is 0, then
        ///     the entry lives forever.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key of the entry</param>
        /// <param name="value">value of the entry</param>
        /// <param name="ttl">
        ///     maximum time for this entry to stay in the map.
        ///     0 means infinite.
        /// </param>
        /// <param name="timeunit"><see cref="TimeUnit" /> for the ttl</param>
        void PutTransient(TKey key, TValue value, long ttl, TimeUnit timeunit);

        /// <summary>
        ///     Copies all of the mappings from the specified map to this map
        /// </summary>
        /// <param name="m">mappings to be stored in this map</param>
        void PutAll(IDictionary<TKey, TValue> m);

        /// <summary>Replaces the entry for a key only if currently mapped to some value.</summary>
        /// <remarks>
        ///     Replaces the entry for a key only if currently mapped to some value.
        ///     This is equivalent to
        ///     <code>
        /// if (map.ContainsKey(key))
        ///     return map.put(key, value);
        /// else 
        ///     return null;
        /// </code>
        ///     >
        ///     except that the action is performed atomically.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns> previous value associated with <c>key</c> or <c>null</c> if there was no mapping for <c>key</c> . </returns>
        TValue Replace(TKey key, TValue value);

        /// <summary>Replaces the entry for a key only if currently mapped to a given value.</summary>
        /// <remarks>
        ///     Replaces the entry for a key only if currently mapped to a given value.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="oldValue">old value</param>
        /// <param name="newValue">new value</param>
        /// <returns><c>true</c> if the value was replaced </returns>
        bool Replace(TKey key, TValue oldValue, TValue newValue);

        /// <summary>
        ///     Tries to remove the entry with the given key from this map
        ///     within specified timeout value.
        /// </summary>
        /// <remarks>
        ///     Tries to remove the entry with the given key from this map
        ///     within specified timeout value. If the key is already locked by another
        ///     thread and/or member, then this operation will wait timeout
        ///     amount for acquiring the lock.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     <p>
        ///         This method returns a clone of previous value, not the original (identically equal) value
        ///         previously put into map.
        ///     </p>
        /// </remarks>
        /// <param name="key">key of the entry</param>
        /// <param name="timeout"> maximum time to wait for acquiring the lock for the key </param>
        /// <param name="timeunit"><see cref="TimeUnit" /> for the ttl</param>
        /// <returns> <c>true</c> if the remove is successful, <c>false</c> otherwise. </returns>
        bool TryRemove(TKey key, long timeout, TimeUnit timeunit);

        /// <summary>Removes the mapping for a key from this map if it is present.</summary>
        /// <remarks>
        ///     Removes the mapping for a key from this map if it is present.
        ///     <p>
        ///         The map will not contain a mapping for the specified key once the call returns.
        ///     </p>
        /// </remarks>
        /// <param name="key">key</param>
        /// <returns>
        ///     previous value associated with <c>key</c> or <c>null</c>
        ///     if there was no mapping for <c>key</c> .
        /// </returns>
        TValue Remove(object key);

        /// <summary>Removes the entry for a key only if currently mapped to a given value.</summary>
        /// <remarks>
        ///     Removes the entry for a key only if currently mapped to a given value.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        ///     <p />
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns><c>true</c> if the value was removed </returns>
        bool Remove(object key, object value);

        /// <summary>Asynchronously removes the given key.</summary>
        /// <remarks>
        ///     Asynchronously removes the given key.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">The key of the map entry to remove.</param>
        /// <returns>Task&lt;V&gt; from which the value removed from the map can be retrieved.</returns>
        Task<TValue> RemoveAsync(TKey key);

        /// <summary>Puts an entry into this map.</summary>
        /// <remarks>
        ///     Puts an entry into this map.
        ///     Similar to <see cref="Put(TKey,TValue)" /> operation except that <c>Set</c>
        ///     doesn't return the old value which is more efficient.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key of the entry</param>
        /// <param name="value">value of the entry</param>
        void Set(TKey key, TValue value);

        /// <summary>Puts an entry into this map with a given ttl (time to live) value.</summary>
        /// <remarks>
        ///     Puts an entry into this map with a given ttl (time to live) value.
        ///     Similar to put operation except that set
        ///     doesn't return the old value which is more efficient.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key of the entry</param>
        /// <param name="value">value of the entry</param>
        /// <param name="ttl"> maximum time to wait for acquiring the lock for the key </param>
        /// <param name="timeunit"><see cref="TimeUnit" /> for the ttl</param>
        void Set(TKey key, TValue value, long ttl, TimeUnit timeunit);

        /// <summary>Acquires the lock for the specified key.</summary>
        /// <remarks>
        ///     Acquires the lock for the specified key.
        ///     <p />
        ///     If the lock is not available then
        ///     the current thread becomes disabled for thread scheduling
        ///     purposes and lies dormant until the lock has been acquired.
        ///     <p />
        ///     Scope of the lock is this map only.
        ///     Acquired lock is only for the key in this map.
        ///     <p />
        ///     Locks are re-entrant so if the key is locked N times then
        ///     it should be unlocked N times before another thread can acquire it.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key to lock.</param>
        void Lock(TKey key);

        /// <summary>Acquires the lock for the specified key for the specified lease time.</summary>
        /// <remarks>
        ///     Acquires the lock for the specified key for the specified lease time.
        ///     <p />
        ///     After lease time, lock will be released..
        ///     <p />
        ///     If the lock is not available then
        ///     the current thread becomes disabled for thread scheduling
        ///     purposes and lies dormant until the lock has been acquired.
        ///     <p />
        ///     Scope of the lock is this map only.
        ///     Acquired lock is only for the key in this map.
        ///     <p />
        ///     Locks are re-entrant so if the key is locked N times then
        ///     it should be unlocked N times before another thread can acquire it.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key to lock.</param>
        /// <param name="leaseTime">time to wait before releasing the lock.</param>
        /// <param name="timeUnit"><see cref="TimeUnit" /> for the ttl</param>
        void Lock(TKey key, long leaseTime, TimeUnit timeUnit);

        /// <summary>Checks the lock for the specified key.</summary>
        /// <remarks>
        ///     Checks the lock for the specified key.
        ///     <p />
        ///     If the lock is acquired then returns true, else false.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key to lock to be checked.</param>
        /// <returns><c>true</c> if lock is acquired, <c>false</c> otherwise.</returns>
        bool IsLocked(TKey key);

        /// <summary>Tries to acquire the lock for the specified key.</summary>
        /// <remarks>
        ///     Tries to acquire the lock for the specified key.
        ///     <p>
        ///         If the lock is not available then the current thread
        ///         doesn't wait and returns false immediately.
        ///     </p>
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key to lock.</param>
        /// <returns><c>true</c> if lock is acquired, <c>false</c> otherwise.</returns>
        bool TryLock(TKey key);

        /// <summary>Tries to acquire the lock for the specified key.</summary>
        /// <remarks>
        ///     Tries to acquire the lock for the specified key.
        ///     <p>
        ///         If the lock is not available then
        ///         the current thread becomes disabled for thread scheduling
        ///         purposes and lies dormant until one of two things happens:
        ///         <ul>
        ///             <li>The lock is acquired by the current thread; or</li>
        ///             <li>The specified waiting time elapses</li>
        ///         </ul>
        ///     </p>
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key to lock in this map</param>
        /// <param name="time">maximum time to wait for the lock</param>
        /// <param name="timeunit"><see cref="TimeUnit" /> for the ttl</param>
        /// <returns><c>true</c> if lock is acquired, <c>false</c> otherwise.</returns>
        bool TryLock(TKey key, long time, TimeUnit timeunit);

        /// <summary>Tries to acquire the lock for the specified key for the specified lease time.</summary>
        /// <remarks>
        ///     Tries to acquire the lock for the specified key for the specified lease time.
        ///     <p>
        ///         After lease time, the lock will be released.
        ///         <p />
        ///         <p>
        ///             If the lock is not available, then
        ///             the current thread becomes disabled for thread scheduling
        ///             purposes and lies dormant until one of two things happens:
        ///             <ul>
        ///                 <li>
        ///                     the lock is acquired by the current thread, or</li>
        ///                     <li>the specified waiting time elapses.</li>
        ///             </ul>
        ///             <p />
        ///             <p>
        ///                 <b>Warning:</b>
        ///             </p>
        ///             This method uses <tt>hashCode</tt> and <tt>equals</tt> of the binary form of
        ///             the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        ///             defined in the <tt>key</tt>'s class.
        ///         </p>
        ///     </p>
        /// </remarks>
        /// <param name="key">key to lock in this map.</param>
        /// <param name="time">maximum time to wait for the lock.</param>
        /// <param name="timeunit">time unit of the <tt>time</tt> argument.</param>
        /// <param name="leaseTime">time to wait before releasing the lock.</param>
        /// <param name="leaseTimeunit">unit of time to specify lease time.</param>
        /// <returns>
        ///     <tt>true</tt> if the lock was acquired and <tt>false</tt>
        ///     if the waiting time elapsed before the lock was acquired.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">if the specified key is null.</exception>
        /// <exception cref="System.Exception" />
        bool TryLock(TKey key, long time, TimeUnit timeunit, long leaseTime, TimeUnit leaseTimeunit);

        /// <summary>Releases the lock for the specified key.</summary>
        /// <remarks>
        ///     Releases the lock for the specified key. It never blocks and
        ///     returns immediately.
        ///     <p />
        ///     <p>
        ///         If the current thread is the holder of this lock then the hold
        ///         count is decremented.  If the hold count is now zero then the lock
        ///         is released.  If the current thread is not the holder of this
        ///         lock then <see cref="SynchronizationLockException" /> is thrown.
        ///     </p>
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key to lock.</param>
        /// <exception cref="SynchronizationLockException">if the current thread does not hold this lock</exception>
        void Unlock(TKey key);

        /// <summary>Releases the lock for the specified key regardless of the lock owner.</summary>
        /// <remarks>
        ///     Releases the lock for the specified key regardless of the lock owner.
        ///     It always successfully unlocks the key, never blocks
        ///     and returns immediately.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key to lock.</param>
        void ForceUnlock(TKey key);

        /// <summary>
        ///     Returns a
        ///     <see cref="ICollection{E}" />
        ///     clone of the mappings contained in this map.
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </summary>
        /// <returns>a <see cref="ISet{E}" /> clone of the keys mappings in this map</returns>
        ISet<KeyValuePair<TKey, TValue>> EntrySet();

        /// <summary>
        ///     Queries the map based on the specified predicate and
        ///     returns the matching entries.
        /// </summary>
        /// <remarks>
        ///     Queries the map based on the specified predicate and
        ///     returns the matching entries.
        ///     <p />
        ///     Specified predicate runs on all members in parallel.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <param name="predicate">query criteria</param>
        /// <returns>result entry <see cref="ISet{E}" /> of the query</returns>
        ISet<KeyValuePair<TKey, TValue>> EntrySet(IPredicate<TKey, TValue> predicate);

        /// <summary>Returns a set clone of the keys contained in this map.</summary>
        /// <remarks>
        ///     Returns a set clone of the keys contained in this map.
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <returns>a <see cref="ISet{E}" /> clone of the keys contained in this map</returns>
        ISet<TKey> KeySet();

        /// <summary>
        ///     Queries the map based on the specified predicate and
        ///     returns the keys of matching entries.
        /// </summary>
        /// <remarks>
        ///     Queries the map based on the specified predicate and
        ///     returns the keys of matching entries.
        ///     <p />
        ///     Specified predicate runs on all members in parallel.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <param name="predicate">query criteria</param>
        /// <returns>result key <see cref="ISet{E}" /> of the query</returns>
        ISet<TKey> KeySet(IPredicate<TKey, TValue> predicate);

        /// <summary>Returns a collection clone of the values contained in this map.</summary>
        /// <remarks>
        ///     Returns a collection clone of the values contained in this map.
        ///     The collection is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </remarks>
        /// <returns>a collection clone of the values contained in this map</returns>
        ICollection<TValue> Values();

        /// <summary>
        ///     Queries the map based on the specified predicate and
        ///     returns the values of matching entries.
        /// </summary>
        /// <remarks>
        ///     Queries the map based on the specified predicate and
        ///     returns the values of matching entries.
        ///     <p />
        ///     Specified predicate runs on all members in parallel.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     The collection is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </remarks>
        /// <param name="predicate">query criteria</param>
        /// <returns>result value collection of the query</returns>
        ICollection<TValue> Values(IPredicate<TKey, TValue> predicate);

        /// <summary>Returns the <c>EntryView</c> for the specified key.</summary>
        /// <remarks>
        ///     Returns the <c>EntryView</c> for the specified key.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns a clone of original mapping, modifying the returned value does not change
        ///         the actual value in the map. One should put modified value back to make changes visible to all nodes.
        ///     </p>
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key of the entry</param>
        /// <returns><c>EntryView</c> of the specified key</returns>
        /// <seealso cref="IEntryView{K, V}" />
        IEntryView<TKey, TValue> GetEntryView(TKey key);

        /// <summary>
        ///     Removes all of the mappings from this map (optional operation).
        /// </summary>
        /// <remarks>
        ///     The map will be empty after this call returns.
        /// </remarks>
        void Clear();

        /// <summary>Returns <c>true</c> if this map contains no entries.</summary>
        /// <remarks>Returns <c>true</c> if this map contains no entries.</remarks>
        /// <returns><c>true</c> if this map contains no entries</returns>
        bool IsEmpty();

        /// <summary>Returns the number of entries in this map.</summary>
        /// <remarks>Returns the number of entries in this map.</remarks>
        /// <returns>the number of entries in this map</returns>
        int Size();

        /// <summary>
        ///     If this map has a MapStore this method flushes
        ///     all the local dirty entries by calling <c>MapStore.storeAll()</c> and/or <c>MapStore.deleteAll()</c>
        /// </summary>
        void Flush();

        /// <summary>Evicts the specified key from this map.</summary>
        /// <remarks>
        ///     Evicts the specified key from this map. If
        ///     a <c>MapStore</c> defined for this map, then the entry is not
        ///     deleted from the underlying <c>MapStore</c>, evict only removes
        ///     the entry from the memory.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key to evict</param>
        /// <returns><c>true</c> if the key is evicted, <c>false</c> otherwise.</returns>
        bool Evict(TKey key);

        /// <summary>Evicts all keys from this map except the locked ones.</summary>
        /// <remarks>
        /// Evicts all keys from this map except the locked ones.
        /// <p/>
        /// <see cref="IMap{K, V}.Clear()"/>
        /// method.
        /// <p/>
        /// The EVICT_ALL event is fired for any registered listeners.
        /// </remarks>
        void EvictAll();
    }
}