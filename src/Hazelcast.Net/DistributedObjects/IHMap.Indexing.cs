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

using System.Threading.Tasks;
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    // ReSharper disable UnusedTypeParameter
    public partial interface IHMap<TKey, TValue> // Indexing
    // ReSharper restore NonReadonlyMemberInGetHashCode
    {

        /// <summary>
        /// Adds an index to this dictionary for the specified entries so that queries can run faster.
        /// </summary>
        /// <param name="indexConfig">Index options.</param>
        /// <returns>A task that will complete when the index added.</returns>
        Task AddIndexAsync(IndexOptions indexOptions);

        /// <summary>
        /// Convenient method to add an index to this dictionary with the given type and attributes.
        /// Attributes are indexed in ascending order.
        /// </summary>
        /// <param name="type">Index type.</param>
        /// <param name="attributes">Attributes to be indexed.</param>
        /// <returns>A task that will complete when the index added.</returns>
        Task AddIndexAsync(IndexType type, params string[] attributes);
    }
}
