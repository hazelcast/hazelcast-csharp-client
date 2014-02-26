using System.Collections.Generic;
using Hazelcast.Net.Ext;

namespace Hazelcast.Core
{
    /// <summary>A specialized map whose keys can be associated with multiple values.</summary>
    public interface IMultiMap<K, V> : IDistributedObject
    {
        //string GetName();
        /// <summary>Returns the name of this multimap.</summary>
        /// <remarks>Returns the name of this multimap.</remarks>
        /// <returns>the name of this multimap</returns>
        /// <summary>Stores a key-value pair in the multimap.</summary>
        /// <param name="key">the key to be stored</param>
        /// <param name="value">the value to be stored</param>
        /// <returns>
        ///     true if size of the multimap is increased, false if the multimap
        ///     already contains the key-value pair.
        /// </returns>
        bool Put(K key, V value);

        /// <summary>Returns the collection of values associated with the key.</summary>
        /// <param name="key">the key whose associated values are to be returned</param>
        /// <returns>the collection of the values associated with the key.</returns>
        ICollection<V> Get(K key);

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
        ICollection<V> Remove(object key);

        /// <summary>Returns the set of keys in the multimap.</summary>
        /// <returns>
        ///     the set of keys in the multimap. Returned set might be modifiable
        ///     but it has no effect on the multimap
        /// </returns>
        ICollection<K> KeySet();

        /// <summary>Returns the collection of values in the multimap.</summary>
        /// <returns>
        ///     the collection of values in the multimap. Returned collection might be modifiable
        ///     but it has no effect on the multimap
        /// </returns>
        ICollection<V> Values();

        /// <summary>Returns the set of key-value pairs in the multimap.</summary>
        /// <returns>
        ///     the set of key-value pairs in the multimap. Returned set might be modifiable
        ///     but it has no effect on the multimap
        /// </returns>
        ICollection<KeyValuePair<K, V>> EntrySet();

        /// <summary>Returns whether the multimap contains an entry with the key.</summary>
        /// <param name="key">the key whose existence is checked.</param>
        /// <returns>true if the multimap contains an entry with the key, false otherwise.</returns>
        bool ContainsKey(K key);

        /// <summary>Returns whether the multimap contains an entry with the value.</summary>
        /// <param name="value">the value whose existence is checked.</param>
        /// <returns>true if the multimap contains an entry with the value, false otherwise.</returns>
        bool ContainsValue(object value);

        /// <summary>Returns whether the multimap contains the given key-value pair.</summary>
        /// <param name="key">the key whose existence is checked.</param>
        /// <param name="value">the value whose existence is checked.</param>
        /// <returns>true if the multimap contains the key-value pair, false otherwise.</returns>
        bool ContainsEntry(K key, V value);

        /// <summary>Returns the number of key-value pairs in the multimap.</summary>
        /// <returns>the number of key-value pairs in the multimap.</returns>
        int Size();

        /// <summary>Clears the multimap.</summary>
        /// <remarks>Clears the multimap. Removes all key-value pairs.</remarks>
        void Clear();

        /// <summary>Returns number of values matching to given key in the multimap.</summary>
        /// <param name="key">the key whose values count are to be returned</param>
        /// <returns>number of values matching to given key in the multimap.</returns>
        int ValueCount(K key);

        /// <summary>Adds an entry listener for this multimap.</summary>
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
        /// <param name="registrationId">Id of listener registration</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveEntryListener(string registrationId);

        /// <summary>Adds the specified entry listener for the specified key.</summary>
        /// <param name="listener">entry listener</param>
        /// <param name="key">the key to listen</param>
        /// <param name="includeValue">
        ///     <tt>true</tt> if <tt>EntryEvent</tt> should
        ///     contain the value.
        /// </param>
        /// <returns>returns registration id.</returns>
        string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue);

        /// <summary>Acquires the lock for the specified key.</summary>
        /// <param name="key">key to lock.</param>
        void Lock(K key);

        /// <summary>Acquires the lock for the specified key for the specified lease time.</summary>
        /// <param name="key">key to lock.</param>
        /// <param name="leaseTime">time to wait before releasing the lock.</param>
        /// <param name="timeUnit">unit of time to specify lease time.</param>
        void Lock(K key, long leaseTime, TimeUnit timeUnit);

        /// <summary>Checks the lock for the specified key.</summary>
        /// <param name="key">key to lock to be checked.</param>
        /// <returns><tt>true</tt> if lock is acquired, <tt>false</tt> otherwise.</returns>
        bool IsLocked(K key);

        /// <summary>Tries to acquire the lock for the specified key.</summary>
        /// <param name="key">key to lock.</param>
        /// <returns><tt>true</tt> if lock is acquired, <tt>false</tt> otherwise.</returns>
        bool TryLock(K key);

        /// <summary>Tries to acquire the lock for the specified key.</summary>
        /// <param name="time">the maximum time to wait for the lock</param>
        /// <param name="timeunit">the time unit of the <tt>time</tt> argument.</param>
        /// <returns>
        ///     <tt>true</tt> if the lock was acquired and <tt>false</tt>
        ///     if the waiting time elapsed before the lock was acquired.
        /// </returns>
        /// <exception cref="System.Exception"></exception>
        bool TryLock(K key, long time, TimeUnit timeunit);

        /// <summary>Releases the lock for the specified key.</summary>
        /// <param name="key">key to lock.</param>
        void Unlock(K key);

        /// <summary>Releases the lock for the specified key regardless of the lock owner.</summary>
        /// <param name="key">key to lock.</param>
        void ForceUnlock(K key);
    }
}