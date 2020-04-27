using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregators;
using Hazelcast.Configuration;
using Hazelcast.Data.Map;
using Hazelcast.Predicates;
using Hazelcast.Projections;

// TODO: remove this directives - only during dev
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a distributed map.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <remarks>
    /// <para>Keys are identified by their own hash code and equality.</para>
    /// <para>Methods return clones of the original keys and values. Modifying these clones does not change
    /// the actual keys and values in the map. One should put the modified entries back, to make changes visible
    /// to all nodes.</para>
    /// <para>All asynchronous methods return a task that will complete when they are done, and represent
    /// the value which is documented on each method. When documenting each method, we do not repeat "a task
    /// that will complete..." but this is assumed.</para>
    /// </remarks>
    public interface IMap<TKey, TValue> : IDistributedObject
    {
        // NOTES
        //
        // In most cases it would be pointless to return async enumerable since we must fetch
        // everything from the network anyways (else we'd hang the socket) before returning,
        // and therefore all that remains is CPU-bound de-serialization of data.

        #region Setting

        /// <summary>
        /// Adds or replaces an entry and returns the previous value.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value);

        /// <summary>
        /// Adds or replaces an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>Nothing.</returns>
        Task AddOrReplaceAsync(TKey key, TValue value);

        /// <summary>
        /// Adds or replaces an entry with a time-to-live and returns the previous value.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Adds or replaces an entry with a time-to-live.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        Task AddOrReplaceAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Adds or replaces entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        /// <returns>Nothing.</returns>
        /// TODO: is this transactional?
        Task AddOrReplace(IDictionary<TKey, TValue> entries);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        Task<TValue> ReplaceAsync(TKey key, TValue newValue);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue);

        /// <summary>
        /// Tries to set an entry within a timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was set; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// </remarks>
        Task<bool> TryAddOrReplaceAsync(TKey key, TValue value, TimeSpan timeout);

        /// <summary>
        /// Adds an entry, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        Task<TValue> AddIfMissingAsync(TKey key, TValue value);

        /// <summary>
        /// Adds an entry with a time-to-live, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        Task<TValue> AddIfMissingAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Adds an entry, or its <see cref="MapStore"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <remarks>
        /// <para>If the map has a <see cref="MapStore"/> attached, the entry is added to the store
        /// but not persisted. Flushing the store (see <see cref="Flush"/>) is required to make sure
        /// that the entry is actually persisted to the map.</para>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// TODO: is it really removed? or just evicted?
        /// </remarks>
        Task AddTransientAsync(TKey key, TValue value, TimeSpan timeToLive);

        #endregion

        #region Getting

        /// <summary>
        /// Gets the value for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value for the specified key, or null if the map does not contain an entry with this key.</returns>
        Task<TValue> GetAsync(TKey key);

        /// <summary>
        /// Gets all entries for keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>The values for the specified keys.</returns>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(ICollection<TKey> keys);

        /// <summary>
        /// Gets an entry for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>An entry for the specified key, or null if the map does not contain an entry with this key.</returns>
        Task<IMapEntry<TKey, TValue>> GetEntryAsync(TKey key);

        /// <summary>
        /// Queries entries.
        /// </summary>
        /// <param name="predicate">A predicate to filter the entries with.</param>
        /// <returns>Entries matching the <paramref name="predicate"/>, or all entries.</returns>
        /// <remarks>
        /// <para>The result it *not* backed by the map, so changes to the map are not
        /// reflected, and vice-versa.</para>
        /// <para>The <paramref name="predicate"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(IPredicate predicate = null);

        /// <summary>
        /// Gets keys.
        /// </summary>
        /// <param name="predicate">An optional predicate to filter the entries with.</param>
        /// <returns>All keys.</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate = null);

        /// <summary>
        /// Gets all values for entries matching a predicate.
        /// </summary>
        /// <param name="predicate">An optional predicate to filter the entries.</param>
        /// <returns>All values.</returns>
        Task<IReadOnlyList<TValue>> GetValuesAsync(IPredicate predicate = null);

        /// <summary>
        /// Gets the number of entries in the map.
        /// </summary>
        /// <returns>The total number of entries in the map.</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Determines whether this map is empty.
        /// </summary>
        /// <returns>true if the map does not contain entries; otherwise false.</returns>
        Task<bool> IsEmptyAsync();

        /// <summary>
        /// Determines whether this map contains an entry for a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if the map contains an entry for the specified key; otherwise false.</returns>
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>
        /// Determines whether this map contains at least one entry with a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if the map contains at least an entry with the specified value; otherwise false.</returns>
        Task<bool> ContainsValueAsync(TValue value);

        #endregion

        #region Removing

        /// <summary>
        /// Tries to remove an entry from the map within a timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was removed; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// TODO or when there was no value with that key?
        /// </remarks>
        Task<bool> TryRemoveAsync(TKey key, TimeSpan timeout);

        /// <summary>
        /// Removes an entry from this map, and returns the corresponding value if any.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        /// <remarks>
        /// <para>This method serializes the return value. For performance reasons, prefer
        /// <see cref="DeleteAsync"/> when the returned value is not used.</para>
        /// </remarks>
        Task<TValue> RemoveAsync(TKey key);

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        /// <remarks>
        /// <para>This method removes an entry if the key and the value both match the
        /// specified key and value.</para>
        /// </remarks>
        Task<bool> RemoveAsync(TKey key, TValue value);

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <remarks>
        /// <para>For performance reasons, this method does not return the value. Prefer
        /// <see cref="RemoveAsync(TKey)"/> if the value is required.</para>
        /// </remarks>
        Task DeleteAsync(TKey key); // FIXME rename 

        /// <summary>
        /// Empties this map.
        /// </summary>
        Task ClearAsync();

        #endregion

        #region Caching

        /// <summary>
        /// Evicts an entry from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the entry was evicted; otherwise false.</returns>
        /// <remarks>
        /// <para>Locked entries are not evicted (TODO: true?)</para>
        /// <para>Evicts the entry from the in-memory cache. The entry is not removed from
        /// the map. If a <see cref="MapStore"/> is defined for this map, The entry is
        /// not evicted from the map store.</para>
        /// </remarks>
        Task<bool> EvictAsync(TKey key);

        /// <summary>
        /// Evicts all entries but the locked entries from the cache.
        /// </summary>
        /// <remarks>
        /// <para>Locked entries are not evicted.</para>
        /// <para>Evicts entries from the in-memory cache. Entries are not removed from
        /// the map. If a <see cref="MapStore"/> is defined for this map, entries are
        /// not evicted from the map store.</para>
        /// </remarks>
        Task EvictAllAsync();

        /// <summary>
        /// Flushes the map store, if any.
        /// </summary>
        /// <remarks>
        /// <para>If a <see cref="MapStore"/> is defined for this map, this method flushes
        /// all dirty entries by deleting or storing them.</para>
        /// </remarks>
        Task FlushAsync();

        #endregion

        #region Processing

        /// <summary>
        /// Processes an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="processor">An entry processor.</param>
        /// <returns>The result of the process.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<object> ExecuteAsync(IEntryProcessor processor, TKey key);

        /// <summary>
        /// Processes entries.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="processor">An entry processor.</param>
        /// <returns>The result of the processing of each entry.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, IEnumerable<TKey> keys);

        /// <summary>
        /// Process all entries.
        /// </summary>
        /// <param name="processor">An entry processor.</param>
        /// <returns>The result of the processing of all entries.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor);

        /// <summary>
        /// TODO: kill that one, it seems to be ExecuteOnKeyAsync? what is this? updating the value?
        /// </summary>
        /// <param name="key"></param>
        /// <param name="processor"></param>
        /// <returns></returns>
        Task<object> ApplyAsync(IEntryProcessor processor, TKey key);

        #endregion

        #region Locking

        /// <summary>
        /// Locks an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <remarks>
        /// TODO: document this properly (also: not distributed lock?)
        /// <para>If the lock is not available, then "the current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until the lock has been acquired." The scope of the lock is this map only,
        /// and the lock is only for the specified key in this map.</para>
        /// <para>Locks are re-entrant, but counted: if a key is locked N times, then it should be unlocked
        /// N times before another thread can lock it.</para>
        /// </remarks>
        Task LockAsync(TKey key);

        /// <summary>
        /// Locks an entry for a given time span.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="leaseTime">A time span.</param>
        /// <remarks>
        /// TODO: document this properly (also: not distributed lock?)
        /// <para>If the lock is not available, then "the current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until the lock has been acquired." The scope of the lock is this map only,
        /// and the lock is only for the specified key in this map.</para>
        /// <para>Locks are re-entrant, but counted: if a key is locked N times, then it should be unlocked
        /// N times before another thread can lock it.</para>
        /// <para>The lock is released after the time span.</para>
        /// </remarks>
        Task LockAsync(TKey key, TimeSpan leaseTime);

        /// <summary>
        /// Tries to lock an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>true if the lock was acquired; otherwise false.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked, returns false immediately.</para>
        /// </remarks>
        Task<bool> TryLockAsync(TKey key);

        /// <summary>
        /// Tries to lock an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the lock was acquired; otherwise false.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked after <paramref name="timeout"/>, returns false.</para>
        /// <para>If <paramref name="timeout"/> is <see cref="Timeout.InfiniteTimeSpan"/>, waits forever.</para>
        /// </remarks>
        Task<bool> TryLockAsync(TKey key, TimeSpan timeout);

        /// <summary>
        /// Tries to lock an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <param name="leaseTime">A lease time.</param>
        /// <returns>true if the lock was acquired; otherwise false.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked after <paramref name="timeout"/>, returns false.
        /// If <paramref name="timeout"/> is <see cref="Timeout.InfiniteTimeSpan"/>, waits forever.</para>
        /// <para>If acquired, the lock is automatically released after <paramref cref="leaseTime"/>.
        /// If <paramref name="leaseTime"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the lock is never
        /// released.</para>
        /// </remarks>
        Task<bool> TryLockAsync(TKey key, TimeSpan timeout, TimeSpan leaseTime);

        /// <summary>
        /// Determines whether an entry is locked.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <returns>true if the entry is locked; otherwise false.</returns>
        Task<bool> IsLockedAsync(TKey key);

        /// <summary>
        /// Unlocks an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A task that will complete when the entry has been unlocked.</returns>
        Task UnlockAsync(TKey key);

        /// <summary>
        /// Unlocks an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <remarks>
        /// <para>Unlocks the entry identified by the key, regardless of the lock owner.</para>
        /// <para>This always succeed, never blocks, and returns immediately.</para>
        /// TODO: but, async?
        /// </remarks>
        Task ForceUnlockAsync(TKey key);

        #endregion

        #region Indexing

        // TODO what is this?

        Task AddIndexAsync(IndexType type, params string[] attributes);
        Task AddIndexAsync(IndexConfig indexConfig);

        #endregion

        #region Aggregating and Projecting

        /// <summary>
        /// Aggregates values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregator">The aggregator.</param>
        /// <param name="predicate">An optional predicate to filter the entries with.</param>
        /// <returns>The result of the aggregation.</returns>
        /// <remarks>
        /// <para>The <paramref name="aggregator"/> and <paramref name="predicate"/> must be
        /// serializable via Hazelcast serialization, and have a counterpart on the server.</para>
        /// </remarks>
        Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, IPredicate predicate = null);

        /// <summary>
        /// Projects values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="predicate">An optional predicate to filter the entries with.</param>
        /// <returns>The projected values.</returns>
        /// <remarks>
        /// <para>The <paramref name="projection"/> and <paramref name="predicate"/> must be
        /// serializable via Hazelcast serialization, and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, IPredicate predicate = null);

        #endregion

        #region Interception

        // TODO what is an interceptor?

        /// <summary>
        /// Adds an interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns>The interceptor unique identifier.</returns>
        Task<string> AddInterceptorAsync(IMapInterceptor interceptor);

        /// <summary>
        /// Removes an interceptor.
        /// </summary>
        /// <param name="id">The identifier of the interceptor.</param>
        Task RemoveInterceptorAsync(string id);

        #endregion

        #region Events

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(bool includeValues, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="key">A key to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(TKey key, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(bool includeValues, TKey key, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(IPredicate predicate, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="key">A key to filter events.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(TKey key, IPredicate predicate, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(bool includeValues, TKey key, IPredicate predicate, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>?</returns>
        /// TODO: what is returned? how could this fail?
        Task<bool> UnsubscribeAsync(Guid subscriptionId);

        #endregion
    }
}
