using System.Threading.Tasks;
using Hazelcast.DistributedObjects;

namespace Hazelcast
{
    /// <summary>
    /// Defines the Hazelcast client.
    /// </summary>
    public interface IHazelcastClient
    {
        // TODO: document here

        Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name);
    }
}
