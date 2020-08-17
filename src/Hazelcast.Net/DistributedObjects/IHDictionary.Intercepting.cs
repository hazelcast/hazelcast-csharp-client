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

namespace Hazelcast.DistributedObjects
{
    // ReSharper disable UnusedTypeParameter
    public partial interface IHDictionary<TKey, TValue> // Interception
    // ReSharper restore NonReadonlyMemberInGetHashCode
    {
        // TODO what is an interceptor?

        /// <summary>
        /// Adds an interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <returns>The interceptor unique identifier.</returns>
        Task<string> AddInterceptorAsync(IDictionaryInterceptor interceptor);

        /// <summary>
        /// Removes an interceptor.
        /// </summary>
        /// <param name="id">The identifier of the interceptor.</param>
        /// <returns>A task that will complete when the interceptor has been removed.</returns>
        Task RemoveInterceptorAsync(string id);
    }
}
