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

namespace Hazelcast.Transactions
{
    /// <summary>
    /// Represents a transaction context.
    /// </summary>
    public interface ITransactionContext : IAsyncDisposable
    {
        /// <summary>
        /// Gets the unique identifier of the transaction.
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        /// Gets the state of the transaction.
        /// </summary>
        TransactionState State { get; }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        Task CommitAsync(TimeSpan timeout = default);

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task CommitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Rolls the transaction back.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        Task RollbackAsync(TimeSpan timeout = default);

        /// <summary>
        /// Rolls the transaction back.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task RollbackAsync(CancellationToken cancellationToken);

        // Objects

        /// <summary>
        /// Gets a <see cref="IHTxList{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="name">The unique name of the list.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional list that was retrieved or created.</returns>
        Task<IHTxList<TItem>> GetListAsync<TItem>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxList{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="name">The unique name of the list.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional list that was retrieved or created.</returns>
        Task<IHTxList<TItem>> GetListAsync<TItem>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTxList{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="source">The original, non-transactional list.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional list that was retrieved or created.</returns>
        Task<IHTxList<TItem>> GetListAsync<TItem>(IHList<TItem> source, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxList{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="source">The original, non-transactional list.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional list that was retrieved or created.</returns>
        Task<IHTxList<TItem>> GetListAsync<TItem>(IHList<TItem> source, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTxSet{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="name">The unique name of the set.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional set that was retrieved or created.</returns>
        Task<IHTxSet<TItem>> GetSetAsync<TItem>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxSet{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="name">The unique name of the set.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional set that was retrieved or created.</returns>
        Task<IHTxSet<TItem>> GetSetAsync<TItem>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTxSet{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="source">The original, non-transactional set.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional set that was retrieved or created.</returns>
        Task<IHTxSet<TItem>> GetSetAsync<TItem>(IHSet<TItem> source, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxSet{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="source">The original, non-transactional set.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional set that was retrieved or created.</returns>
        Task<IHTxSet<TItem>> GetSetAsync<TItem>(IHSet<TItem> source, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTxQueue{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="name">The unique name of the queue.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional queue that was retrieved or created.</returns>
        Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxQueue{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="name">The unique name of the v.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional queue that was retrieved or created.</returns>
        Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTxQueue{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="source">The original, non-transactional queue.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional queue that was retrieved or created.</returns>
        Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(IHQueue<TItem> source, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxQueue{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="source">The original, non-transactional queue.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional queue that was retrieved or created.</returns>
        Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(IHQueue<TItem> source, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTxMultiMap{TKey, TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxMultiMap{TKey, TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the v.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTxMultiMap{TKey, TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The original, non-transactional map.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(IHMultiMap<TKey, TValue> source, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxMultiMap{TKey, TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The original, non-transactional map.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(IHMultiMap<TKey, TValue> source, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTxMap{TKey, TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxMap{TKey, TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the v.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Gets a <see cref="IHTxMap{TKey, TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The original, non-transactional map.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(IHMap<TKey, TValue> source, TimeSpan timeout = default);

        /// <summary>
        /// Gets a <see cref="IHTxMap{TKey, TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The original, non-transactional map.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(IHMap<TKey, TValue> source, CancellationToken cancellationToken);
    }
}
