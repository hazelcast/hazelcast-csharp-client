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

namespace Hazelcast.Benchmarks.AsyncSerialization
{
    // This benchmark is part of the compact serialization async evaluation. See the notes file for details.

    public class AsyncSerializationBare
    {
        // synchronous
        [Benchmark(Baseline = true)]
        public async Task Sync()
        {
            await Task.Yield();

            var source = new IntegerSource();

            var sum = 0;
            for (var i = 0; i < 1000; i++) sum += source.GetSync();
        }

        // true asynchronous
        [Benchmark]
        public async Task BareTrueAsync()
        {
            await Task.Yield();

            var source = new IntegerSource();

            var sum = 0;
            for (var i = 0; i < 1000; i++) sum += await source.GetTrueAsync();
        }

        // non-asynchronous (returns a ValueTask)
        [Benchmark]
        public async Task BareNonAsync()
        {
            await Task.Yield();

            var source = new IntegerSource();

            var sum = 0;
            for (var i = 0; i < 1000; i++) sum += await source.GetNonAsync();
        }

        // a source of integers that support different ways of producing integers
        // practically this emulates what different versions of ToData and ToObject might do
        private class IntegerSource
        {
            private int _value;

            public int GetSync()
            {
                return Interlocked.Increment(ref _value);
            }

            public async ValueTask<int> GetTrueAsync()
            {
                await Task.Yield();
                return Interlocked.Increment(ref _value);
            }

            public ValueTask<int> GetNonAsync()
            {
                return new ValueTask<int>(Interlocked.Increment(ref _value));
            }
        }
    }
}
