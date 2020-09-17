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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal abstract partial class HCollectionBase<T> : DistributedObjectBase, IHCollection<T>
    {
        protected HCollectionBase(string serviceName, string name, DistributedObjectFactory factory, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(serviceName, name, factory, cluster, serializationService, loggerFactory)
        { }

        // usage? just define it on the read-only list?
        public async Task CopyToAsync(T[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var items = await GetAsync().CAF();

            if (array.Length - index < items.Count)
                throw new ArgumentException("The number of elements in the source array is greater than the available number of elements from index to the end of the destination array.");

            foreach (var item in items)
                array[index++] = item;
        }

        // usage? going to allocate for no reason
        public async Task<T[]> ToArrayAsync()
        {
            return (await GetAsync().CAF()).ToArray();
        }

        // usage? going to allocate for no reason
        public async Task<TItem[]> ToArrayAsync<TItem>(TItem[] array)
            where TItem : T
        {
            var items = await GetAsync().CAF();

            if (array == null || array.Length < items.Count)
                return items.Cast<TItem>().ToArray();

            var index = 0;

            foreach (var item in items)
                array[index++] = (TItem)item;

            for (; index < array.Length; index++)
                array[index] = default;

            return array;
        }

        /// <inheritdoc />
        public virtual async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            // all collections are async enumerable,
            // but by default we load the whole items set at once,
            // then iterate in memory
            var items = await GetAsync().CAF();
            foreach (var item in items)
                yield return item;
        }
    }
}
