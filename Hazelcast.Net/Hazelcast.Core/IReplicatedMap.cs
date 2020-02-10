// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    /// A ReplicatedMap is a map-like data structure with weak consistency and values locally stored on every node of the cluster. 
    /// </summary>
    /// <remarks>
    /// <p>Whenever a value is written asynchronously, the new value will be internally
    /// distributed to all existing cluster members, and eventually every node will have
    /// the new value.</p>
    /// <p>When a new node joins the cluster, the new node initially will request existing
    ///  values from older nodes and replicate them locally.</p>
    /// </remarks>
    /// <typeparam name="TKey">the type of keys maintained by this map</typeparam>
    /// <typeparam name="TValue">the type of mapped values</typeparam>
    public interface IReplicatedMap<TKey, TValue> : IDistributedObject
    {
        /// <summary>
        ///     Adds an entry listener for this map.
        /// </summary>
        /// <remarks>
        ///     Adds an entry listener for this map. Listener will get notified for all map Add/remove/update/evict events.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <returns>returns registration id</returns>
        Guid AddEntryListener(IEntryListener<TKey, TValue> listener);

        /// <summary>Adds the specified entry listener for the specified key.</summary>
        /// <remarks>
        ///     Adds the specified entry listener for the specified key.
        ///     The listener will get notified for all
        ///     Add/remove/update/evict events of the specified key only.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <param name="key">key to listen</param>
        /// <returns>returns registration id</returns>
        Guid AddEntryListener(IEntryListener<TKey, TValue> listener, TKey key);

        /// <summary>Adds an continuous entry listener for this map.</summary>
        /// <remarks>
        ///     Adds a continuous entry listener for this map. Listener will get notified
        ///     for map Add/remove/update/evict events filtered by given predicate.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <param name="predicate">predicate for filtering entries</param>
        /// <returns>returns registration id</returns>
        Guid AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate);

        /// <summary>Adds an continuous entry listener for this map.</summary>
        /// <remarks>
        ///     Adds a continuous entry listener for this map. Listener will get notified
        ///     for map Add/remove/update/evict events filtered by given predicate.
        /// </remarks>
        /// <param name="listener">entry listener</param>
        /// <param name="predicate">predicate for filtering entries</param>
        /// <param name="key">key to listen</param>
        /// <returns>returns registration id</returns>
        Guid AddEntryListener(IEntryListener<TKey, TValue> listener, IPredicate predicate, TKey key);

        /// <summary>
        /// The clear operation wipes data out of the replicated maps.
        /// </summary>
        /// <remarks>
        /// If some node fails on executing the operation, it is retried for at most 
        /// 5 times (on the failing nodes only).
        ///</remarks>
        void Clear();
        
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
        ///         This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///         the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
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

        /// <summary>Returns <c>true</c> if this map contains no entries.</summary>
        /// <returns><c>true</c> if this map contains no entries</returns>
        bool IsEmpty();

        /// <summary>
        /// Returns a <see cref="ISet{E}" /> clone of the mappings contained in this map.
        /// </summary>
        /// <remarks>
        ///     Returns a
        ///     <see cref="ISet{E}" />
        ///     clone of the mappings contained in this map.
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <returns>a set clone of the keys mappings in this map</returns>
        ISet<KeyValuePair<TKey, TValue>> EntrySet();

        /// <summary>Returns a set clone of the keys contained in this map.</summary>
        /// <remarks>
        ///     Returns a set clone of the keys contained in this map.
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <returns>a <see cref="ISet{E}" /> clone of the keys contained in this map</returns>
        ISet<TKey> KeySet();

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

        /// <summary>
        /// Associates a given value to the specified key and replicates it to the 
        /// cluster. If there is an old value, it will be replaced by the specified 
        /// one and returned from the call.
        /// </summary>
        /// <remarks>
        /// In addition, you have to specify a ttl and its <see cref="TimeUnit" /> 
        /// to define when the value is outdated and thus should be removed from the
        /// replicated map.
        /// </remarks>
        /// <param name="key">key with which the specified value is to be associated.</param>
        /// <param name="value">value to be associated with the specified key.</param>
        /// <param name="ttl">ttl to be associated with the specified key-value pair.</param>
        /// <param name="timeunit"><see cref="TimeUnit" /> to be used for the ttl value.</param>
        /// <returns>old value of the entry</returns>
        TValue Put(TKey key, TValue value, long ttl, TimeUnit timeunit);

        /// <summary>
        ///     Copies all of the mappings from the specified map to this map
        /// </summary>
        /// <param name="m">mappings to be stored in this map</param>
        void PutAll(IDictionary<TKey, TValue> m);

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

        /// <summary>
        /// Removes the specified entry listener.
        /// </summary>
        /// <remarks>
        /// Returns false if there was no such listener added before.
        /// </remarks>
        /// <param name="id">ID of the registered entry listener.</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveEntryListener(Guid id);

        /// <summary>Returns the number of entries in this map.</summary>
        /// <remarks>Returns the number of entries in this map.</remarks>
        /// <returns>the number of entries in this map</returns>
        int Size();

        /// <summary>Returns a collection clone of the values contained in this map.</summary>
        /// <remarks>
        ///     Returns a collection clone of the values contained in this map.
        ///     The collection is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </remarks>
        /// <returns>a collection clone of the values contained in this map</returns>
        ICollection<TValue> Values();
    }
}