using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation
{
    /// <summary>
    /// Implements a caching version of <see cref="IMap{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal class CachedMap<TKey, TValue> : Map<TKey, TValue>
    {
        private readonly NearCacheBase _cache;

        /// <summary>
        /// Initializes a new version of the <see="CacheMap{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="lockReferenceIdSequence">A lock reference identifiers sequence.</param>
        /// <param name="cache">A cache.</param>
        public CachedMap(string serviceName, string name, Cluster cluster, ISerializationService serializationService, ISequence<long> lockReferenceIdSequence, NearCacheBase cache)
            : base(serviceName, name, cluster, serializationService, lockReferenceIdSequence)
        {
            _cache = cache;
        }

        #region Setting

        // PutAsyncInternal?
        // etc
        // TODO: keep implementing

        #endregion

        #region Getting

        /// <inheritdoc />
        protected override async Task<TValue> GetAsync(IData keyData)
        {
            try
            {
                var (result, value) = await _cache.TryGetOrAddAsync(keyData, data => base.GetAsync(keyData));
                return result ? value : default;
            }
            catch
            {
                _cache.Invalidate(keyData);
                throw;
            }
        }

        /// <inheritdoc />
        protected override async Task<bool> ContainsKeyAsync(IData keyData)
        {
            return _cache.ContainsKey(keyData) || await base.ContainsKeyAsync(keyData);
        }

        #endregion

        #region Removing

        /// <inheritdoc />
        protected override async Task<bool> TryRemoveAsync(IData keyData, TimeSpan timeout)
        {
            var removed = await base.TryRemoveAsync(keyData, timeout);
            if (removed) _cache.Invalidate(keyData);
            return removed;
        }

        /// <inheritdoc />
        protected override async Task<TValue> RemoveAsync(IData keyData)
        {
            try
            {
                return await base.RemoveAsync(keyData);
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task<bool> RemoveAsync(IData keyData, IData valueData)
        {
            try
            {
                return await base.RemoveAsync(keyData, valueData);
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task DeleteAsync(IData keyData)
        {
            try
            {
                await base.RemoveAsync(keyData);
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        #endregion
    }
}
