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
using Hazelcast.Models;
namespace Hazelcast.DistributedObjects
{
    public interface IHVectorCollection<TKey, TValue>:IDistributedObject
    {
        Task<VectorDocument<TValue>> GetAsync(TKey key);
        Task<VectorDocument<TValue>> PutAsync(TKey key, VectorDocument<TValue> valueVectorDocument);
        Task<VectorDocument<TValue>> SetAsync(TKey key, VectorDocument<TValue> vectorDocument);
        Task<VectorDocument<TValue>> PutIfAbsentAsync(TKey key, VectorDocument<TValue> vectorDocument);
        Task<VectorDocument<TValue>> PutAllAsync(IDictionary<TKey, VectorDocument<TValue>> vectorDocumentMap);
        Task<VectorDocument<TValue>> RemoveAsync(TKey key);
        Task OptimizeAsync(TKey key);
        Task ClearAsync();
        Task<long> GetSizeAsync();
        Task<IVectorSearchResult<TKey, TValue>> SearchAsync(VectorValues vectorValues, VectorSearchOptions searchOptions);
    }
}
