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
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects.HMapImpl
{
    internal partial class HMapWithCache<TKey, TValue> // Getting
    {
        /// <inheritdoc />
        protected override async Task<IData> GetAsync(IData keyData, CancellationToken cancellationToken)
        {
            async Task<object> BaseGetAsync(IData data, CancellationToken token)
                => await base.GetAsync(data, token).CAF();

            try
            {
                var attempt = await _cache.TryGetOrAddAsync(keyData, data => BaseGetAsync(keyData, cancellationToken)).CAF();
                return (IData)attempt.ValueOr(default);
            }
            catch
            {
                _cache.Invalidate(keyData);
                throw;
            }
        }

        /// <inheritdoc />
        protected override async Task<ReadOnlyLazyDictionary<TKey, TValue>> GetAsync(Dictionary<Guid, Dictionary<int, List<IData>>> ownerKeys, CancellationToken cancellationToken)
        {
            var cachedEntries = new Dictionary<IData, object>();

            foreach (var (_, part) in ownerKeys)
            {
                foreach (var (_, list) in part)
                {
                    var remove = new List<IData>();
                    foreach (var key in list)
                    {
                        var (hasValue, value) = await _cache.TryGetValue(key).CAF();
                        if (hasValue)
                        {
                            remove.Add(key);
                            cachedEntries[key] = value;
                        }
                    }

                    foreach (var key in remove)
                        list.Remove(key);
                }
            }

            var entries = await base.GetAsync(ownerKeys, cancellationToken).CAF();

            // _cache may contain either the value data (IData) or the
            // de-serialized object (TValue), depending on configuration

            // cache the retrieved entries
            // none of them have a value yet, and ...
            // FIXME what is it we want to put in the cache?
            foreach (var (key, entry) in entries.Entries)
                await _cache.TryAdd(key, entry.ValueObject).CAF();

            // add cached entries to the retrieved entries
            foreach (var (key, valueObject) in cachedEntries)
                entries.Add(key, valueObject);

            return entries;
        }

        /// <inheritdoc />
        protected override async Task<bool> ContainsKeyAsync(IData keyData, CancellationToken cancellationToken)
        {
            return await _cache.ContainsKey(keyData).CAF() || await base.ContainsKeyAsync(keyData, cancellationToken).CAF();
        }
    }
}
