﻿// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.CP;
using Hazelcast.DistributedObjects;
using Hazelcast.Models;
using Hazelcast.Sql;
using Hazelcast.Transactions;

namespace Hazelcast
{
    /// <summary>
    /// Represents the Hazelcast client.
    /// </summary>
    public interface IHazelcastClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of this client.
        /// </summary>
        /// <remarks>
        /// <para>The name of a client can be fully specified by <see cref="HazelcastOptions.ClientName" />
        /// option value. Alternatively, it is automatically generated.</para>
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the unique identifier of this client.
        /// </summary>
        /// <remarks>
        /// <para>The unique identifier of the client is a self-assigned random <see cref="Guid"/>.</para>
        /// </remarks>
        Guid Id { get; }

        /// <summary>
        /// Gets the name of the cluster.
        /// </summary>
        /// <remarks>
        /// <para>The name of the cluster is specified by the <see cref="HazelcastOptions.ClusterName"/>
        /// option value. Alternatively, it is "dev" by default.</para>
        /// </remarks>
        string ClusterName { get; }

        /// <summary>
        /// Gets the CP subsystem.
        /// </summary>
        ICPSubsystem CPSubsystem { get; }

        /// <summary>
        /// Returns a service to execute distributed SQL queries.
        /// </summary>
        ///<remarks>
        /// The service is in beta state. Behavior and API might be changed in future releases.
        /// </remarks>
        ISqlService Sql { get; }

        // TODO: consider implementing client.ClusterId
        /*
        /// <summary>
        /// Gets the unique identifier of the cluster.
        /// </summary>
        /// <remarks>
        /// <para>The unique identifier of the cluster is determined by the cluster itself and
        /// becomes available once the client has authenticated with the cluster.</para>
        /// </remarks>
        Guid ClusterId { get; }
        */

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
        /// Gets the client state.
        /// </summary>
        ClientState State { get; }

        /// <summary>
        /// Gets the options that were used to configure this client.
        /// </summary>
        /// <remarks>
        /// <para>This returns a clone of the options, and modifying this clone has no effect on the actual
        /// options used by the client, nor on the behavior of the client.</para>
        /// </remarks>
        HazelcastOptions Options { get; }

        /// <summary>
        /// Gets a snapshot of the members that the cluster declared to this client.
        /// </summary>
        IReadOnlyCollection<MemberInfoState> Members { get; }

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
        /// <param name="events">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<HazelcastClientEventHandlers> events);

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
        /// Gets information about all distributed objects known to the cluster.
        /// </summary>
        /// <returns>Information about all distributed objects know to the cluster.</returns>
        Task<IReadOnlyCollection<DistributedObjectInfo>> GetDistributedObjectsAsync();

        /// <summary>
        /// Gets an <see cref="IHMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <returns>The map that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name);

        /// <summary>
        /// Gets an <see cref="IHReplicatedMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <returns>The map that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHReplicatedMap<TKey, TValue>> GetReplicatedMapAsync<TKey, TValue>(string name);

        /// <summary>
        /// Gets an <see cref="IHMultiMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <returns>The map that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IHMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name);

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

        /// <summary>
        /// Gets an <see cref="IFlakeIdGenerator"/> distributed object.
        /// </summary>
        /// <param name="name">The unique name of the Flake Id Generator.</param>
        /// <returns>The Flake Id Generator that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IFlakeIdGenerator> GetFlakeIdGeneratorAsync(string name);
    }
}
