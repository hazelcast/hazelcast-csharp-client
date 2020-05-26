﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Implementation.Topic;

namespace Hazelcast
{
    /// <summary>
    /// Defines the Hazelcast client.
    /// </summary>
    public interface IHazelcastClient : IAsyncDisposable
    {
        /// <summary>
        /// Opens the client.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A task that will complete when the client is open and ready.</returns>
        /// <remarks>
        /// <para>There is no equivalent 'close' method: a client is closed when it is disposed.</para>
        /// </remarks>
        Task OpenAsync(TimeSpan timeout = default);

        /// <summary>
        /// Opens the client.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client is open and ready.</returns>
        /// <remarks>
        /// <para>There is no equivalent 'close' method: a client is closed when it is disposed.</para>
        /// </remarks>
        Task OpenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets an <see cref="IMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A task that will complete when the map has been retrieved or created,
        /// and represents the map that has been retrieved or created.</returns>
        Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets an <see cref="IMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the map has been retrieved or created,
        /// and represents the map that has been retrieved or created.</returns>
        Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="Topic"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the topic messages.</typeparam>
        /// <param name="name">The unique name of the topic.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A task that will complete when the topic has been retrieved or created,
        /// and represents the topic that has been retrieved or created.</returns>
        Task<ITopic<T>> GetTopicAsync<T>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="Topic"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the topic messages.</typeparam>
        /// <param name="name">The unique name of the topic.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the topic has been retrieved or created,
        /// and represents the topic that has been retrieved or created.</returns>
        Task<ITopic<T>> GetTopicAsync<T>(string name, CancellationToken cancellationToken);
    }
}
