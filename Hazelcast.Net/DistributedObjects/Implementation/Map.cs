using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregators;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Data.Map;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation
{
    /// <summary>
    /// Implements <see cref="IMap{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal class Map<TKey, TValue> : DistributedObjectBase, IMap<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Map{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        public Map(string serviceName, string name, Cluster cluster, ISerializationService serializationService)
            : base(serviceName, name, cluster, serializationService)
        { }

        #region Setting

        /// <inheritdoc />
        public async Task<TValue> AddAsync(TKey key, TValue value)
            => await AddAsync(key, value, Timeout.InfiniteTimeSpan);

        public async Task<TValue> AddAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            // FIXME but... TKey could be a value type?!
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var keyData = ToData(key);
            var valueData = ToData(value);

            // FIXME again here ClientProxy has the weird ExceptionUtil.Rethrow ??
            // FIXME what is the resolution of ttl? says seconds?

            var requestMessage = MapPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, (long) timeToLive.TotalMilliseconds);
            var responseMessage = await Cluster.SendToKeyOwnerAsync(requestMessage, keyData);
            var response = MapPutCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        public Task AddAsync(IDictionary<TKey, TValue> entries)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> AddIfMissing(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> AddIfMissing(TKey key, TValue value, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public void AddTransient(TKey key, TValue value, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public TValue Replace(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public bool Replace(TKey key, TValue expectedValue, TValue newValue)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public bool TryPut(TKey key, TValue value, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Getting

        public Task<TValue> GetAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<TKey, TValue>> GetAllAsync(ICollection<TKey> keys)
        {
            throw new NotImplementedException();
        }

        public Task<IEntryView<TKey, TValue>> GetEntryViewAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public ISet<KeyValuePair<TKey, TValue>> EntrySet(IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        public ISet<TKey> KeySet(IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        public ICollection<TValue> Values(IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Removing

        public bool TryRemoveAsync(TKey key, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task<TValue> RemoveAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAsync(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task ClearAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Testing and Counting

        public int Count()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsEmpty()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ContainsKeyAsync(TKey key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ContainsValueAsync(TValue value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Caching

        public bool Evict(TKey key)
        {
            throw new NotImplementedException();
        }

        public void EvictAll()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Processing

        public object ExecuteOnKey(TKey key, IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public IDictionary<TKey, object> ExecuteOnKeys(ISet<TKey> keys, IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public IDictionary<TKey, object> ExecuteOnEntries(IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        public Task<object> SubmitToKey(TKey key, IEntryProcessor processor)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Locking

        public void Lock(TKey key)
        {
            throw new NotImplementedException();
        }

        public void Lock(TKey key, TimeSpan leaseTime)
        {
            throw new NotImplementedException();
        }

        public bool TryLock(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryLock(TKey key, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public bool TryLock(TKey key, TimeSpan timeout, TimeSpan leaseTime)
        {
            throw new NotImplementedException();
        }

        public bool IsLocked(TKey key)
        {
            throw new NotImplementedException();
        }

        public void ForceUnlock(TKey key)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Indexing

        public void AddIndex(IndexType type, params string[] attributes)
        {
            throw new NotImplementedException();
        }

        public void AddIndex(IndexConfig indexConfig)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Aggregating and Projecting

        public TResult Aggregate<TResult>(IAggregator<TResult> aggregator, IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        public ICollection<TResult> Project<TResult>(IProjection projection, IPredicate predicate = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Intercepting

        public void RemoveInterceptor(string id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Events

        public bool RemoveEntryListener(Guid id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
