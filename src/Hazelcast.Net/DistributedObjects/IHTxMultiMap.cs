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
        /// <summary>Returns the collection of values associated with the key.</summary>
        /// <param name="key">the key whose associated values are to be returned</param>
        /// <returns>the collection of the values associated with the key.</returns>
        Task<IReadOnlyCollection<TValue>> GetAsync(TKey key);

        /// <summary>Stores a key-value pair in the multi-map.</summary>
        /// <param name="key">the key to be stored</param>
        /// <param name="value">the value to be stored</param>
        /// <returns>
        /// <c>true</c> if size of the multi-map is increased, <c>false</c> otherwise.
        /// </returns>
        Task<bool> PutAsync(TKey key, TValue value);

        /// <summary>Removes the given key value pair from the multi-map.</summary>
        /// <param name="key">the key of the entry to remove</param>
        /// <param name="value">the value of the entry to remove</param>
        /// <returns>true if the size of the multi-map changed after the remove operation, false otherwise.</returns>
        Task<bool> RemoveAsync(TKey key, TValue value);

        /// <summary>Removes all the entries with the given key.</summary>
        /// <param name="key">the key of the entries to remove</param>
        /// <returns>the collection of removed values associated with the given key</returns>
        Task<IReadOnlyCollection<TValue>> RemoveAsync(TKey key);

        /// <summary>Returns the number of key-value pairs in the multi-map.</summary>
        /// <returns>the number of key-value pairs in the multi-map.</returns>
        Task<int> GetSizeAsync();

        /// <summary>Returns number of values matching to given key in the multi-map.</summary>
        /// <param name="key">the key whose values count are to be returned</param>
        /// <returns>number of values matching to given key in the multi-map.</returns>
        Task<int> GetValueCountAsync(TKey key);
    }
}
