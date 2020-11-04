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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Predicates;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a transactional distributed dictionary.
    /// </summary>
    /// <typeparam name="TKey">key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public interface IHTxDictionary<TKey, TValue> : ITransactionalObject
    {
        /// <summary>
        /// Transactional implementation of<see cref="IHDictionary{TKey,TValue}.ContainsKeyAsync(TKey)"/>.
        /// </summary>
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.RemoveAsync(TKey)"/>.
        /// </summary>
        Task RemoveAsync(TKey key);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.GetAsync(TKey)"/>.
        /// </summary>
        Task<Attempt<TValue>> GetAsync(TKey key);

        /// <summary>Locks the key and then gets and returns the value to which the specified key is mapped.</summary>
        /// <remarks>
        /// <para>The lock will be released at the end of the transaction (either commit or rollback).</para>
        /// </remarks>
        Task<TValue> GetForUpdateAsync(TKey key);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.IsEmptyAsync()"/>.
        /// </summary>
        /// <returns><c>true</c> if the map does not contain entries; otherwise <c>false</c>.</returns>
        Task<bool> IsEmptyAsync();

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.GetKeysAsync()"/>.
        /// </summary>
        /// <returns>All keys.</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync();

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.GetKeysAsync(IPredicate)"/>.
        /// </summary>
        /// <param name="predicate">An predicate to filter the entries with.</param>
        /// <returns>All keys matching the predicate.</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.SetAsync(TKey, TValue)"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <remarks>
        /// <para>The inserted entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task SetAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.GetAndSetAsync(TKey, TValue)"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <remarks>
        /// <para>The inserted entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> GetAndSetAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.SetAsync(TKey, TValue, TimeSpan)"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <remarks>
        /// <para>The inserted entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task SetAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.GetAndSetAsync(TKey, TValue, TimeSpan)"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <remarks>
        /// <para>The inserted entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> GetAndSetAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.GetOrAddAsync(TKey, TValue)"/>.
        /// </summary>
        /// <remarks>
        /// <para>The inserted entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> GetOrAddAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.GetAndRemoveAsync(TKey)"/>.
        /// </summary>
        /// <remarks>
        /// <para>The removed entry wil be removed only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> GetAndRemoveAsync(TKey key);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.RemoveAsync(TKey, TValue)"/>.
        /// </summary>
        /// <remarks>
        /// <para>The removed entry wil be removed only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<bool> RemoveAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.TryUpdateAsync(TKey, TValue)"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="newValue">The new value.</param>
        /// <remarks>
        /// <para>The updated entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<TValue> TryUpdateAsync(TKey key, TValue newValue);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.TryUpdateAsync(TKey, TValue, TValue)"/>.
        /// </summary>
        /// <remarks>
        /// <para>The updated entry wil be visible only in the current transaction context, until the transaction is committed.</para>
        /// </remarks>
        Task<bool> TryUpdateAsync(TKey key, TValue oldValue, TValue newValue);

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.CountAsync()"/>.
        /// </summary>
        Task<int> CountAsync();

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.GetValuesAsync()"/>.
        /// </summary>
        Task<IReadOnlyList<TValue>> GetValuesAsync();

        /// <summary>
        /// Transactional implementation of <see cref="IHDictionary{TKey,TValue}.GetValuesAsync(IPredicate)"/>.
        /// </summary>
        Task<IReadOnlyList<TValue>> GetValuesAsync(IPredicate predicate);
    }
}
