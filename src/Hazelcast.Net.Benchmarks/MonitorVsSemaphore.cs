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

using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Hazelcast.Benchmarks
{
    public class MonitorVsSemaphore
    {
        private readonly object _lock = new object();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

#pragma warning disable IDE0052 // Remove unread private members - still, we want it
        private int _i;
#pragma warning restore IDE0052

        // so... semaphore would be 3x slower that plain lock
        // but plain lock cannot contain async code ;(

        [Benchmark]
        public Task Lock()
        {
            lock (_lock)
            {
                _i++;
            }

            return Task.CompletedTask;
        }

        [Benchmark]
        public async Task Semaphore()
        {
            await _semaphore.WaitAsync(); //.CfAwait();
            _i++;
            _semaphore.Release();
        }
    }
}
