/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Collections.Generic;
using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>
    /// Transactional implementation of Map
    /// </summary>
    /// <typeparam name="K">key type</typeparam>
    /// <typeparam name="V">Value type</typeparam>
    public interface ITransactionalMap<K, V> : ITransactionalObject
    {
        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Contains(object)"/>
        /// .
        /// </summary>
        /// <seealso cref="IMap{K, V}.Contains(object)"/>
        bool ContainsKey(object key);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Get(object)"/>
        /// .
        /// </summary>
        /// <seealso cref="IMap{K, V}.Get(object)"/>
        V Get(object key);

        /// <summary>Locks the key and then gets and returns the value to which the specified key is mapped.</summary>
        /// <remarks>
        /// Locks the key and then gets and returns the value to which the specified key is mapped.
        /// Lock will be released at the end of the transaction (either commit or rollback).
        /// </remarks>
        /// <seealso cref="IMap{K, V}.Get(object)"/>
        V GetForUpdate(object key);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="BaseMap{K, V}.Size()"/>
        /// .
        /// </summary>
        /// <seealso cref="BaseMap{K, V}.Size()"/>
        int Size();

        /// <summary>
        /// Transactional implementation of
        /// <see cref="BaseMap{K, V}.IsEmpty()"/>
        /// .
        /// </summary>
        /// <seealso cref="BaseMap{K, V}.IsEmpty()"/>
        bool IsEmpty();

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Put(object, object)"/>
        /// .
        /// <p/>
        /// The object to be put will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Put(object, object)"/>
        V Put(K key, V value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Put(object, object, long, Hazelcast.Net.Ext.TimeUnit)"/>
        /// .
        /// <p/>
        /// The object to be put will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Put(object, object, long, Hazelcast.Net.Ext.TimeUnit)"/>
        V Put(K key, V value, long ttl, TimeUnit timeunit);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Set(object, object)"/>
        /// .
        /// <p/>
        /// The object to be set will be accessible only in the current transaction context till transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Set(object, object)"/>
        void Set(K key, V value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.PutIfAbsent(object, object)"/>
        /// .
        /// <p/>
        /// The object to be put will be accessible only in the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.PutIfAbsent(object, object)"/>
        V PutIfAbsent(K key, V value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Replace(object, object)"/>
        /// .
        /// <p/>
        /// The object to be replaced will be accessible only in the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Replace(object, object)"/>
        V Replace(K key, V value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Replace(object, object, object)"/>
        /// .
        /// <p/>
        /// The object to be replaced will be accessible only in the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Replace(object, object, object)"/>
        bool Replace(K key, V oldValue, V newValue);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="Sharpen.Collections.Remove(object)"/>
        /// .
        /// <p/>
        /// The object to be removed will be removed from only the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="Sharpen.Collections.Remove(object)"/>
        V Remove(object key);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Delete(object)"/>
        /// .
        /// <p/>
        /// The object to be deleted will be removed from only the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Delete(object)"/>
        void Delete(object key);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Remove(object, object)"/>
        /// .
        /// <p/>
        /// The object to be removed will be removed from only the current transaction context until the transaction is committed.
        /// </summary>
        /// <seealso cref="IMap{K, V}.Remove(object, object)"/>
        bool Remove(object key, object value);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Keys()"/>
        /// .
        /// </summary>
        /// <seealso cref="IMap{K, V}.Keys()"/>
        ICollection<K> KeySet();

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.KeySet(Hazelcast.Query.Predicate{K, V})"/>
        /// .
        /// </summary>
        /// <seealso cref="IMap{K, V}.KeySet(Hazelcast.Query.Predicate{K, V})"/>
        ICollection<K> KeySet(IPredicate<K,V> predicate);

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Values()"/>
        /// .
        /// </summary>
        /// <seealso cref="IMap{K, V}.Values()"/>
        ICollection<V> Values();

        /// <summary>
        /// Transactional implementation of
        /// <see cref="IMap{K, V}.Values(Hazelcast.Query.Predicate{K, V})"/>
        /// .
        /// </summary>
        /// <seealso cref="IMap{K, V}.Values(Hazelcast.Query.Predicate{K, V})"/>
        ICollection<V> Values(IPredicate<K,V> predicate);
    }
}