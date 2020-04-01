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

using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>A specialized Concurrent, distributed map whose keys can be associated with multiple values.</summary>
    public interface IMultiMap<TKey, TValue> : IDistributedObject
    {
        /// <summary>Adds an entry listener for this multimap.</summary>
        /// <param name="listener">entry listener</param>
        /// <param name="includeValue">
        ///     <c>true</c> if <c>EntryEvent</c> should
        ///     contain the value.
        /// </param>
        /// <returns>returns registration id.</returns>
        string AddEntryListener(IEntryListener<TKey, TValue> listener, bool includeValue);

        /// <summary>Adds the specified entry listener for the specified key.</summary>
        /// <param name="listener">entry listener</param>
        /// <param name="key">the key to listen</param>
        /// <param name="includeValue">
        ///     <c>true</c> if <c>EntryEvent</c> should
        ///     contain the value.
        /// </param>
        /// <returns>returns registration id.</returns>
        string AddEntryListener(IEntryListener<TKey, TValue> listener, TKey key, bool includeValue);

        /// <summary>Clears the multimap.</summary>
        /// <remarks>Clears the multimap. Removes all key-value pairs.</remarks>
        void Clear();

        /// <summary>Returns whether the multimap contains the given key-value pair.</summary>
        /// <param name="key">the key whose existence is checked.</param>
        /// <param name="value">the value whose existence is checked.</param>
        /// <returns>true if the multimap contains the key-value pair, false otherwise.</returns>
        bool ContainsEntry(TKey key, TValue value);

        /// <summary>Returns whether the multimap contains an entry with the key.</summary>
        /// <param name="key">the key whose existence is checked.</param>
        /// <returns>true if the multimap contains an entry with the key, false otherwise.</returns>
        bool ContainsKey(TKey key);

        /// <summary>Returns whether the multimap contains an entry with the value.</summary>
        /// <param name="value">the value whose existence is checked.</param>
        /// <returns>true if the multimap contains an entry with the value, false otherwise.</returns>
        bool ContainsValue(object value);

        /// <summary>Returns the set of key-value pairs in the multimap.</summary>
        /// <returns>
        ///     the set of key-value pairs in the multimap. Returned set might be modifiable
        ///     but it has no effect on the multimap
        /// </returns>
        ISet<KeyValuePair<TKey, TValue>> EntrySet();

        /// <summary>Releases the lock for the specified key regardless of the lock owner.</summary>
        /// <param name="key">key to lock.</param>
        void ForceUnlock(TKey key);

        /// <summary>Returns the collection of values associated with the key.</summary>
        /// <param name="key">the key whose associated values are to be returned</param>
        /// <returns>the collection of the values associated with the key.</returns>
        ICollection<TValue> Get(TKey key);

        /// <summary>Checks the lock for the specified key.</summary>
        /// <param name="key">key to lock to be checked.</param>
        /// <returns><c>true</c> if lock is acquired, <c>false</c> otherwise.</returns>
        bool IsLocked(TKey key);

        /// <summary>Returns the set of keys in the multimap.</summary>
        /// <returns>
        ///     the set of keys in the multimap. Returned set might be modifiable
        ///     but it has no effect on the multimap
        /// </returns>
        ISet<TKey> KeySet();

        /// <summary>Acquires the lock for the specified key.</summary>
        /// <param name="key">key to lock.</param>
        void Lock(TKey key);

        /// <summary>Acquires the lock for the specified key for the specified lease time.</summary>
        /// <param name="key">key to lock.</param>
        /// <param name="leaseTime">time to wait before releasing the lock.</param>
        /// <param name="timeUnit">unit of time to specify lease time.</param>
        void Lock(TKey key, long leaseTime, TimeUnit timeUnit);

        /// <summary>Stores a key-value pair in the multimap.</summary>
        /// <param name="key">the key to be stored</param>
        /// <param name="value">the value to be stored</param>
        /// <returns>
        ///     true if size of the multimap is increased, false if the multimap
        ///     already contains the key-value pair.
        /// </returns>
        bool Put(TKey key, TValue value);

        /// <summary>Removes the given key value pair from the multimap.</summary>
        /// <param name="key">the key of the entry to remove</param>
        /// <param name="value">the value of the entry to remove</param>
        /// <returns>true if the size of the multimap changed after the remove operation, false otherwise.</returns>
        bool Remove(object key, object value);

        /// <summary>Removes all the entries with the given key.</summary>
        /// <param name="key">the key of the entries to remove</param>
        /// <returns>
        ///     the collection of removed values associated with the given key. Returned collection
        ///     might be modifiable but it has no effect on the multimap
        /// </returns>
        ICollection<TValue> Remove(object key);

        /// <summary>
        ///     Removes the specified entry listener
        ///     Returns silently if there is no such listener added before.
        /// </summary>
        /// <param name="registrationId">Id of listener registration</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveEntryListener(string registrationId);

        /// <summary>Returns the number of key-value pairs in the multimap.</summary>
        /// <returns>the number of key-value pairs in the multimap.</returns>
        int Size();

        /// <summary>Tries to acquire the lock for the specified key.</summary>
        /// <param name="key">key to lock.</param>
        /// <returns><c>true</c> if lock is acquired, <c>false</c> otherwise.</returns>
        bool TryLock(TKey key);

        /// <summary>Tries to acquire the lock for the specified key.</summary>
        /// <param name="key">the key to lock</param>
        /// <param name="time">the maximum time to wait for the lock</param>
        /// <param name="timeunit">the time unit of the <c>time</c> argument.</param>
        /// <returns>
        ///     <c>true</c> if the lock was acquired and <c>false</c>
        ///     if the waiting time elapsed before the lock was acquired.
        /// </returns>
        /// <exception cref="System.Exception"></exception>
        bool TryLock(TKey key, long time, TimeUnit timeunit);

        /// <summary>Tries to acquire the lock for the specified key for the specified lease time.</summary>
        /// <remarks>
        /// Tries to acquire the lock for the specified key for the specified lease time.
        /// <p>After lease time, the lock will be released.
        /// <p/>
        /// <p>If the lock is not available, then
        /// the current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until one of two things happens:</p>
        /// <ul>
        /// <li>the lock is acquired by the current thread, or</li>
        /// <li>the specified waiting time elapses.</li>
        /// </ul>
        /// </p>
        /// <p/>
        /// <p><b>Warning:</b></p>
        /// This method uses <tt>hashCode</tt> and <tt>equals</tt> of the binary form of
        /// the <tt>key</tt>, not the actual implementations of <tt>hashCode</tt> and <tt>equals</tt>
        /// defined in the <tt>key</tt>'s class.
        /// </remarks>
        /// <param name="key">key to lock in this map.</param>
        /// <param name="time">maximum time to wait for the lock.</param>
        /// <param name="timeunit">time unit of the <tt>time</tt> argument.</param>
        /// <param name="leaseTime">time to wait before releasing the lock.</param>
        /// <param name="leaseTimeunit">unit of time to specify lease time.</param>
        /// <returns>
        /// <tt>true</tt> if the lock was acquired and <tt>false</tt>
        /// if the waiting time elapsed before the lock was acquired.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">if the specified key is null.</exception>
        /// <exception cref="System.Exception"/>
        bool TryLock(TKey key, long time, TimeUnit timeunit, long leaseTime, TimeUnit leaseTimeunit);

        /// <summary>Releases the lock for the specified key.</summary>
        /// <param name="key">key to lock.</param>
        void Unlock(TKey key);

        /// <summary>Returns number of values matching to given key in the multimap.</summary>
        /// <param name="key">the key whose values count are to be returned</param>
        /// <returns>number of values matching to given key in the multimap.</returns>
        int ValueCount(TKey key);

        /// <summary>Returns the collection of values in the multimap.</summary>
        /// <returns>
        ///     the collection of values in the multimap. Returned collection might be modifiable
        ///     but it has no effect on the multimap
        /// </returns>
        ICollection<TValue> Values();
    }
}