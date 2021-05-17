// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Query;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a transactional distributed map.
    /// </summary>
    /// <typeparam name="TKey">key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public interface IHTxMap<TKey, TValue> : ITransactionalObject
    {
        /// <summary>
        /// Transactional implementation of<see cref="IHMap{TKey,TValue}.ContainsKeyAsync(TKey)"/>.
        /// </summary>
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.DeleteAsync"/>.
        /// </summary>
        Task DeleteAsync(TKey key);

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.GetAsync(TKey)"/>.
        /// </summary>
        Task<TValue> GetAsync(TKey key);

        /// <summary>Locks the key and then gets and returns the value to which the specified key is mapped.</summary>
        /// <remarks>
        /// <para>The lock will be released at the end of the transaction (either commit or rollback).</para>
        /// </remarks>
        Task<TValue> GetForUpdateAsync(TKey key);

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.IsEmptyAsync()"/>.
        /// </summary>
        /// <returns><c>true</c> if the map does not contain entries; otherwise <c>false</c>.</returns>
        Task<bool> IsEmptyAsync();

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.GetKeysAsync()"/>.
        /// </summary>
        /// <returns>All keys.</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync();

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.GetKeysAsync(IPredicate)"/>.
        /// </summary>
        /// <param name="predicate">An predicate to filter the entries with.</param>
        /// <returns>All keys matching the predicate.</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate);

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.SetAsync(TKey, TValue)"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <remarks>
        /// <para>The inserted entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task SetAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of <see cref="IHMapBase{TKey,TValue}.PutAsync(TKey, TValue)"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <remarks>
        /// <para>The inserted entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> PutAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of <see cref="IHMapBase{TKey,TValue}.PutAsync(TKey,TValue, TimeSpan)"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <remarks>
        /// <para>The inserted entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> PutAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.PutIfAbsent(TKey, TValue)"/>.
        /// </summary>
        /// <remarks>
        /// <para>The inserted entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> PutIfAbsentAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of <see cref="IHMapBase{TKey,TValue}.RemoveAsync"/>.
        /// </summary>
        /// <remarks>
        /// <para>The removed entry wil be removed only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> RemoveAsync(TKey key);

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.RemoveAsync(TKey, TValue)"/>.
        /// </summary>
        /// <remarks>
        /// <para>The removed entry wil be removed only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<bool> RemoveAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.ReplaceAsync(TKey,TValue)"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="newValue">The new value.</param>
        /// <remarks>
        /// <para>The updated entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> ReplaceAsync(TKey key, TValue newValue);

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.ReplaceAsync(TKey,TValue,TValue)"/>.
        /// </summary>
        /// <remarks>
        /// <para>The updated entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<bool> ReplaceAsync(TKey key, TValue oldValue, TValue newValue);

        /// <summary>
        /// Transactional implementation of <see cref="IHMapBase{TKey,TValue}.GetSizeAsync"/>.
        /// </summary>
        Task<int> GetSizeAsync();

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.GetValuesAsync()"/>.
        /// </summary>
        Task<IReadOnlyCollection<TValue>> GetValuesAsync();

        /// <summary>
        /// Transactional implementation of <see cref="IHMap{TKey,TValue}.GetValuesAsync(IPredicate)"/>.
        /// </summary>
        Task<IReadOnlyCollection<TValue>> GetValuesAsync(IPredicate predicate);
    }
}
