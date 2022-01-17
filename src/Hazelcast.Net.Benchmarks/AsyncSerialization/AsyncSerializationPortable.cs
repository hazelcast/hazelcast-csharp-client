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

namespace Hazelcast.Benchmarks.AsyncSerialization
{
    // This benchmark is part of the compact serialization async evaluation. See the notes file for details.

    public class AsyncSerializationPortable
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
                .SetPartitioningStrategy(new PartitionAwarePartitioningStragegy())
                .SetVersion(SerializationService.SerializerVersion)
                .AddHook<PredicateDataSerializerHook>()
                .AddHook<AggregatorDataSerializerHook>()
                .AddHook<ProjectionDataSerializerHook>()
                .AddDefinitions(new ConstantSerializerDefinitions())
                .AddDefinitions(new DefaultSerializerDefinitions())
                ;

            _serializationService = serializationServiceBuilder.Build();

            var obj = new PortableThing
            {
                IntValue = 42,
                StringValue = Guid.NewGuid().ToString("N"),
                DoubleValue = 123.456
            };
            _serializedThing = _serializationService.ToData(obj);
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
        public async Task AsyncOptimizedWorst()
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
                else sum += await source.GetAsync();
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
                else sum += await source.GetAsync();
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
                else sum += await source.GetAsync();
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
                    sum += await source.GetAsync();
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

            public int GetSync()
            {
                var thing = _serializationService.ToObject<PortableThing>(_serializedThing);
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
                if (_condition)
                {
                    var thing = _serializationService.ToObject<PortableThing>(_serializedThing);
                    return new ValueTask<int>(Interlocked.Increment(ref _value));
                }

                return GetAsync();
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

            public int IntValue { get; set; }

            public string StringValue { get; set; }

            public double DoubleValue { get; set; }

            public void ReadPortable(IPortableReader reader)
            {
                IntValue = reader.ReadInt(nameof(IntValue));
                StringValue = reader.ReadString(nameof(StringValue));
                DoubleValue = reader.ReadDouble(nameof(DoubleValue));
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteInt(nameof(IntValue), IntValue);
                writer.WriteString(nameof(StringValue), StringValue);
                writer.WriteDouble(nameof(DoubleValue), DoubleValue);
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
