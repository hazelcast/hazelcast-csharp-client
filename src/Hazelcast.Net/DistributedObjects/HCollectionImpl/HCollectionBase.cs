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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.HCollectionImpl
{
    internal abstract partial class HCollectionBase<T> : DistributedObjectBase, IHCollection<T>
    {
        protected HCollectionBase(string serviceName, string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(serviceName, name, cluster, serializationService, loggerFactory)
        { }

        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task CopyToAsync(T[] array, int arrayIndex, TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(CopyToAsync, array, arrayIndex, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        // usage? just define it on the read-only list?
        public async Task CopyToAsync(T[] array, int index, CancellationToken cancellationToken)
        {
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var items = await GetAllAsync(cancellationToken).CAF();

            if (array.Length - index < items.Count)
                throw new ArgumentException("The number of elements in the source array is greater than the available number of elements from index to the end of the destination array.");

            foreach (var item in items)
                array[index++] = item;
        }

        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T[]> ToArrayAsync(TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(ToArrayAsync, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // usage? going to allocate for no reason
        public async Task<T[]> ToArrayAsync(CancellationToken cancellationToken)
        {
            return (await GetAllAsync(cancellationToken).CAF()).ToArray();
        }

        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<TItem[]> ToArrayAsync<TItem>(TItem[] array, TimeSpan timeout = default)
            where TItem : T
        {
            var task = TaskEx.WithTimeout(ToArrayAsync, array, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // usage? going to allocate for no reason
        public async Task<TItem[]> ToArrayAsync<TItem>(TItem[] array, CancellationToken cancellationToken)
            where TItem : T
        {
            var items = await GetAllAsync(cancellationToken).CAF();

            if (array == null || array.Length < items.Count)
                return items.Cast<TItem>().ToArray();

            var index = 0;

            foreach (var item in items)
                array[index++] = (TItem)item;

            for (; index < array.Length; index++)
                array[index] = default;

            return array;
        }
    }
}