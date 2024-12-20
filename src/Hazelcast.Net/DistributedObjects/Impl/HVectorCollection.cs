// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Clustering;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
namespace Hazelcast.DistributedObjects.Impl
{
    internal class HVectorCollection<TKey, TVal> : DistributedObjectBase, IHVectorCollection<TKey, TVal>
    {

        public HVectorCollection(string name,
            DistributedObjectFactory factory,
            Cluster cluster,
            SerializationService serializationService,
            ILoggerFactory loggerFactory)
            : base(ServiceNames.VectorCollection, name, factory, cluster, serializationService, loggerFactory)
        { }

        public Task<VectorDocument<TVal>> GetAsync(TKey key)
            => throw new System.NotImplementedException();
        public Task<VectorDocument<TVal>> PutAsync(TKey key, VectorDocument<TVal> valueVectorDocument)
            => throw new System.NotImplementedException();
        public Task SetAsync(TKey key, VectorDocument<TVal> vectorDocument)
            => throw new System.NotImplementedException();
        public Task<VectorDocument<TVal>> PutIfAbsentAsync(TKey key, VectorDocument<TVal> vectorDocument)
            => throw new System.NotImplementedException();
        public Task PutAllAsync(IDictionary<TKey, VectorDocument<TVal>> vectorDocumentMap)
            => throw new System.NotImplementedException();
        public Task<VectorDocument<TVal>> RemoveAsync(TKey key)
            => throw new System.NotImplementedException();
        public Task OptimizeAsync()
            => throw new System.NotImplementedException();
        public Task OptimizeAsync(string indexName)
            => throw new System.NotImplementedException();
        public Task ClearAsync()
            => throw new System.NotImplementedException();
        public Task<long> GetSizeAsync()
            => throw new System.NotImplementedException();
        public Task<IVectorSearchResult<TKey, TVal>> SearchAsync(VectorValues vectorValues, VectorSearchOptions searchOptions)
            => throw new System.NotImplementedException();
    }
}
