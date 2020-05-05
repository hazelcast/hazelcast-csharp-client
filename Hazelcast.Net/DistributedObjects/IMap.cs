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
    public partial interface IMap<TKey, TValue> : IDistributedObject
    {
        // NOTES
        //
        // In most cases it would be pointless to return async enumerable since we must fetch
        // everything from the network anyways (else we'd hang the socket) before returning,
        // and therefore all that remains is CPU-bound de-serialization of data.
    }
}
