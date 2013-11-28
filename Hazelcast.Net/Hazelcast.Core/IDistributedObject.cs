using System;

namespace Hazelcast.Core
{
    /// <summary>Base interface for all distributed objects.</summary>
    /// <remarks>Base interface for all distributed objects.</remarks>
    /// <seealso cref="IMap{K, V}">IMap&lt;K, V&gt;</seealso>
    /// <seealso cref="IQueue{E}">IQueue&lt;E&gt;</seealso>
    /// <seealso cref="IMultiMap{K,V}">IMultiMap&lt;K, V&gt;</seealso>
    /// <seealso cref="ITopic{E}">ITopic&lt;E&gt;</seealso>
    /// <seealso cref="ILock">ILock</seealso>
    /// <seealso cref="IExecutorService">IExecutorService</seealso>
    /// <seealso cref="ITransactionalMap{K,V}">ITransactionalMap&lt;K, V&gt;</seealso>
    /// <seealso cref="IITransactionalQueue{E}">ITransactionalQueue&lt;E&gt;</seealso>
    /// <seealso cref="ITransactionalMultiMap{K,V}">ITransactionalMultiMap&lt;K, V&gt;</seealso>
    public interface IDistributedObject
    {
        /// <summary>Returns the unique id for this object.</summary>
        /// <remarks>Returns the unique id for this object.</remarks>
        /// <returns>id the of this instance</returns>
        [Obsolete(@"use GetName() instead")]
        object GetId();

        /// <summary>Returns the key of partition this IDistributedObject is assigned to.</summary>
        /// <remarks>
        ///     Returns the key of partition this IDistributedObject is assigned to. The returned value only has meaning
        ///     for a non partitioned data-structure like an IAtomicLong. For a partitioned data-structure like an IMap
        ///     the returned value will not be null, but otherwise undefined.
        /// </remarks>
        /// <returns>the partition key.</returns>
        string GetPartitionKey();

        /// <summary>Returns the unique name for this IDistributedObject.</summary>
        /// <remarks>Returns the unique name for this IDistributedObject. Returned value will never be null.</remarks>
        /// <returns>the unique name for this object.</returns>
        string GetName();

        /// <summary>Returns the service name for this object.</summary>
        /// <remarks>Returns the service name for this object.</remarks>
        string GetServiceName();

        //REQUIRED
        /// <summary>Destroys this object cluster-wide.</summary>
        /// <remarks>
        ///     Destroys this object cluster-wide.
        ///     Clears and releases all resources for this object.
        /// </remarks>
        void Destroy();
    }
}