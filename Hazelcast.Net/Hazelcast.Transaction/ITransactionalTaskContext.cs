// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Core;

namespace Hazelcast.Transaction
{
    /// <summary>
    ///     Provides a context to access transactional data-structures like the
    ///     <see cref="ITransactionalMap{TKey,TValue}">Hazelcast.Core.ITransactionalMap&lt;K, V&gt;</see>
    ///     .
    /// </summary>
    public interface ITransactionalTaskContext
    {
        /// <summary>Returns the transactional list instance with the specified name.</summary>
        /// <remarks>Returns the transactional list instance with the specified name.</remarks>
        /// <param name="name">name of the list</param>
        /// <returns>transactional list instance with the specified name</returns>
        ITransactionalList<T> GetList<T>(string name);

        /// <summary>Returns the transactional distributed map instance with the specified name.</summary>
        /// <remarks>Returns the transactional distributed map instance with the specified name.</remarks>
        /// <param name="name">name of the distributed map</param>
        /// <returns>transactional distributed map instance with the specified name</returns>
        ITransactionalMap<TKey, TValue> GetMap<TKey, TValue>(string name);

        /// <summary>Returns the transactional multimap instance with the specified name.</summary>
        /// <remarks>Returns the transactional multimap instance with the specified name.</remarks>
        /// <param name="name">name of the multimap</param>
        /// <returns>transactional multimap instance with the specified name</returns>
        ITransactionalMultiMap<TKey, TValue> GetMultiMap<TKey, TValue>(string name);

        /// <summary>Returns the transactional queue instance with the specified name.</summary>
        /// <remarks>Returns the transactional queue instance with the specified name.</remarks>
        /// <param name="name">name of the queue</param>
        /// <returns>transactional queue instance with the specified name</returns>
        ITransactionalQueue<T> GetQueue<T>(string name);

        /// <summary>Returns the transactional set instance with the specified name.</summary>
        /// <remarks>Returns the transactional set instance with the specified name.</remarks>
        /// <param name="name">name of the set</param>
        /// <returns>transactional set instance with the specified name</returns>
        ITransactionalSet<T> GetSet<T>(string name);

        T GetTransactionalObject<T>(string serviceName, string name) where T : ITransactionalObject;
    }
}