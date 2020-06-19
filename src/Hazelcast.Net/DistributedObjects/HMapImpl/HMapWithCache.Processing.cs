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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.HMapImpl
{
    internal partial class HMapWithCache<TKey, TValue> // Processing
    {
        // <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<object> ExecuteAsync(IData processorData, IData keyData, CancellationToken cancellationToken)
        {
            var task = base.ExecuteAsync(processorData, keyData, cancellationToken).ContinueWith(t =>
            {
                _cache.Invalidate(keyData);
                return t;
            }, default, default, TaskScheduler.Current);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<object> ApplyAsync(IData processorData, IData keyData, CancellationToken cancellationToken)
        {
            var task = base.ApplyAsync(processorData, keyData, cancellationToken).ContinueWith(t =>
            {
                _cache.Invalidate(keyData);
                return t;
            }, default, default, TaskScheduler.Current);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }
    }
}
