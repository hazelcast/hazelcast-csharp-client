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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.FlakeId
{
    [TestFixture]
    public class AutoBatcherOriginalTests
    {
        [TestCase(1, 10)]
        [TestCase(10, 1)]
        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_Concurrent(int batchSize, int batchCount)
        {
            var fetchedBatches = 0;
            int GetNextBatchNumber() => Interlocked.Increment(ref fetchedBatches) - 1;

            var batcher = new AutoBatcherOriginal(() =>
            {
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= batchCount) throw new InvalidOperationException("Overflow.");
                var batch = new Batch(batchNumber * batchSize, 1, batchSize, Timeout.InfiniteTimeSpan);
                return Task.FromResult(batch);
            });

            Task<long> GetNextId(int _) => batcher.GetNextIdAsync().AsTask();

            var ids = await TaskEx.Parallel(GetNextId, batchCount * batchSize);

            Assert.That(fetchedBatches, Is.EqualTo(batchCount));

            var expected = Enumerable.Range(0, batchCount * batchSize).ToList();
            CollectionAssert.AreEquivalent(expected, ids);
        }

        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_SyncException(int batchSize, int batchCount)
        {
            var fetchedBatchCount = 0;
            var failedBatches = new HashSet<int>();
            var mutex = new object();
            var exceptionCount = 0;

            int GetNextBatchNumber()
            {
                lock (mutex)
                {
                    if (failedBatches.Contains(fetchedBatchCount))
                        return fetchedBatchCount++;

                    failedBatches.Add(fetchedBatchCount);
                    throw new Exception($"Fail to get batch #{fetchedBatchCount}.");
                }
            }

            var batcher = new AutoBatcherOriginal(() =>
            {
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= batchCount) throw new InvalidOperationException("Overflow.");
                var batch = new Batch(batchNumber * batchSize, 1, batchSize, Timeout.InfiniteTimeSpan);
                return Task.FromResult(batch);
            });

            async Task<long> GetNextId(int _)
            {
                while (true)
                {
                    try
                    {
                        return await batcher.GetNextIdAsync().CfAwait();
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.StartsWith("Fail to get batch")) throw;
                        Interlocked.Increment(ref exceptionCount);
                    }
                }
            }

            var ids = await TaskEx.Parallel(GetNextId, batchCount * batchSize);

            Assert.That(fetchedBatchCount, Is.EqualTo(batchCount));

            // sync exceptions = each batch fetch has throw and get caught exactly once before succeeding
            // NOTE: can raise more exceptions than needed => test that >=
            Assert.That(exceptionCount, Is.GreaterThanOrEqualTo(batchCount));

            var expected = Enumerable.Range(0, batchCount * batchSize).ToList();
            CollectionAssert.AreEquivalent(expected, ids);
        }

        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_AsyncException(int batchSize, int batchCount)
        {
            var fetchedBatchCount = 0;
            var failedBatches = new HashSet<int>();
            var mutex = new object();
            var exceptionCount = 0;

            int GetNextBatchNumber()
            {
                lock (mutex)
                {
                    if (failedBatches.Contains(fetchedBatchCount))
                        return fetchedBatchCount++;

                    failedBatches.Add(fetchedBatchCount);
                    throw new Exception($"Fail to get batch #{fetchedBatchCount}.");
                }
            }

            var batcher = new AutoBatcherOriginal(async () =>
            {
                await Task.Yield();
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= batchCount) throw new InvalidOperationException("Overflow.");
                var batch = new Batch(batchNumber * batchSize, 1, batchSize, Timeout.InfiniteTimeSpan);
                return batch;
            });

            async Task<long> GetNextId(int _)
            {
                while (true)
                {
                    try
                    {
                        return await batcher.GetNextIdAsync().CfAwait();
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.StartsWith("Fail to get batch")) throw;
                        Interlocked.Increment(ref exceptionCount);
                    }
                }
            }

            var ids = await TaskEx.Parallel(GetNextId, batchCount * batchSize);

            Assert.That(fetchedBatchCount, Is.EqualTo(batchCount));

            // async exceptions = each batch fetch has throw exactly once before succeeding, but
            // potentially the task has been awaited multiple times, so exceptionCount is greater
            Assert.That(exceptionCount, Is.GreaterThanOrEqualTo(batchCount));

            var expected = Enumerable.Range(0, batchCount * batchSize).ToList();
            CollectionAssert.AreEquivalent(expected, ids);
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
    }

    [TestFixture]
    public class AutoBatcher1Tests
    {
        [TestCase(1, 10)]
        [TestCase(10, 1)]
        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_Concurrent(int batchSize, int batchCount)
        {
            var fetchedBatches = 0;
            int GetNextBatchNumber() => Interlocked.Increment(ref fetchedBatches) - 1;

            var batcher = new AutoBatcher1(() =>
            {
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= batchCount) throw new InvalidOperationException("Overflow.");
                var batch = new Batch(batchNumber * batchSize, 1, batchSize, Timeout.InfiniteTimeSpan);
                return Task.FromResult(batch);
            });

            Task<long> GetNextId(int _) => batcher.GetNextIdAsync().AsTask();

            var ids = await TaskEx.Parallel(GetNextId, batchCount * batchSize);

            Assert.That(fetchedBatches, Is.EqualTo(batchCount));

            var expected = Enumerable.Range(0, batchCount * batchSize).ToList();
            CollectionAssert.AreEquivalent(expected, ids);
        }

        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_SyncException(int batchSize, int batchCount)
        {
            var fetchedBatchCount = 0;
            var failedBatches = new HashSet<int>();
            var mutex = new object();
            var exceptionCount = 0;

            int GetNextBatchNumber()
            {
                lock (mutex)
                {
                    if (failedBatches.Contains(fetchedBatchCount))
                        return fetchedBatchCount++;

                    failedBatches.Add(fetchedBatchCount);
                    throw new Exception($"Fail to get batch #{fetchedBatchCount}.");
                }
            }

            var batcher = new AutoBatcher1(() =>
            {
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= batchCount) throw new InvalidOperationException("Overflow.");
                var batch = new Batch(batchNumber * batchSize, 1, batchSize, Timeout.InfiniteTimeSpan);
                return Task.FromResult(batch);
            });

            async Task<long> GetNextId(int _)
            {
                while (true)
                {
                    try
                    {
                        return await batcher.GetNextIdAsync().CfAwait();
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.StartsWith("Fail to get batch")) throw;
                        Interlocked.Increment(ref exceptionCount);
                    }
                }
            }

            var ids = await TaskEx.Parallel(GetNextId, batchCount * batchSize);

            Assert.That(fetchedBatchCount, Is.EqualTo(batchCount));

            // sync exceptions = each batch fetch has throw and get caught exactly once before succeeding
            // NOTE: can raise more exceptions than needed => test that >=
            Assert.That(exceptionCount, Is.EqualTo(batchCount));

            var expected = Enumerable.Range(0, batchCount * batchSize).ToList();
            CollectionAssert.AreEquivalent(expected, ids);
        }

        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_AsyncException(int batchSize, int batchCount)
        {
            var fetchedBatchCount = 0;
            var failedBatches = new HashSet<int>();
            var mutex = new object();
            var exceptionCount = 0;

            int GetNextBatchNumber()
            {
                lock (mutex)
                {
                    if (failedBatches.Contains(fetchedBatchCount))
                        return fetchedBatchCount++;

                    failedBatches.Add(fetchedBatchCount);
                    throw new Exception($"Fail to get batch #{fetchedBatchCount}.");
                }
            }

            var batcher = new AutoBatcher1(async () =>
            {
                await Task.Yield();
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= batchCount) throw new InvalidOperationException("Overflow.");
                var batch = new Batch(batchNumber * batchSize, 1, batchSize, Timeout.InfiniteTimeSpan);
                return batch;
            });

            async Task<long> GetNextId(int _)
            {
                while (true)
                {
                    try
                    {
                        return await batcher.GetNextIdAsync().CfAwait();
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.StartsWith("Fail to get batch")) throw;
                        Interlocked.Increment(ref exceptionCount);
                    }
                }
            }

            var ids = await TaskEx.Parallel(GetNextId, batchCount * batchSize);

            Assert.That(fetchedBatchCount, Is.EqualTo(batchCount));

            // async exceptions = each batch fetch has throw exactly once before succeeding, but
            // potentially the task has been awaited multiple times, so exceptionCount is greater
            Assert.That(exceptionCount, Is.GreaterThanOrEqualTo(batchCount));

            var expected = Enumerable.Range(0, batchCount * batchSize).ToList();
            CollectionAssert.AreEquivalent(expected, ids);
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
    }

    [TestFixture]
    public class AutoBatcherImproved2Tests
    {
        [TestCase(1, 10)]
        [TestCase(10, 1)]
        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_Concurrent(int batchSize, int batchCount)
        {
            var batcher = new AutoBatcherTester(batchCount, batchSize);

            Task<long> GetNextId(int _) => batcher.GetNextIdAsync().AsTask();

            var ids = await TaskEx.Parallel(GetNextId, batchCount * batchSize);

            Assert.That(batcher.FetchedBatchCount, Is.EqualTo(batchCount));

            var expected = Enumerable.Range(0, batchCount * batchSize).ToList();
            CollectionAssert.AreEquivalent(expected, ids);
        }

        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_SyncException(int batchSize, int batchCount)
        {
            var exceptionCount = 0;

            var batcher = new AutoBatcherTesterSyncException(batchCount, batchSize);

            async Task<long> GetNextId(int _)
            {
                while (true)
                {
                    try
                    {
                        return await batcher.GetNextIdAsync().CfAwait();
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.StartsWith("Fail to get batch")) throw;
                        Interlocked.Increment(ref exceptionCount);
                    }
                }
            }

            var ids = await TaskEx.Parallel(GetNextId, batchCount * batchSize);

            Assert.That(batcher.FetchedBatchCount, Is.EqualTo(batchCount));

            // sync exceptions = each batch fetch has throw and get caught exactly once before succeeding
            Assert.That(exceptionCount, Is.EqualTo(batchCount));

            var expected = Enumerable.Range(0, batchCount * batchSize).ToList();
            CollectionAssert.AreEquivalent(expected, ids);
        }

        [TestCase(5, 5)]
        [TestCase(100, 10)]
        public async Task GetNextId_AsyncException(int batchSize, int batchCount)
        {
            var exceptionCount = 0;

            var batcher = new AutoBatcherTesterAsyncException(batchCount, batchSize);

            async Task<long> GetNextId(int _)
            {
                while (true)
                {
                    try
                    {
                        return await batcher.GetNextIdAsync().CfAwait();
                    }
                    catch (Exception e)
                    {
                        if (!e.Message.StartsWith("Fail to get batch")) throw;
                        Interlocked.Increment(ref exceptionCount);
                    }
                }
            }

            var ids = await TaskEx.Parallel(GetNextId, batchCount * batchSize);

            Assert.That(batcher.FetchedBatchCount, Is.EqualTo(batchCount));

            // async exceptions = each batch fetch has throw exactly once before succeeding, but
            // potentially the task has been awaited multiple times, so exceptionCount is greater
            Assert.That(exceptionCount, Is.GreaterThanOrEqualTo(batchCount));

            var expected = Enumerable.Range(0, batchCount * batchSize).ToList();
            CollectionAssert.AreEquivalent(expected, ids);
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

        internal sealed class AutoBatcherTesterSyncException : AutoBatcherBase
        {
            private readonly int _batchCount, _batchSize;
            private readonly object _mutex = new object();
            private readonly HashSet<int> _failedBatches = new HashSet<int>();
            private int _fetchedBatchCount;

            public AutoBatcherTesterSyncException(int batchCount, int batchSize)
            {
                _batchCount = batchCount;
                _batchSize = batchSize;
            }

            public int FetchedBatchCount => _fetchedBatchCount;

            private int GetNextBatchNumber()
            {
                lock (_mutex)
                {
                    if (_failedBatches.Contains(_fetchedBatchCount))
                        return _fetchedBatchCount++;

                    _failedBatches.Add(_fetchedBatchCount);
#pragma warning disable CA2201 // Do not raise reserved exception types - ok here
                    throw new Exception($"Fail to get batch #{_fetchedBatchCount}.");
#pragma warning restore CA2201
                }
            }

            protected override Task<Batch> FetchBatch()
            {
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= _batchCount) throw new InvalidOperationException("Overflow.");
                var batch = new Batch(batchNumber * _batchSize, 1, _batchSize, Timeout.InfiniteTimeSpan);
                return Task.FromResult(batch);
            }
        }

        internal sealed class AutoBatcherTesterAsyncException : AutoBatcherBase
        {
            private readonly int _batchCount, _batchSize;
            private readonly object _mutex = new object();
            private readonly HashSet<int> _failedBatches = new HashSet<int>();
            private int _fetchedBatchCount;

            public AutoBatcherTesterAsyncException(int batchCount, int batchSize)
            {
                _batchCount = batchCount;
                _batchSize = batchSize;
            }

            public int FetchedBatchCount => _fetchedBatchCount;

            private int GetNextBatchNumber()
            {
                lock (_mutex)
                {
                    if (_failedBatches.Contains(_fetchedBatchCount))
                        return _fetchedBatchCount++;

                    _failedBatches.Add(_fetchedBatchCount);
#pragma warning disable CA2201 // Do not raise reserved exception types - ok here
                    throw new Exception($"Fail to get batch #{_fetchedBatchCount}.");
#pragma warning restore CA2201
                }
            }

            protected override async Task<Batch> FetchBatch()
            {
                await Task.Yield();
                var batchNumber = GetNextBatchNumber();
                if (batchNumber >= _batchCount) throw new InvalidOperationException("Overflow.");
                var batch = new Batch(batchNumber * _batchSize, 1, _batchSize, Timeout.InfiniteTimeSpan);
                return batch;
            }
        }

        [Test]
        public async Task TestConcurrentFetching()
        {
            var batcher = new AutoBatcherTester2();

            var fetch0 = batcher.GetNextIdAsync();
            var fetch1 = batcher.GetNextIdAsync();

            await Task.Delay(500);

            // by now both fetch0 and fetch1 should be blocked waiting for the batcher

            batcher.Release();

            var success0 = false;
            try
            {
                await fetch0;
                success0 = true;
            }
            catch { /* don't care */ }

            var success1 = false;
            try
            {
                await fetch1;
                success1 = true;
            }
            catch { /* don't care */ }

            // 1 of them should fail - but not both
            Assert.That(success0 ? !success1 : success1);
        }

        internal sealed class AutoBatcherTester2 : AutoBatcherBase
        {
            private readonly SemaphoreSlim _semaphore = new(0, 1);
            private int _count;

            public void Release() => _semaphore.Release();

            protected override async Task<Batch> FetchBatch()
            {
                await _semaphore.WaitAsync();
                try
                {
                    if (_count++ == 0) throw new Exception("bang");
                    return new Batch(0, 1, 100, TimeSpan.FromMinutes(1));
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }
    }
}
