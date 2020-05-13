using System.Threading.Tasks;
using Hazelcast.DistributedObjects;

namespace Hazelcast
{
    /// <summary>
    /// Defines the Hazelcast client.
    /// </summary>
    public interface IHazelcastClient // TODO: IDisposable + close
    {
        /// <summary>
        /// Opens the client.
        /// </summary>
        /// <returns>A task that will complete when the client is open and ready.</returns>
        Task OpenAsync();

        /// <summary>
        /// Gets an <see cref="IMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <returns>A task that will complete when the map has been retrieved or created,
        /// and represents the map that has been retrieved or created.</returns>
        Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name);

        /// <summary>
        /// Gets a <see cref="Topic{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the topic messages.</typeparam>
        /// <param name="name">The unique name of the topic.</param>
        /// <returns>A task that will complete when the topic has been retrieved or created,
        /// and represents the topic that has been retrieved or created.</returns>
        Task<ITopic<T>> GetTopicAsync<T>(string name);
    }
}
