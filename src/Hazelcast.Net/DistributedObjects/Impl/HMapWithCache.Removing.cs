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

using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Query;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HMapWithCache<TKey, TValue> // Removing
    {
        /// <inheritdoc />
        protected override async Task<bool> TryRemoveAsync(IData keyData, TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var removed = await base.TryRemoveAsync(keyData, timeToWait, cancellationToken).CfAwait();
            if (removed) _cache.Remove(keyData);
            return removed;
        }

        /// <inheritdoc />
        protected override async Task<TValue> GetAndRemoveAsync(IData keyData, CancellationToken cancellationToken)
        {
            try
            {
                return await base.GetAndRemoveAsync(keyData, cancellationToken).CfAwait();
            }
            finally
            {
                _cache.Remove(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task<bool> RemoveAsync(IData keyData, IData valueData, CancellationToken cancellationToken)
        {
            try
            {
                return await base.RemoveAsync(keyData, valueData, cancellationToken).CfAwait();
            }
            finally
            {
                _cache.Remove(keyData);
            }
        }

        /// <inheritdoc />
        protected override Task RemoveAsync(IPredicate predicate, CancellationToken cancellationToken)
        {
            try
            {
                return base.RemoveAsync(predicate, cancellationToken);
            }
            finally
            {
                // not exactly pretty, but we cannot run the predicate locally
                _cache.Clear();
            }
        }

        /// <inheritdoc />
        protected override async Task RemoveAsync(IData keyData, CancellationToken cancellationToken)
        {
            try
            {
                await base.GetAndRemoveAsync(keyData, cancellationToken).CfAwait();
            }
            finally
            {
                _cache.Remove(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task ClearAsync(CancellationToken cancellationToken)
        {
            await base.ClearAsync(cancellationToken)
                .ContinueWith(_ => _cache.Clear(), default, default, TaskScheduler.Current)
                .CfAwait();
        }
    }
}
