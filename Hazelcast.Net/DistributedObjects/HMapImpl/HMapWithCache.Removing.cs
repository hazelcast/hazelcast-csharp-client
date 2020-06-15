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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.HMapImpl
{
    internal partial class HMapWithCache<TKey, TValue> // Removing
    {
        /// <inheritdoc />
        protected override async Task<bool> TryRemoveAsync(IData keyData, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var removed = await base.TryRemoveAsync(keyData, serverTimeout, cancellationToken).CAF();
            if (removed) _cache.Invalidate(keyData);
            return removed;
        }

        /// <inheritdoc />
        protected override async Task<TValue> RemoveAsync(IData keyData, CancellationToken cancellationToken)
        {
            try
            {
                return await base.RemoveAsync(keyData, cancellationToken).CAF();
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task<bool> RemoveAsync(IData keyData, IData valueData, CancellationToken cancellationToken)
        {
            try
            {
                return await base.RemoveAsync(keyData, valueData, cancellationToken).CAF();
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        protected override async Task DeleteAsync(IData keyData, CancellationToken cancellationToken)
        {
            try
            {
                await base.RemoveAsync(keyData, cancellationToken).CAF();
            }
            finally
            {
                _cache.Invalidate(keyData);
            }
        }

        /// <inheritdoc />
        public override async Task ClearAsync(CancellationToken cancellationToken)
        {
            await base.ClearAsync(cancellationToken)
                .ContinueWith(_ => _cache.InvalidateAll(), default, default, TaskScheduler.Current)
                .CAF();
        }
    }
}