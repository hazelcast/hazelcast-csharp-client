namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines a processor that can process the entries of an <see cref="IMap{TKey,TValue}"/> on the server.
    /// </summary>
    /// <remarks>
    /// <para>Client-side <see cref="IEntryProcessor"/> implementations do not have any processing logic,
    /// they must be backed by a corresponding processor registered on the server and containing the
    /// actual implementation.</para>
    /// </remarks>
    public interface IEntryProcessor
    { }
}
