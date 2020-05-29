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
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    // ReSharper disable twice UnusedTypeParameter
    public partial interface IMap<TKey, TValue> // Interception
    {
        // TODO what is an interceptor?

        /// <summary>
        /// Adds an interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The interceptor unique identifier.</returns>
        Task<string> AddInterceptorAsync(IMapInterceptor interceptor, TimeSpan timeout = default);

        /// <summary>
        /// Adds an interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The interceptor unique identifier.</returns>
        Task<string> AddInterceptorAsync(IMapInterceptor interceptor, CancellationToken cancellationToken);

        /// <summary>
        /// Removes an interceptor.
        /// </summary>
        /// <param name="id">The identifier of the interceptor.</param>
        /// <param name="timeout">A timeout.</param>
        Task RemoveInterceptorAsync(string id, TimeSpan timeout = default);

        /// <summary>
        /// Removes an interceptor.
        /// </summary>
        /// <param name="id">The identifier of the interceptor.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task RemoveInterceptorAsync(string id, CancellationToken cancellationToken);
    }
}
