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
using Hazelcast.Core;

namespace Hazelcast.DistributedObjects.Implementation.Collection
{
    internal partial class HCollectionBase<T> // Setting
    {
        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<bool> AddAsync(T item, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = AddAsync(item, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public abstract Task<bool> AddAsync(T item, CancellationToken cancellationToken);

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items, TimeSpan timeout = default)
            where TItem : T
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = AddRangeAsync(items, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public abstract Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken) where TItem : T;
    }
}