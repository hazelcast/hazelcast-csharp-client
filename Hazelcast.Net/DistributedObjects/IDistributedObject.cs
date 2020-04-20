namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines the base contract for all Hazelcast distributed objects.
    /// </summary>
    public interface IDistributedObject
    {
        /// <summary>
        /// Gets the name of the service managing this object.
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Gets the unique name of the object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the partition key of this object.
        /// </summary>
        /// <returns>The partition key.</returns>
        /// <remarks>
        /// <para>The returned value has meaning only for non-partitioned data structures, such as
        /// IAtomicLong. For partitioned data structures such as <see cref="IMap"/>, the returned
        /// value is not null but has no meaning.</para>
        /// </remarks>
        string PartitionKey { get; }

        /// <summary>
        /// Destroys the object, cluster-wide.
        /// </summary>
        void Destroy();
    }
}
