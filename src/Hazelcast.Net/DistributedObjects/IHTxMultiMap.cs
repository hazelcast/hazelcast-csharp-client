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
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>Transactional implementation of MultiMap</summary>
    public interface IHTxMultiMap<TKey, TValue> : ITransactionalObject
    {
        Task<IReadOnlyCollection<TValue>> GetAsync(TKey key);

        Task<bool> PutAsync(TKey key, TValue value);

        Task<bool> RemoveAsync(TKey key, TValue value);

        Task<IReadOnlyCollection<TValue>> RemoveAsync(TKey key);

        Task<int> GetSizeAsync();

        Task<int> GetValueCountAsync(TKey key);
    }
}
