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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.DistributedObjects.Impl
{
    internal abstract class AutoBatcherBase
    {
        private readonly object _mutex = new object();

        private volatile Task<Batch> _fetchingBatch;
        private volatile Batch _batch;

        public ValueTask<long> GetNextIdAsync(CancellationToken cancellationToken = default)
        {
            // synchronously return next identifier if possible, else trigger the async operation

            var batch = _batch;
            return batch != null && batch.TryGetNextId(out var id)
                ? new ValueTask<long>(id)
                : FetchBatchAndGetNextIdAsync(cancellationToken);
        }

        // async methods that returns a batch
        protected abstract Task<Batch> FetchBatch();

        // async method that returns a batch *and* assigns _batch through SetBatch() - this is important: because
        // _batch is assigned by the fetching task, it's assigned only once and always assigned before tha task
        // completes, thus avoiding having to lock
        private async Task<Batch> FetchAndSetBatch()
        {
            return _batch = await FetchBatch().CfAwait();
        }

        private async ValueTask<long> FetchBatchAndGetNextIdAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fetchingBatch = _fetchingBatch;

                if (fetchingBatch != null) // are we already fetching?
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
                    // if no other thread has updated _fetchingBatch yet, do it - and then calling FetchBatch() may
                    // throw immediately, in which case we raise the exception to the caller, but before that
                    // we make sure to clear _fetchingBatch so that FetchBatch() will be tried again next time
                    if (_fetchingBatch != fetchingBatch) continue;

                    try
                    {
                        _fetchingBatch = FetchAndSetBatch();
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
