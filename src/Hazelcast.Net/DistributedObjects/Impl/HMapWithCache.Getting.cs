// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HMapWithCache<TKey, TValue> // Getting
    {
        /// <inheritdoc />
        protected override async Task<TValue> GetAsync(IData keyData, CancellationToken cancellationToken)
        {
            var fromCache = await _cache.TryGetOrAddAsync(keyData, _ => GetDataAsync(keyData, cancellationToken)).CfAwait();
            return fromCache.ValueOrDefault();
        }

        /// <inheritdoc />
        protected override async Task<ReadOnlyLazyDictionary<TKey, TValue>> GetAllAsync(Dictionary<Guid, Dictionary<int, List<IData>>> ownersKeys, CancellationToken cancellationToken)
        {
            // owner keys are grouped by owners (members) and partitions

            var cachedValues = new Dictionary<IData, TValue>();
            var cachedKeys = new List<IData>();

            foreach (var (_, ownerKeys) in ownersKeys)
            {
                foreach (var (_, partitionKeys) in ownerKeys)
                {
                    cachedKeys.Clear();
                    foreach (var keyData in partitionKeys)
                    {
                        var (hasValue, value) = await _cache.TryGetAsync(keyData).CfAwait();
                        if (hasValue)
                        {
                            cachedKeys.Add(keyData);
                            cachedValues[keyData] = value;
                        }
                    }

                    foreach (var keyData in cachedKeys)
                        partitionKeys.Remove(keyData);
                }
            }

            var entries = await base.GetAllAsync(ownersKeys, cancellationToken).CfAwait();

            // 'entries' is a ReadOnlyLazyDictionary that contains values that are lazily
            // deserialized - and since we just fetched the entries, none have been deserialized
            // yet - so entry.ValueObject here is not null, and is an IData that we can pass
            // to the cache - which will either deserialized or not depending on InMemoryFormat

            // cache retrieved entries
            foreach (var (key, entry) in entries.Entries)
                await _cache.TryAddAsync(key, entry.ValueData).CfAwait();

            // merge cached entries and retrieved entries
            foreach (var (keyData, valueObject) in cachedValues)
                entries.Add(keyData, valueObject);

            return entries;
        }

        /// <inheritdoc />
        protected override async Task<bool> ContainsKeyAsync(IData keyData, CancellationToken cancellationToken)
        {
            return await _cache.ContainsKeyAsync(keyData).CfAwait() || await base.ContainsKeyAsync(keyData, cancellationToken).CfAwait();
        }
    }
}
