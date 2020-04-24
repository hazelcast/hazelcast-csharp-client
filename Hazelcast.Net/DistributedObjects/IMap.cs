using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregators;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Data;
using Hazelcast.Data.Map;
using Hazelcast.DistributedObjects.Implementation;
using Hazelcast.Messaging;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// <para>Keys are identified by their own hash code and equality.</para>
    /// TODO: add para here about returning clones, and not on every method
    /// </remarks>
    public interface IMap<TKey, TValue> : IDistributedObject
    {
        // FIXME
        // careful, all network-related stuff MUST be async here!

        // TODO: every async method returns...
        // "A task that will complete when... and represents...

        #region Setting

        // FIXME these Add were Put and are actually Replace?

        /// <summary>
        /// Adds an entry to the map.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        Task<TValue> AddAsync(TKey key, TValue value);

        /// <summary>
        /// Adds an entry to the map with a time-to-live.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// TODO: is it really removed? or just evicted?
        /// </remarks>
        Task<TValue> AddAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Adds entries to the map.
        /// </summary>
        /// <param name="entries">Entries.</param>
        /// <returns>A task that will complete when all entries have been added to the map.</returns>
        /// TODO: is this transactional?
        Task AddAsync(IDictionary<TKey, TValue> entries);

        /// <summary>
        /// Adds an entry to the map, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <returns>A task that will complete when ...</returns>
        /// TODO: how can it return "the previous value" if we only add when it's missing?
        Task<TValue> AddIfMissingAsync(TKey key, TValue value);

        /// <summary>
        /// Adds an entry to the map with a time-to-live, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>A task that will complete when ...</returns>
        /// TODO: how can it return "the previous value" if we only add when it's missing?
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// TODO: is it really removed? or just evicted?
        /// </remarks>
        Task<TValue> AddIfMissingAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Adds an entry to the map, or its <see cref="MapStore"/>.
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

        // TODO: how is this different from Add?
        Task<TValue> ReplaceAsync(TKey key, TValue value);

        /// <summary>
        /// Replaces an entry in the map.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue);

        // FIXME these Set are actually true Add?
        // but with an IDictionary, can only Add once?! only the [] supports overwriting
        // so we have Add and AddOrReplace, really, or Set and SetAndReplace

        Task SetAsync(TKey key, TValue value);
        Task SetAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Tries to set an entry into the map within a timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was set; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// </remarks>
        // TODO is this Set or SetAndReplace?
        Task<bool> TryPutAsync(TKey key, TValue value, TimeSpan timeout);

        #endregion

        #region Getting

        /// <summary>
        /// Gets the value for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value for the specified key, or null if the map does not contain an entry with this key.</returns>
        /// <remarks>
        /// <para>This methods returns a clone of the original value. Modifying that clone does not change
        /// the actual value in the map. One should put the modified value back, to make changes visible
        /// to all nodes.</para>
        /// </remarks>
        Task<TValue> GetAsync(TKey key);

        /// <summary>
        /// Gets all entries for keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>The values for the specified keys.</returns>
        /// <remarks>
        /// <para>This methods returns clones of the original values. Modifying those clones does not change
        /// the actual values in the map. One should put the modified values back, to make changes visible
        /// to all nodes.</para>
        /// </remarks>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(ICollection<TKey> keys);

        /// <summary>
        /// Gets an entry view for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>An entry view for the specified key, or null if the map does not contain an entry with this key.</returns>
        /// <remarks>
        /// <para>This methods returns a clone of the original value. Modifying that clone does not change
        /// the actual value in the map. One should put the modified value back, to make changes visible
        /// to all nodes.</para>
        /// TODO: what's an entry view?
        /// </remarks>
        Task<IEntryView<TKey, TValue>> GetEntryViewAsync(TKey key);

        // TODO: return an IEnumerable, or an IDictionary (async?)

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

        // TODO: return an IEnumerable? async?
        // TODO: usual remark about returning clones?

        /// <summary>
        /// Gets keys.
        /// </summary>
        /// <param name="predicate">An optional predicate to filter the entries with.</param>
        /// <returns>All keys.</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate = null);

        // TODO: IEnumerable? async?
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
        Task DeleteAsync(TKey key);

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


        // TODO: these should be renamed + done entirely differently
        /*
        Guid AddEntryListener(IMapListener listener, bool includeValue);
        Guid AddEntryListener(IMapListener listener, TKey key, bool includeValue);
        Guid AddEntryListener(IMapListener listener, IPredicate predicate, TKey key, bool includeValue);
        Guid AddEntryListener(IMapListener listener, IPredicate predicate, bool includeValue);
        */

        /// <summary>
        /// Subscribe to events.
        /// </summary>
        /// <param name="includeValues">Whether the event arguments should contain values.</param>
        /// <param name="predicate">An optional predicate to filter entries.</param>
        /// <param name="entryAdded">An optional handler for <see cref="EntryEventType.Added"/> events.</param>
        /// <param name="entryRemoved">An optional handler for <see cref="EntryEventType.Removed"/> events.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        /// <remarks>
        /// <para>By default, event arguments contain values. However, for performance reasons, it
        /// is possible to omit values by specifying <paramref name="includeValues"/> as false.</para>
        /// </remarks>
        Task<Guid> SubscribeAsync(bool includeValues = true, IPredicate predicate = null,
            Action<IMap<TKey, TValue>, EntryAddedEventArgs<TKey, TValue>> entryAdded = null,
            Action<IMap<TKey, TValue>, EntryRemovedEventArgs<TKey, TValue>> entryRemoved = null);

        // NOTES
        // 
        // because TKey can be a value type and have its default value (e.g. zero for an int value),
        // we cannot make it an optional parameter (i.e. 'TKey key = default') and have to explicitly
        // create two overloads of SubscribeAsync.

        /// <summary>
        /// Subscribe to events.
        /// </summary>
        /// <param name="key">The key identifying an entry.</param>
        /// <param name="includeValues">Whether the event arguments should contain values.</param>
        /// <param name="predicate">An optional predicate to filter entries.</param>
        /// <param name="entryAdded">An optional handler for <see cref="EntryEventType.Added"/> events.</param>
        /// <param name="entryRemoved">An optional handler for <see cref="EntryEventType.Removed"/> events.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        /// <remarks>
        /// <para>By default, event arguments contain values. However, for performance reasons, it
        /// is possible to omit values by specifying <paramref name="includeValues"/> as false.</para>
        /// </remarks>
        Task<Guid> SubscribeAsync(TKey key, bool includeValues = true, IPredicate predicate = null,
            Action<IMap<TKey, TValue>, EntryAddedEventArgs<TKey, TValue>> entryAdded = null,
            Action<IMap<TKey, TValue>, EntryRemovedEventArgs<TKey, TValue>> entryRemoved = null);

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>fixme</returns>
        Task<bool> UnsubscribeAsync(Guid subscriptionId);

        #endregion
    }
}
