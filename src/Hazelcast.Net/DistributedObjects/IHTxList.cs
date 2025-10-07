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
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a transactional list
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public interface IHTxList<TItem>
    {
        /// <summary>Add new item to transactional list</summary>
        /// <param name="item">item</param>
        /// <returns>true if item is added successfully</returns>
        Task<bool> AddAsync(TItem item);

        /// <summary>Add item from transactional list</summary>
        /// <param name="item">item</param>
        /// <returns>true if item is remove successfully</returns>
        Task<bool> RemoveAsync(TItem item);

        /// <summary>Returns the size of the list</summary>
        /// <returns>size</returns>
        Task<int> GetSizeAsync();
    }
}
