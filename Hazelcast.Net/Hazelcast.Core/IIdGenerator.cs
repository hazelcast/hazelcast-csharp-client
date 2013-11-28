namespace Hazelcast.Core
{
    /// <summary>ICluster-wide unique id generator.</summary>
    /// <remarks>ICluster-wide unique id generator.</remarks>
    public interface IIdGenerator : IDistributedObject
    {
        /// <summary>Try to initialize this IIdGenerator instance with given id</summary>
        /// <returns>true if initialization success</returns>
        bool Init(long id);

        /// <summary>Generates and returns cluster-wide unique id.</summary>
        /// <remarks>
        ///     Generates and returns cluster-wide unique id.
        ///     Generated ids are guaranteed to be unique for the entire cluster
        ///     as long as the cluster is live. If the cluster restarts then
        ///     id generation will start from 0.
        /// </remarks>
        /// <returns>cluster-wide new unique id</returns>
        long NewId();
    }
}