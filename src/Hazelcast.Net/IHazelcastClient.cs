// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Transactions;

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
        /// <remarks>
        /// <para>There is no equivalent 'close' method: a client is closed when it is disposed.</para>
        /// </remarks>
        Task OpenAsync(TimeSpan timeout = default);

        /// <summary>
        /// Opens the client.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>There is no equivalent 'close' method: a client is closed when it is disposed.</para>
        /// </remarks>
        Task OpenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A new transaction context.</returns>
        Task<ITransactionContext> BeginTransactionAsync(TimeSpan timeout = default);

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A new transaction context.</returns>
        Task<ITransactionContext> BeginTransactionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <param name="options">Transaction options.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A new transaction context.</returns>
        Task<ITransactionContext> BeginTransactionAsync(TransactionOptions options, TimeSpan timeout = default);

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <param name="options">Transaction options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A new transaction context.</returns>
        Task<ITransactionContext> BeginTransactionAsync(TransactionOptions options, CancellationToken cancellationToken);

        /// <summary>
        /// Destroys a distributed object.
        /// </summary>
        /// <param name="o">The object to destroy.</param>
        /// <param name="timeout">A timeout.</param>
        ValueTask DestroyAsync(IDistributedObject o, TimeSpan timeout = default);

        /// <summary>
        /// Destroys a distributed object.
        /// </summary>
        /// <param name="o">The object to destroy.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask DestroyAsync(IDistributedObject o, CancellationToken cancellationToken);

        /// <summary>
        /// Gets an <see cref="IHMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The map that was retrieved or created.</returns>
        Task<IHMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets an <see cref="IHMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The map that was retrieved or created.</returns>
        Task<IHMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets an <see cref="IHReplicatedMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The map that was retrieved or created.</returns>
        Task<IHReplicatedMap<TKey, TValue>> GetReplicatedMapAsync<TKey, TValue>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets an <see cref="IHReplicatedMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The map that was retrieved or created.</returns>
        Task<IHReplicatedMap<TKey, TValue>> GetReplicatedMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets an <see cref="IHMultiMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The map that was retrieved or created.</returns>
        Task<IHMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets an <see cref="IHMultiMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The map that was retrieved or created.</returns>
        Task<IHMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTopic{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the topic messages.</typeparam>
        /// <param name="name">The unique name of the topic.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The topic that was retrieved or created.</returns>
        Task<IHTopic<T>> GetTopicAsync<T>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTopic{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the topic messages.</typeparam>
        /// <param name="name">The unique name of the topic.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The topic that was retrieved or created.</returns>
        Task<IHTopic<T>> GetTopicAsync<T>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHList{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the list items.</typeparam>
        /// <param name="name">The unique name of the list.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The list that was retrieved or created.</returns>
        Task<IHList<T>> GetListAsync<T>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHList{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the list items.</typeparam>
        /// <param name="name">The unique name of the list.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The list that was retrieved or created.</returns>
        Task<IHList<T>> GetListAsync<T>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHSet{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the set items.</typeparam>
        /// <param name="name">The unique name of the set .</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The set that was retrieved or created.</returns>
        Task<IHSet<T>> GetSetAsync<T>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHSet{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the set items.</typeparam>
        /// <param name="name">The unique name of the set.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The set that was retrieved or created.</returns>
        Task<IHSet<T>> GetSetAsync<T>(string name, CancellationToken cancellationToken);
    }
}
