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

    This benchmark was created for compact serialization - up until now, ToObject or ToData were pure synchronous methods,
    but compact serialization introduces schemas, and a schema may not be available on the client when required, and then
    has to be fetched from the cluster - and that is an asynchronous operation.

    So, the challenge is: how can we support asynchronous ToObject and ToData with as little impact on the general performances
    of serialization, both for existing mechanisms (portable...) and for compact in the (most usual) case where the schema is
    already available on the client.

    |                    Method |          Mean |       Error |      StdDev |        Median | Ratio | RatioSD |   Gen 0 | Allocated |
    |-------------------------- |--------------:|------------:|------------:|--------------:|------:|--------:|--------:|----------:|
    |                  BareSync |      9.712 us |   0.1928 us |   0.3715 us |      9.532 us |  0.03 |    0.00 |  0.0305 |     152 B |
    |                      Sync |    323.863 us |   6.3919 us |   9.1670 us |    325.095 us |  1.00 |    0.00 | 61.0352 | 256,224 B |
    |                 BareAsync |  1,045.974 us |  15.0724 us |  13.3613 us |  1,041.333 us |  3.27 |    0.10 | 31.2500 | 128,248 B |
    |                     Async |  1,526.789 us |  30.1138 us |  69.1915 us |  1,531.886 us |  4.73 |    0.30 | 91.7969 | 384,256 B |
    |        AsyncOptimizedBest |    344.557 us |   6.2061 us |  11.1909 us |    343.162 us |  1.07 |    0.05 | 61.0352 | 256,256 B |
    |       AsyncOptimizedFalse |  1,533.501 us |  30.6277 us |  80.6855 us |  1,531.931 us |  4.66 |    0.25 | 91.7969 | 384,256 B |
    |       AsyncOptimized2Best |    339.537 us |   6.6748 us |   6.2436 us |    340.879 us |  1.06 |    0.02 | 61.0352 | 256,256 B |
    |      AsyncOptimized2Worst |  1,536.926 us |  30.5131 us |  80.3838 us |  1,537.679 us |  4.75 |    0.32 | 91.7969 | 384,256 B |
    |  SyncOrFalseThenAsyncBest |    345.687 us |   6.8390 us |  11.0437 us |    344.977 us |  1.07 |    0.05 | 61.0352 | 256,256 B |
    | SyncOrFalseThenAsyncWorst |  1,097.796 us |  12.2270 us |  10.8389 us |  1,096.415 us |  3.43 |    0.11 | 31.2500 | 128,256 B |
    |  SyncOrThrowThenAsyncBest |    346.972 us |   6.8229 us |  12.8150 us |    346.728 us |  1.07 |    0.05 | 61.0352 | 256,256 B |
    | SyncOrThrowThenAsyncWorst | 10,386.337 us | 318.8073 us | 899.2016 us | 10,107.230 us | 31.57 |    2.44 | 93.7500 | 448,256 B |

    Things to note:

    - BareSync an BareAsync are here to show the pure impact of asynchronous calls, which *is* expensive as async has
      a ratio of ~110, but the meaningful comparison is when some actual serialization work is done, and then the
      ratio falls down to ~5 for a minimal serialization op, and would be even smaller for bigger serialization ops.

    - AsyncOptimized get a ValueTask which is synchronously produced (would correspond to "schema exists") with a fallback
      to returning a ValueTask backed by an actual Task, should the schema need to be fetched from the cluster. We benchmark
      the best and worst cases (never / always need to do async), and there we see that in the best case we bring the
      ratio down to 1.05. It would be way bigger if we did BareAsyncOptimized, but there is little value in benchmarking this.

    - AsyncOptimized2 is same as AsyncOptimized, but instead of always awaiting the ValueTask we check for completion. It makes
      caller's code more complicated, and we can see that there is no benefit in doing so.

    - SyncOrFalse and SyncOrThrow are variations of the optimization, where we first try a fully synchronous methods, and
      fallback to another call to an asynchronous method, if required. The synchronous methods can either return a boolean
      flag (for SyncOrFalse) or throw an exception (for SyncOrThrow).

      Bare comparisons would show that BareSyncOrThrow is the fastest - in the best case, when we don't throw. But, the
      difference is marginal as soon as we do actual serialization, and in addition it is terribly slow as soon as we need
      to throw, so that is not really a possible solution.

      Bare comparison would show that SyncOrFalse is close to AsyncOptimized, and slightly worst in the best case. As soon
      as actual serialization is done, there is practically no difference for best case. It is interesting to note that it
      is better in worst case (~3.4 vs ~4.7). However, it complicates everything as the responsibility of the test is
      moved to the caller.

      Note: the 3.4 vs 4.7 difference can probably be explained by the fact that AsyncOptimized in worst case ends up
      creating a Task wrapped in a ValueTask, in a way that is more expensive than simply awaiting the async method.

    Conclusion:

    Switching from Sync (our current ToObject implementation) to AsyncOptimized implies a max 1.1 ratio in best case, which
    includes other existing (portable...) serialization methods. No method other than AsyncOptimized provides a measurable
    improvements.

    SyncOrFalse could provide a better worst-case performance, without sacrificing best-case, but at the cost of a
    complex code (essentially, if/then/else everywhere we have a ToObject call today). But, we have to consider that in
    worst cases, we would have to introduce a network call to fetch the missing schema - so the difference between
    SyncOrFalse vs AsyncOptimized would be even more marginal.

    Therefore, go with AsyncOptimized.

    More notes:

    - some methods do 'await Task.Yield()' - this achieves nothing but prevents the compiler from optimizing the method
      as a non-async one ie a synchronous one.

     */
    public class AsyncToObject
    {
        private SerializationService _serializationService;
        private IData _serializedThing;

        [GlobalSetup]
        public void Setup()
        {
            var options = new SerializationOptions();
            options.AddPortableFactory(PortableThing.PortableFactoryId, new PortableThingFactory());
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

            var obj = new PortableThing { Value = 42 };
            _serializedThing = _serializationService.ToData(obj);
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
                var thing = _serializationService.ToObject<PortableThing>(_serializedThing);
                return Interlocked.Increment(ref _value);
            }

            public async ValueTask<int> GetBareAsync()
            {
                await Task.Yield();
                return Interlocked.Increment(ref _value);
            }

            public async ValueTask<int> GetAsync()
            {
                var thing = _serializationService.ToObject<PortableThing>(_serializedThing);
                await Task.Yield();
                return Interlocked.Increment(ref _value);
            }

            public ValueTask<int> GetAsyncOptimized()
            {
                var thing = _serializationService.ToObject<PortableThing>(_serializedThing);
                return _condition
                    ? new ValueTask<int>(Interlocked.Increment(ref _value))
                    : GetBareAsync();
            }

            public (bool, int) TryGetSyncOrFalse()
            {
                if (!_condition) return (false, default);
                var thing = _serializationService.ToObject<PortableThing>(_serializedThing);
                return (true, Interlocked.Increment(ref _value));
            }

            public int TryGetSyncOrThrow()
            {
                if (!_condition) throw new Exception("bah");
                var thing = _serializationService.ToObject<PortableThing>(_serializedThing);
                return Interlocked.Increment(ref _value);
            }
        }

        // a dummy portable class
        private class PortableThing : IPortable
        {
            public const int PortableFactoryId = 123;
            public const int PortableClassId = 456;

            public int FactoryId => PortableFactoryId;

            public int ClassId => PortableClassId;

            public int Value { get; set; }

            public void ReadPortable(IPortableReader reader)
            {
                Value = reader.ReadInt(nameof(Value));
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteInt(nameof(Value), Value);
            }
        }

        // a dummy portable factory
        private class PortableThingFactory : IPortableFactory
        {
            public IPortable Create(int classId)
            {
                if (classId == PortableThing.PortableClassId) return new PortableThing();

                throw new ArgumentOutOfRangeException(nameof(classId));
            }
        }
    }
}
