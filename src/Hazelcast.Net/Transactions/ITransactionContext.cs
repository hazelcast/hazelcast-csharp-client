// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
        /// Commits the transaction.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Rolls the transaction back.
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        /// <remarks>
        /// <para>If the transaction has neither been committed nor rolled back when the
        /// <see cref="ITransactionContext"/> is disposed, it will be committed if it has
        /// been completed, else it will be rolled back.</para>
        /// </remarks>
        void Complete();

        // Objects

        /// <summary>
        /// Gets a <see cref="IHTxList{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="name">The unique name of the list.</param>
        /// <returns>The transactional list that was retrieved or created.</returns>
        Task<IHTxList<TItem>> GetListAsync<TItem>(string name);

        /// <summary>
        /// Gets a <see cref="IHTxSet{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="name">The unique name of the set.</param>
        /// <returns>The transactional set that was retrieved or created.</returns>
        Task<IHTxSet<TItem>> GetSetAsync<TItem>(string name);

        /// <summary>
        /// Gets a <see cref="IHTxQueue{TItem}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="name">The unique name of the v.</param>
        /// <returns>The transactional queue that was retrieved or created.</returns>
        Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(string name);

        /// <summary>
        /// Gets a <see cref="IHTxMultiMap{TKey,TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the v.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name);

        /// <summary>
        /// Gets a <see cref="IHTxMap{TKey,TValue}"/> transactional distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the v.</param>
        /// <returns>The transactional map that was retrieved or created.</returns>
        Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name);
    }
}
