﻿// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
namespace Hazelcast.Models
{
    class VectorSearchResult<TKey, TVal> : IVectorSearchResult<TKey, TVal>
    {
        private IEnumerable<VectorSearchResultEntry<TKey, TVal>> _results;

        public VectorSearchResult(int size, IEnumerable<VectorSearchResultEntry<TKey, TVal>> results)
        {
            Size = size;
            _results = results;
        }

        //<inheritdoc/>
        public int Size { get; }
        //<inheritdoc/>
        public IEnumerable<VectorSearchResultEntry<TKey, TVal>> Results => _results;
    }
}
