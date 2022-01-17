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
using BenchmarkDotNet.Attributes;
using Hazelcast.Aggregation;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Projection;
using Hazelcast.Query;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Serialization.DefaultSerializers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast.Benchmarks
{
    /*

    This benchmark was created for compact serialization - as a simplified version of AsyncToObjectPortable. Here, we only
    deserialize a simple integer value, not an object. So, the cost of serialization is lower, which explains the 1.21
    ratio of our preferred AsyncOptimized method. In other words, a 20% penalty for a single integer, compared to the
    pure-sync method, whatever the method we use (boolean, exception...) which is essentially due to async.

    |                    Method |         Mean |      Error |     StdDev |       Median |  Ratio | RatioSD |   Gen 0 | Allocated |
    |-------------------------- |-------------:|-----------:|-----------:|-------------:|-------:|--------:|--------:|----------:|
    |                  BareSync |     10.04 us |   0.133 us |   0.125 us |     10.06 us |   0.13 |    0.00 |  0.0305 |     152 B |
    |                      Sync |     76.55 us |   1.524 us |   2.503 us |     76.18 us |   1.00 |    0.00 | 17.2119 |  72,223 B |
    |                 BareAsync |  1,021.43 us |  16.628 us |  14.740 us |  1,017.85 us |  13.24 |    0.52 | 31.2500 | 128,248 B |
    |                     Async |  1,182.70 us |  18.237 us |  16.166 us |  1,179.05 us |  15.33 |    0.57 | 46.8750 | 200,256 B |
    |        AsyncOptimizedBest |     92.70 us |   1.850 us |   5.186 us |     92.19 us |   1.21 |    0.08 | 17.2119 |  72,256 B |
    |       AsyncOptimizedFalse |  1,181.52 us |  14.632 us |  12.971 us |  1,182.44 us |  15.31 |    0.61 | 46.8750 | 200,256 B |
    |       AsyncOptimized2Best |     88.00 us |   2.723 us |   7.899 us |     84.94 us |   1.20 |    0.13 | 17.2119 |  72,256 B |
    |      AsyncOptimized2Worst |  1,195.67 us |  18.770 us |  17.557 us |  1,188.59 us |  15.54 |    0.56 | 46.8750 | 200,256 B |
    |  SyncOrFalseThenAsyncBest |     90.63 us |   2.679 us |   7.856 us |     88.01 us |   1.18 |    0.10 | 17.2119 |  72,255 B |
    | SyncOrFalseThenAsyncWorst |  1,055.05 us |  12.659 us |  11.841 us |  1,052.33 us |  13.71 |    0.52 | 31.2500 | 128,256 B |
    |  SyncOrThrowThenAsyncBest |     87.18 us |   1.741 us |   4.854 us |     86.35 us |   1.14 |    0.07 | 17.2119 |  72,255 B |
    | SyncOrThrowThenAsyncWorst | 11,724.89 us | 331.353 us | 966.573 us | 11,495.38 us | 150.92 |   11.43 | 93.7500 | 448,256 B |

     */

    public class AsyncToObjectInteger
    {
        private SerializationService _serializationService;
        private IData _serializedThing;

        [GlobalSetup]
        public void Setup()
        {
            var options = new SerializationOptions();
            var serializationServiceBuilder = new SerializationServiceBuilder(new NullLoggerFactory());
            serializationServiceBuilder
                .SetConfig(options)
                .SetPartitioningStrategy(new PartitionAwarePartitioningStragegy()) // TODO: should be configure-able
                .SetVersion(SerializationService.SerializerVersion) // uh? else default is wrong?
                .AddHook<PredicateDataSerializerHook>() // shouldn't they be configurable?
                .AddHook<AggregatorDataSerializerHook>()
                .AddHook<ProjectionDataSerializerHook>()
                .AddDefinitions(new ConstantSerializerDefinitions())
                .AddDefinitions(new DefaultSerializerDefinitions())
                ;

            _serializationService = serializationServiceBuilder.Build();
            _serializedThing = _serializationService.ToData(123456);
        }

        // synchronous, no serialization - just to see the impact of bare async
        [Benchmark]
        public async Task BareSync()
        {
            await Task.Yield();

            var source = new IntegerSource();

            var sum = 0;
            for (var i = 0; i < 1000; i++) sum += source.GetBareSync();
        }

        // synchronous
        [Benchmark(Baseline = true)]
        public async Task Sync()
        {
            await Task.Yield();

            var source = new IntegerSource(
                true,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++) sum += source.GetSync();
        }

        // unoptimized asynchronous, no serialization - just to see the impact of bare async
        [Benchmark]
        public async Task BareAsync()
        {
            await Task.Yield();

            var source = new IntegerSource();

            var sum = 0;
            for (var i = 0; i < 1000; i++) sum += await source.GetBareAsync();
        }

        // unoptimized asynchronous
        [Benchmark]
        public async Task Async()
        {
            await Task.Yield();

            var source = new IntegerSource(
                true,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++) sum += await source.GetAsync();
        }

        // optimized asynchronous: the value can be synchronously returned if immediately available
        // best case is, it's always available
        [Benchmark]
        public async Task AsyncOptimizedBest()
        {
            await Task.Yield();

            var source = new IntegerSource(
                true,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++) sum += await source.GetAsyncOptimized();
        }

        // optimized asynchronous: the value can be synchronously returned if immediately available
        // worst case is, it's never available
        [Benchmark]
        public async Task AsyncOptimizedFalse()
        {
            await Task.Yield();

            var source = new IntegerSource(
                false,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++) sum += await source.GetAsyncOptimized();
        }

        // optimized asynchronous: the value can be synchronously returned if immediately available
        // best case is, it's always available
        [Benchmark]
        public async Task AsyncOptimized2Best()
        {
            await Task.Yield();

            var source = new IntegerSource(
                true,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++)
            {
                var valueTask = source.GetAsyncOptimized();
                if (valueTask.IsCompletedSuccessfully) sum += valueTask.Result;
                else sum += await valueTask;
            }
        }

        // optimized asynchronous: the value can be synchronously returned if immediately available
        // best case is, it's always available
        [Benchmark]
        public async Task AsyncOptimized2Worst()
        {
            await Task.Yield();

            var source = new IntegerSource(
                false,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++)
            {
                var valueTask = source.GetAsyncOptimized();
                if (valueTask.IsCompletedSuccessfully) sum += valueTask.Result;
                else sum += await valueTask;
            }
        }

        // try synchronous, else asynchronous, via boolean
        // best case is, it's always available
        [Benchmark]
        public async Task SyncOrFalseThenAsyncBest()
        {
            await Task.Yield();

            var source = new IntegerSource(
                true,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++)
            {
                var (hasValue, value) = source.TryGetSyncOrFalse();
                if (hasValue) sum += value;
                else sum += await source.GetBareAsync();
            }
        }

        // try synchronous, else asynchronous, via boolean
        // worst case is, it's never available
        [Benchmark]
        public async Task SyncOrFalseThenAsyncWorst()
        {
            await Task.Yield();

            var source = new IntegerSource(
                false,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++)
            {
                var (hasValue, value) = source.TryGetSyncOrFalse();
                if (hasValue) sum += value;
                else sum += await source.GetBareAsync();
            }
        }

        // try synchronous, else asynchronous, via exception
        // best case is, it's always available
        [Benchmark]
        public async Task SyncOrThrowThenAsyncBest()
        {
            await Task.Yield();

            var source = new IntegerSource(
                true,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++)
            {
                var (hasValue, value) = source.TryGetSyncOrFalse();
                if (hasValue) sum += value;
                else sum += await source.GetBareAsync();
            }
        }

        // try synchronous, else asynchronous, via exception
        // worst case is, it's never available
        [Benchmark]
        public async Task SyncOrThrowThenAsyncWorst()
        {
            await Task.Yield();

            var source = new IntegerSource(
                false,
                serializationService: _serializationService,
                serializedThing: _serializedThing
            );

            var sum = 0;
            for (var i = 0; i < 1000; i++)
            {
                try
                {
                    sum += source.TryGetSyncOrThrow();
                }
                catch
                {
                    sum += await source.GetBareAsync();
                }
            }
        }

        // a source of integers that support different ways of producing integers
        // practically this emulates what different versions of ToData and ToObject might do
        private class IntegerSource
        {
            private readonly bool _condition;
            private readonly SerializationService _serializationService;
            private readonly IData _serializedThing;

            private int _value;

            public IntegerSource(bool condition = true, SerializationService serializationService = null, IData serializedThing = null)
            {
                _condition = condition;
                _serializationService = serializationService;
                _serializedThing = serializedThing;
            }

            public int GetBareSync()
            {
                return Interlocked.Increment(ref _value);
            }

            public int GetSync()
            {
                var thing = _serializationService.ToObject<int>(_serializedThing);
                return Interlocked.Increment(ref _value);
            }

            public async ValueTask<int> GetBareAsync()
            {
                await Task.Yield();
                return Interlocked.Increment(ref _value);
            }

            public async ValueTask<int> GetAsync()
            {
                var thing = _serializationService.ToObject<int>(_serializedThing);
                await Task.Yield();
                return Interlocked.Increment(ref _value);
            }

            public ValueTask<int> GetAsyncOptimized()
            {
                var thing = _serializationService.ToObject<int>(_serializedThing);
                return _condition
                    ? new ValueTask<int>(Interlocked.Increment(ref _value))
                    : GetBareAsync();
            }

            public (bool, int) TryGetSyncOrFalse()
            {
                if (!_condition) return (false, default);
                var thing = _serializationService.ToObject<int>(_serializedThing);
                return (true, Interlocked.Increment(ref _value));
            }

            public int TryGetSyncOrThrow()
            {
                if (!_condition) throw new Exception("bah");
                var thing = _serializationService.ToObject<int>(_serializedThing);
                return Interlocked.Increment(ref _value);
            }
        }
    }
}
