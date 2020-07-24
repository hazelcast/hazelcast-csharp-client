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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Predicates;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Transactional implementation of Map
    /// </summary>
    /// <typeparam name="TKey">key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public interface IHTxMap<TKey, TValue> : ITransactionalObject
    {
        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.ContainsKey(object)"/>
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.ContainsKey(object)"/>
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Delete(object)"/>
        /// .
        /// <p/>
        /// The object to be deleted will be removed from only the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Delete(object)"/>
        Task RemoveAsync(TKey key);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Get(object)"/>
        /// .
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Get(object)"/>
        Task<TValue> GetAsync(TKey key);

        /// <summary>Locks the key and then gets and returns the value to which the specified key is mapped.</summary>
        /// <remarks>
        /// Locks the key and then gets and returns the value to which the specified key is mapped.
        /// Lock will be released at the end of the transaction (either commit or rollback).
        /// </remarks>
        /// <seealso cref="IHMap{TKey,TValue}.Get(object)"/>
        Task<TValue> GetForUpdateAsync(TKey key);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.IsEmpty()"/>
        /// .
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.IsEmpty()"/>
        Task<bool> IsEmpty();

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.KeySet()"/>
        /// .
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.KeySet()"/>
        Task<IReadOnlyList<TKey>> GetKeysAsync();

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.KeySet(IPredicate)"/>
        /// .
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.KeySet(IPredicate)"/>
        Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Put(K, V)"/>
        /// .
        /// <p/>
        /// The object to be put will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Put(K, V)"/>
        Task<TValue> AddOrUpdateAndReturnAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Put(K, V, long, TimeUnit)"/>
        /// .
        /// <p/>
        /// The object to be put will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Put(K, V, long, TimeUnit)"/>
        Task<TValue> AddOrUpdateAndReturnTtlAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.PutIfAbsent(K, V)"/>
        /// .
        /// <p/>
        /// The object to be put will be accessible only in the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.PutIfAbsent(K, V)"/>
        Task<TValue> AddAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Remove(object)"/>
        /// .
        /// <p/>
        /// The object to be removed will be removed from only the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Remove(object)"/>
        Task<TValue> RemoveAndReturnAsync(TKey key);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Remove(object, object)"/>
        /// .
        /// <p/>
        /// The object to be removed will be removed from only the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Remove(object, object)"/>
        Task<bool> RemoveAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Replace(K, V)"/>
        /// .
        /// <p/>
        /// The object to be replaced will be accessible only in the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Replace(K, V)"/>
        Task<TValue> ReplaceAndReturnAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Replace(K, V, V)"/>
        /// .
        /// <p/>
        /// The object to be replaced will be accessible only in the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Replace(K, V, V)"/>
        Task<bool> ReplaceAsync(TKey key, TValue oldValue, TValue newValue);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Set(K, V)"/>
        /// .
        /// <p/>
        /// The object to be set will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Set(K, V)"/>
        Task AddOrUpdateAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="System.Drawing.Size"/>
        /// .
        /// </summary>
        /// <seealso cref="System.Drawing.Size"/>
        Task<int> CountAsync();

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Values()"/>
        /// .
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Values()"/>
        Task<IReadOnlyList<TValue>> GetValuesAsync();

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IHMap{TKey,TValue}.Values(IPredicate)"/>
        /// .
        /// </summary>
        /// <seealso cref="IHMap{TKey,TValue}.Values(IPredicate)"/>
        Task<IReadOnlyList<TValue>> GetValuesAsync(IPredicate predicate);
    }
}
