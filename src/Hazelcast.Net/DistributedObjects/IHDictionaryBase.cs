// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines the base interface for various Hazelcast distributed dictionaries.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <seealso cref="IHDictionary{TKey,TValue}"/>
    /// <seealso cref="IHReplicatedDictionary{TKey,TValue}"/>
    public interface IHDictionaryBase<TKey, TValue> : IDistributedObject, IAsyncEnumerable<KeyValuePair<TKey, TValue>>
    {
        //getting
        /// <summary>
        /// Gets the value for the specified key, or <c>null</c> if this dictionary does not contain this key.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Warning-1:</b><p/>
        /// This method returns a clone of original value, modifying the returned value does not change
        /// the actual value in the dictionary. One should put modified value back to make changes visible to all nodes.
        /// <code>
        /// var value = await dictionary.GetAsync(key);
        /// value.UpdateSomeProperty();
        /// await dictionary.SetAsync(key, value);
        /// </code>
        /// </para>
        /// <para>
        /// <b>Warning-2:</b><p/>
        /// This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        /// the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        /// defined in <c>key</c>'s class.
        /// </para>
        /// <para>
        /// <b>Interactions with the <c>MapStore</c> on server</b><p/>
        /// If value with key is not found in memory
        /// <c>MapLoader.load(Object)</c> is invoked to load the value from
        /// the <c>MapStore</c> backing the dictionary. Exceptions thrown by load fail
        /// the operation and are propagated to the caller.
        /// </para>
        /// </remarks>
        /// <param name="key">key</param>
        /// <returns>value for the specified key or <c>null</c> if this dictionary does not contain this key</returns>
        Task<TValue> GetAsync(TKey key);

        /// <summary>Gets a <see cref="IReadOnlyCollection{TKey}" /> clone of the keys contained in this dictionary.</summary>
        /// <remarks>
        /// <para>
        /// The returned collection is <b>NOT</b> backed by this dictionary,
        /// so changes to the dictionary are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </para>
        /// </remarks>
        /// <returns>a <see cref="IReadOnlyCollection{TKey}" /> clone of the keys contained in this dictionary</returns>
        Task<IReadOnlyCollection<TKey>> GetKeysAsync();

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{TValue}" /> clone of the values contained in this dictionary.</summary>
        /// <remarks>
        /// The collection is <b>NOT</b> backed by the dictionary,
        /// so changes to the dictionary are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </remarks>
        /// <returns>a collection clone of the values contained in this dictionary</returns>
        Task<IReadOnlyCollection<TValue>> GetValuesAsync();

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{TKey, TValue}" /> clone of the entries contained in this dictionary.
        /// </summary>
        /// <remarks>
        /// The returned readonly dictionary is <b>NOT</b> backed by the hazelcast dictionary,
        /// so changes to the returned readonly dictionary are <b>NOT</b> reflected in the <see cref="IHDictionaryBase{TKey,TValue}"/>,
        /// and vice-versa.
        /// </remarks>
        /// <returns>a <see cref="IReadOnlyDictionary{TKey, TValue}" /> clone of the <see cref="IHDictionaryBase{TKey,TValue}"/></returns>
        Task<IReadOnlyDictionary<TKey, TValue>> GetEntriesAsync();

        /// <summary>Gets the number of entries contained in this dictionary.</summary>
        /// <returns>the number of entries in this dictionary</returns>
        Task<int> CountAsync();
        
        /// <summary>Returns <c>true</c> if this dictionary contains no entries.</summary>
        /// <returns><c>true</c> if this dictionary contains no entries</returns>
        Task<bool> IsEmptyAsync();
        
        /// <summary>
        /// Determines whether this dictionary contains an entry for the specified key.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Warning:</b>
        /// This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        /// the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        /// defined in <c>key</c>'s class.
        /// </para>
        /// </remarks>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key contains key; otherwise, <c>false</c>.</returns>
        Task<bool> ContainsKeyAsync(TKey key);
        
        /// <summary>
        /// Determines whether this dictionary contains one or more keys to the specified value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns <c>true</c> if this dictionary contains one or more keys to the specified value</returns>
        Task<bool> ContainsValueAsync(TValue value);

        //setting
        /// <summary>
        /// Associates a given value to the specified key and replicates it to the
        /// cluster. If there is an old value, it will be replaced by the specified
        /// one and returned from the call.
        /// </summary>
        /// <param name="key">key with which the specified value is to be associated.</param>
        /// <param name="value">value to be associated with the specified key.</param>
        /// <returns>old value of the entry</returns>
        Task<TValue> GetAndSetAsync(TKey key, TValue value);
        
        /// <summary>
        /// Associates a given value to the specified key and replicates it to the
        /// cluster. If there is an old value, it will be replaced by the specified
        /// one and returned from the call.
        /// </summary>
        /// <remarks>
        /// <para>
        /// In addition, you have to specify a <see cref="timeToLive"/> 
        /// to define when the value is outdated and thus should be removed from the
        /// this dictionary.
        /// </para>
        /// </remarks>
        /// <param name="key">key with which the specified value is to be associated.</param>
        /// <param name="value">value to be associated with the specified key.</param>
        /// <param name="timeToLive">A positive time to live period to be associated with the specified key-value pair.</param>
        /// <returns>old value of the entry</returns>
        Task<TValue> GetAndSetAsync(TKey key, TValue value, TimeSpan timeToLive);
        
        /// <summary>
        /// Copies all of the entries from the specified <see cref="IHDictionary{TKey,TValue}"/> to this dictionary.
        /// </summary>
        /// <param name="entries">entries to be stored in this dictionary</param>
        Task SetAllAsync(IDictionary<TKey, TValue> entries);

        //removing
        /// <summary>
        /// Clears the dictionary and deletes the items.
        /// </summary>
        Task ClearAsync();
        
        /// <summary>
        /// Removes the entry for a key from this dictionary if it is present.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The dictionary will not contain a mapping for the specified key once the call returns.
        /// </para>
        /// </remarks>
        /// <param name="key">key</param>
        /// <returns>
        /// previous value associated with <c>key</c> or <c>null</c>
        /// if there was no mapping for <c>key</c> .
        /// </returns>
        Task<TValue> GetAndRemoveAsync(TKey key);
    }
}