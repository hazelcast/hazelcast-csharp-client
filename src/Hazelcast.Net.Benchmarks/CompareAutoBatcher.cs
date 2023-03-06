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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Hazelcast.Core;
using Hazelcast.DistributedObjects.Impl;

namespace Hazelcast.Benchmarks
{
    // compares AutoBatcher implementation
    // original version relies on Lazy<> and can allocate... tried Improved1 and then went for the Final version
    // numbers:
    //
    // |    Method |     Mean |   Error |  StdDev |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
    // |---------- |---------:|--------:|--------:|--------:|-------:|------:|----------:|
    // |  Original | 495.0 us | 2.54 us | 2.38 us | 27.3438 |      - |     - | 112.87 KB |
    // | Improved1 | 453.1 us | 2.71 us | 2.26 us | 12.2070 | 0.4883 |     - |  51.03 KB |
    // |     Final | 445.5 us | 1.61 us | 1.35 us | 10.7422 | 0.4883 |     - |  43.68 KB |
    //
    // so the final version is slightly faster, and divides allocations by at least 2

    public class CompareAutoBatcher
    {
        private const int BatchSize = 100;
        private const int BatchCount = 100;

        public Task GetAllIds(int batchSize, int batchCount, Func<ValueTask<long>> getNextId)
        {
            var starter = new TaskCompletionSource<object>();
            var tasks = new List<Task>(batchCount);
            for (var i = 0; i < batchCount; i++) tasks.Add(Task.Run(async() =>
            {
                await starter.Task;
                for (var j = 0; j < batchSize; j++) await getNextId().CfAwait();
            }));
            starter.SetResult(null);
            return Task.WhenAll(tasks);
        }

        [Benchmark]
        public async Task Original()
        {
            var fetchedBatches = 0;

            int GetNextBatchNumber() => Interlocked.Increment(ref fetchedBatches) - 1;

            var batcher = new AutoBatcherOriginal(() =>
            {
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= BatchCount) throw new IndexOutOfRangeException();
                var batch = new Batch(batchNumber * BatchSize, 1, BatchSize, Timeout.InfiniteTimeSpan);
                return Task.FromResult(batch);
            });

            await GetAllIds(BatchSize, BatchCount, () => batcher.GetNextIdAsync()).CfAwait();
        }

        private class AutoBatcherOriginal
        {
            private readonly Func<Task<Batch>> _supplier;

            private Lazy<Task<Batch>> _nextBatchLazyTask;

            public AutoBatcherOriginal(Func<Task<Batch>> supplier)
            {
                _supplier = supplier;
                _nextBatchLazyTask = NewBatchLazyTask();
            }

            public ValueTask<long> GetNextIdAsync(CancellationToken cancellationToken = default)
            {
                // Avoid using async state machine if possible
                var nextBatchTask = _nextBatchLazyTask.Value;
                if (nextBatchTask.IsCompletedSuccessfully() && nextBatchTask.Result.TryGetNextId(out var id))
                    return new ValueTask<long>(id);

                return GetNextIdInternalAsync(cancellationToken);
            }

            private async ValueTask<long> GetNextIdInternalAsync(CancellationToken cancellationToken)
            {
                while (true) // If batch is finished, get next and repeat the process
                {
                    var nextBatchLazyTask = _nextBatchLazyTask;
                    var nextBatchTask = nextBatchLazyTask.Value;

                    cancellationToken.ThrowIfCancellationRequested();
                    await nextBatchTask.CfAwaitNoThrow();

                    if (nextBatchTask.IsCompletedSuccessfully() && nextBatchTask.Result.TryGetNextId(out var id))
                        return id;

                    // Set new task only if it didn't change during method execution
                    if (_nextBatchLazyTask == nextBatchLazyTask)
                        Interlocked.CompareExchange(ref _nextBatchLazyTask, NewBatchLazyTask(), nextBatchLazyTask);

                    // This ensures any exception is forwarded to the caller
                    // but does it AFTER lazy task is updated to fetch the next batch
                    // to avoid state being stuck on exception
                    await nextBatchTask;
                }
            }

            // Async/await wrapping instead of just passing '_supplier' is needed
            // to ensure exception is thrown in 'await Value' stage, not when calling 'Value'
            private Lazy<Task<Batch>> NewBatchLazyTask() => new Lazy<Task<Batch>>(async () => await _supplier().CfAwait());
        }

        [Benchmark]
        public async Task Improved1()
        {
            var fetchedBatches = 0;

            int GetNextBatchNumber() => Interlocked.Increment(ref fetchedBatches) - 1;

            var batcher = new AutoBatcher1(() =>
            {
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= BatchCount) throw new IndexOutOfRangeException();
                var batch = new Batch(batchNumber * BatchSize, 1, BatchSize, Timeout.InfiniteTimeSpan);
                return Task.FromResult(batch);
            });

            await GetAllIds(BatchSize, BatchCount, () => batcher.GetNextIdAsync()).CfAwait();
        }

        private class AutoBatcher1
        {
            private readonly Func<Task<Batch>> _fetchBatch;
            private readonly object _mutex = new object();

            private volatile Task<Batch> _fetchingBatch;
            private volatile Batch _batch;

            public AutoBatcher1(Func<Task<Batch>> fetchBatch)
            {
                _fetchBatch = fetchBatch;
            }

            public ValueTask<long> GetNextIdAsync(CancellationToken cancellationToken = default)
            {
                // synchronously return next identifier if possible, else trigger the async operation

                var batch = _batch;
                return batch != null && batch.TryGetNextId(out var id)
                    ? new ValueTask<long>(id)
                    : GetNextIdAsync2(cancellationToken);
            }

            // async method that returns a batch *and* assigns _batch - this is important: because _batch is assigned
            // by the fetching task, it's assigned only once and always assigned before tha task completes, thus
            // avoiding having to lock
            private async Task<Batch> FetchBatch()
            {
                return _batch = await _fetchBatch().CfAwait();
            }

            private async ValueTask<long> GetNextIdAsync2(CancellationToken cancellationToken)
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fetchingBatch = _fetchingBatch;

                    if (fetchingBatch != null)
                    {
                        // await fetchingBatch - this may throw, in which case we raise the exception to the
                        // caller, but before that we make sure to clear _fetchingBatch so that FetchBatch()
                        // will be tried again next time - also, ensure we raise only once (i.e. only to the
                        // original caller) through _mutex
                        try
                        {
                            var batch = await fetchingBatch.CfAwait();
                            if (batch.TryGetNextId(out var id)) return id;
                        }
                        catch
                        {
                            lock (_mutex) if (_fetchingBatch == fetchingBatch) { _fetchingBatch = null; throw; }
                        }
                    }

                    lock (_mutex)
                    {
                        // if no other thread has updated _fetchingBatch yet, do it - calling _fetchBatch() may
                        // throw immediately, in which case we raise the exception to the caller but before that
                        // we make sure to clear _fetchingBatch
                        if (_fetchingBatch != fetchingBatch) continue;

                        try
                        {
                            _fetchingBatch = FetchBatch();
                        }
                        catch
                        {
                            _fetchingBatch = null;
                            throw;
                        }
                    }
                }
            }
        }

        [Benchmark]
        public async Task Final()
        {
            var batcher = new AutoBatcherTester(BatchCount, BatchSize);

            await GetAllIds(BatchSize, BatchCount, () => batcher.GetNextIdAsync()).CfAwait();
        }

        internal sealed class AutoBatcherTester : AutoBatcherBase
        {
            private readonly int _batchCount, _batchSize;
            private int _fetchedBatchCount;

            public AutoBatcherTester(int batchCount, int batchSize)
            {
                _batchCount = batchCount;
                _batchSize = batchSize;
            }

            public int FetchedBatchCount => _fetchedBatchCount;

            private int GetNextBatchNumber()
            {
                return Interlocked.Increment(ref _fetchedBatchCount) - 1;
            }

            protected override Task<Batch> FetchBatch()
            {
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= _batchCount) throw new InvalidOperationException("Overflow.");
                var batch = new Batch(batchNumber * _batchSize, 1, _batchSize, Timeout.InfiniteTimeSpan);
                return Task.FromResult(batch);
            }
        }
    }
}
