// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Transaction;

namespace Hazelcast.Core
{
    /// <summary>
    /// Transactional implementation of <see cref="IHList{E}">IHList&lt;E&gt;</see>
    /// </summary>
    public interface ITransactionalList<E> : ITransactionalObject
    {
        /// <summary>Add new item to transactional list</summary>
        /// <param name="e">item</param>
        /// <returns>true if item is added successfully</returns>
        bool Add(E e);

        /// <summary>Add item from transactional list</summary>
        /// <param name="e">item</param>
        /// <returns>true if item is remove successfully</returns>
        bool Remove(E e);

        /// <summary>Returns the size of the list</summary>
        /// <returns>size</returns>
        int Size();
    }
}