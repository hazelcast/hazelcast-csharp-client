// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.FlakeId
{
    internal class AutoBatcher
    {
        private readonly Func<Task<Batch>> _supplier;

        private Lazy<Task<Batch>> _nextBatchLazyTask;

        public AutoBatcher(Func<Task<Batch>> supplier)
        {
            _supplier = supplier;
            _nextBatchLazyTask = NewBatchLazyTask();
        }

        public async Task<long> GetNextIdAsync(CancellationToken cancellationToken = default)
        {
            while (true) // If batch is finished, repeat the process with the next batch
            {
                var nextBatchLazyTask = _nextBatchLazyTask;
                var nextBatchTask = nextBatchLazyTask.Value;

                cancellationToken.ThrowIfCancellationRequested();
                await nextBatchTask.CfAwaitNoThrow();

                if (nextBatchTask.IsCompletedSuccessfully() && nextBatchTask.Result.TryGetNextId(out var id))
                    return id;

                // Set new task only if it didn't change during method execution
                Interlocked.CompareExchange(ref _nextBatchLazyTask, NewBatchLazyTask(), nextBatchLazyTask);

                // Ensure any exception is forwarded to the caller
                // But only AFTER we changed lazy invocation to uses next batch, to avoid being stuck on exception
                await nextBatchTask;
            }
        }

        // Async/await wrapping instead of just passing supplier is needed
        // to ensure exception is thrown in 'await Value' stage, not when calling 'Value'
        private Lazy<Task<Batch>> NewBatchLazyTask() => new Lazy<Task<Batch>>(async () => await _supplier());
    }
}
