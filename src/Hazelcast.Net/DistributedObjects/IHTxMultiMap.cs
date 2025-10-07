// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>Transactional implementation of MultiMap</summary>
    public interface IHTxMultiMap<TKey, TValue> : ITransactionalObject
    {
        /// <summary>
        /// Transactional <see cref="IHMultiMap{TKey, TValue}.GetAsync(TKey)"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<TValue>> GetAsync(TKey key);

        /// <summary>
        /// Transactional <see cref="IHMultiMap{TKey, TValue}.PutAsync(TKey, TValue)"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<bool> PutAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional <see cref="IHMultiMap{TKey, TValue}.RemoveAsync(TKey, TValue)"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<bool> RemoveAsync(TKey key, TValue value);

        /// <summary>
        /// Transactional <see cref="IHMultiMap{TKey, TValue}.RemoveAsync(TKey)"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<IReadOnlyCollection<TValue>> RemoveAsync(TKey key);

        /// <summary>
        /// Transactional <see cref="IHMultiMap{TKey, TValue}.GetSizeAsync"/>
        /// </summary>
        /// <returns></returns>
        Task<int> GetSizeAsync();

        /// <summary>
        /// Transactional <see cref="IHMultiMap{TKey, TValue}.GetValueCountAsync"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<int> GetValueCountAsync(TKey key);
    }
}
