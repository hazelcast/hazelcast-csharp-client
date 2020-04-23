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

using System.Collections;
using System.Collections.Generic;
using Hazelcast.Serialization;

namespace Hazelcast.Core.Collections
{
    // FIXME thread-safety & tests
    // FIXME document
    internal sealed class ReadOnlyLazyList<TValue, T> : IReadOnlyList<TValue>
    {
        private readonly List<CacheEntry<TValue, T>> _content = new List<CacheEntry<TValue, T>>();
        private readonly ISerializationService _serializationService;

        public ReadOnlyLazyList(IEnumerable<T> values, ISerializationService serializationService)
        {
            _serializationService = serializationService;
            foreach (var value in values)
                _content.Add(new CacheEntry<TValue, T> { Source = value });
        }

        private void EnsureValue(CacheEntry<TValue, T> cacheEntry)
        {
            if (cacheEntry.HasValue) return;

            cacheEntry.Value = _serializationService.ToObject<TValue>(cacheEntry.Source);
            cacheEntry.HasValue = true;
            cacheEntry.Source = default;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var cacheEntry in _content)
            {
                EnsureValue(cacheEntry);
                yield return cacheEntry.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _content.Count;

        public TValue this[int index]
        {
            get
            {
                var cacheEntry = _content[index];
                EnsureValue(cacheEntry);
                return cacheEntry.Value;
            }
        }
    }
}