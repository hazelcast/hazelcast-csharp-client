// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal abstract partial class HCollectionBase<T> : DistributedObjectBase, IHCollection<T>
    {
        protected HCollectionBase(string serviceName, string name, DistributedObjectFactory factory, Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory)
            : base(serviceName, name, factory, cluster, serializationService, loggerFactory)
        { }

        /// <inheritdoc />
        public virtual async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            // all collections are async enumerable,
            // but by default we load the whole items set at once,
            // then iterate in memory
            var items = await GetAllAsync().CfAwait();
            foreach (var item in items)
                yield return item;
        }
    }
}
