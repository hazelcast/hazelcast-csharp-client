﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;
using Hazelcast.Transactions;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client.
    /// </summary>
    public interface IHazelcastClient : IAsyncDisposable
    {
        /// <summary>
        /// Starts the client by connecting to the remote cluster.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A task that will complete when the client is connected.</returns>
        /// <exception cref="TaskTimeoutException">Failed to connect within the specified timeout.</exception>
        /// <remarks>
        /// <para>If the timeout is omitted, then the timeout configured in the options is used.</para>
        /// </remarks>
        Task StartAsync(TimeSpan timeout = default);

        /// <summary>
        /// Starts the client by connecting to the remote cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client is connected.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Whether the client is active.
        /// </summary>
        /// <returns><c>true</c> if the client is active; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>The client can be active but not connected, trying to reconnect.</para>
        /// </remarks>
        bool IsActive { get; }

        /// <summary>
        /// Whether the client is connected.
        /// </summary>
        /// <returns><c>true</c> if the client is connected; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>The client can be active but not connected, trying to reconnect.</para>
        /// </remarks>
        bool IsConnected { get; }

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <returns>A new transaction context.</returns>
        Task<ITransactionContext> BeginTransactionAsync();

        /// <summary>
        /// Begins a new transaction.
        /// </summary>
        /// <param name="options">Transaction options.</param>
        /// <returns>A new transaction context.</returns>
        Task<ITransactionContext> BeginTransactionAsync(TransactionOptions options);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="handle">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<HazelcastClientEventHandlers> handle);

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>Whether the un-registration was successful on the server.</returns>
        ValueTask<bool> UnsubscribeAsync(Guid subscriptionId);

        /// <summary>
        /// Destroys a distributed object.
        /// </summary>
        /// <param name="o">The object to destroy.</param>
        /// <returns>A task that will complete when the object has been destroyed.</returns>
        /// <remarks>
        /// <para>Destroying a distributed object completely deletes the object on the cluster.</para>
        /// </remarks>
        ValueTask DestroyAsync(IDistributedObject o);

        /// <summary>
        /// Destroys a distributed object.
        /// </summary>
        /// <param name="serviceName">The service name of the object to destroy.</param>
        /// <param name="name">The name of the object to destroy.</param>
        /// <returns>A task that will complete when the object has been destroyed.</returns>
        /// <remarks>
        /// <para>Destroying a distributed object completely deletes the object on the cluster.</para>
        /// </remarks>
        ValueTask DestroyAsync(string serviceName, string name);

        /// <summary>
        /// Gets an <see cref="IHDictionary{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <returns>The map that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHDictionary<TKey, TValue>> GetDictionaryAsync<TKey, TValue>(string name);

        /// <summary>
        /// Gets an <see cref="IHReplicatedDictionary{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <returns>The map that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHReplicatedDictionary<TKey, TValue>> GetReplicatedDictionaryAsync<TKey, TValue>(string name);

        /// <summary>
        /// Gets an <see cref="IHMultiDictionary{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <returns>The map that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHMultiDictionary<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name);

        /// <summary>
        /// Gets an <see cref="IHTopic{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the topic messages.</typeparam>
        /// <param name="name">The unique name of the topic.</param>
        /// <returns>The topic that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHTopic<T>> GetTopicAsync<T>(string name);

        /// <summary>
        /// Gets an <see cref="IHList{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the list items.</typeparam>
        /// <param name="name">The unique name of the list.</param>
        /// <returns>The list that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHList<T>> GetListAsync<T>(string name);

        /// <summary>
        /// Gets an <see cref="IHSet{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the set items.</typeparam>
        /// <param name="name">The unique name of the set.</param>
        /// <returns>The set that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHSet<T>> GetSetAsync<T>(string name);

        /// <summary>
        /// Gets an <see cref="IHQueue{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the queue items.</typeparam>
        /// <param name="name">The unique name of the queue.</param>
        /// <returns>The queue that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHQueue<T>> GetQueueAsync<T>(string name);

        /// <summary>
        /// Gets an <see cref="IHRingBuffer{T}"/> distributed object.
        /// </summary>
        /// <typeparam name="T">The type of the ring buffer items.</typeparam>
        /// <param name="name">The unique name of the ring buffer.</param>
        /// <returns>The ring buffer that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHRingBuffer<T>> GetRingBufferAsync<T>(string name);
    }
}
