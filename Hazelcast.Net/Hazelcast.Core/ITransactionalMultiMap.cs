using System.Collections.Generic;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Transactional implementation of
    ///     <see cref="IBaseMultiMap{K,V}">BaseMultiMap&lt;K, V&gt;</see>
    ///     .
    /// </summary>
    /// <seealso cref="IBaseMultiMap{K,V}">BaseMultiMap&lt;K, V&gt;</seealso>
    /// <seealso cref="IMultiMap{K,V}">IMultiMap&lt;K, V&gt;</seealso>
    /// <?></?>
    /// <?></?>
    public interface ITransactionalMultiMap<K, V> : IBaseMultiMap<K, V>, ITransactionalObject
    {
        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool Put(K key, V value);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        ICollection<V> Get(K key);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        bool Remove(object key, object value);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        ICollection<V> Remove(object key);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        int ValueCount(K key);

        /// <summary>
        ///     <inheritDoc></inheritDoc>
        /// </summary>
        int Size();
    }
}