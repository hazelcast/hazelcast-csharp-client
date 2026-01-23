// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Tests.Serialization.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using NUnit.Framework;
namespace Hazelcast.Tests.Serialization
{
    internal class DataInputOutputTest
    {
        private static readonly object[] DataCases =
        {
            new object[] { Endianness.BigEndian, 0 },
            new object[] { Endianness.BigEndian, 1 },
            new object[] { Endianness.BigEndian, 2 },
            new object[] { Endianness.BigEndian, 3 },
            new object[] { Endianness.BigEndian, 4 },
            new object[] { Endianness.BigEndian, 10 },

            new object[] { Endianness.LittleEndian, 0 },
            new object[] { Endianness.LittleEndian, 1 },
            new object[] { Endianness.LittleEndian, 2 },
            new object[] { Endianness.LittleEndian, 3 },
            new object[] { Endianness.LittleEndian, 4 },
            new object[] { Endianness.LittleEndian, 10 }
        };

        [TestCaseSource(nameof(DataCases))]
        public virtual void TestDataInputOutputWithPortable(Endianness endianness, int arraySize)
        {
            var portable = KitchenSinkPortable.Generate(arraySize);

            var config = new SerializationOptions();
            config.AddPortableFactory(KitchenSinkPortableFactory.FactoryId, typeof(KitchenSinkPortableFactory));

            using var ss = new SerializationServiceBuilder(config, new NullLoggerFactory())
                .SetEndianness(endianness).Build();

            IObjectDataOutput output = ss.CreateObjectDataOutput(1024);
            output.WriteObject(portable);
            var data = output.ToByteArray();

            IObjectDataInput input = ss.CreateObjectDataInput(data);
            var readObject = input.ReadObject<IPortable>();

            Assert.AreEqual(portable, readObject);
        }

        [TestCaseSource(nameof(DataCases))]
        public virtual void TestInputOutputWithPortableReader(Endianness endianness, int arraySize)
        {
            var portable = KitchenSinkPortable.Generate(arraySize);

            var config = new SerializationOptions();
            config.AddPortableFactory(KitchenSinkPortableFactory.FactoryId, typeof(KitchenSinkPortableFactory));

            using var ss = new SerializationServiceBuilder(config, new NullLoggerFactory())
                .SetEndianness(endianness).Build();

            var data = ss.ToData(portable);
            var reader = ss.CreatePortableReader(data);

            var actual = new KitchenSinkPortable();
            actual.ReadPortable(reader);

            Assert.AreEqual(portable, actual);
        }

        [TestCaseSource(nameof(DataCases))]
        public virtual void TestReadWrite(Endianness endianness, int arraySize)
        {
            var obj = KitchenSinkDataSerializable.Generate(arraySize);
            obj.Serializable = KitchenSinkDataSerializable.Generate(arraySize);

            using var ss = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddDefinitions(new ConstantSerializerDefinitions()) // use constant serializers not CLR serialization
                .AddDataSerializableFactory(1, new ArrayDataSerializableFactory(new Func<IIdentifiedDataSerializable>[]
                {
                    () => new KitchenSinkDataSerializable(),
                }))
                .SetEndianness(endianness).Build();

            IObjectDataOutput output = ss.CreateObjectDataOutput(1024);
            output.WriteObject(obj);

            IObjectDataInput input = ss.CreateObjectDataInput(output.ToByteArray());
            var readObj = input.ReadObject<object>();
            Assert.AreEqual(obj, readObj);
        }

        [Test]
        public void TestNullValue_When_ReferenceType()
        {
            var ss = new SerializationServiceBuilder(new NullLoggerFactory())
                .Build();

            var output = ss.CreateObjectDataOutput(1024);
            ss.WriteObject(output, null, true, false);

            var input = ss.CreateObjectDataInput(output.ToByteArray());
            Assert.IsNull(ss.ReadObject<object>(input));
        }

        [Test]
        public void TestNullValue_When_ValueType()
        {
            Assert.Throws<SerializationException>(() =>
            {
                var ss = new SerializationServiceBuilder(new NullLoggerFactory())
                    .Build();

                var output = ss.CreateObjectDataOutput(1024);
                ss.WriteObject(output, null, true, false);

                var input = ss.CreateObjectDataInput(output.ToByteArray());
                ss.ReadObject<int>(input);
            });
        }

        [Test]
        public void TestNullValue_When_NullableType()
        {
            var ss = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddDefinitions(new ConstantSerializerDefinitions()) // use constant serializers not CLR serialization
                .Build();

            var output = ss.CreateObjectDataOutput(1024);
            ss.WriteObject(output, 1, true, false);
            ss.WriteObject(output, null, true, false);

            var input = ss.CreateObjectDataInput(output.ToByteArray());
            Assert.AreEqual(1, ss.ReadObject<int?>(input));
            Assert.IsNull(ss.ReadObject<int?>(input));
        }

        [Test]
        public void TestObjectDataOutputRentsFromPool()
        {
            var maxPoolSize = 10;
            var myBufferPool = new MyBufferPool();

            // Create a counting policy that creates ObjectDataOutput instances directly
            var countingPolicy = new CountingPooledObjectPolicy(()
                => new ObjectDataOutput(1024, null, Endianness.BigEndian, myBufferPool));
            var objectDataPool = new DefaultObjectPool<ObjectDataOutput>(countingPolicy, maxPoolSize);

            var ss = new SerializationServiceBuilder(new NullLoggerFactory())
                .SetBufferPool(myBufferPool)
                .SetObjectDataOutputPool(objectDataPool)
                .AddDefinitions(new ConstantSerializerDefinitions())
                .Build();

            for (var i = 0; i < maxPoolSize; i++)
            {
                var tempData = new byte[10 * 1024]; // 10 KB data to trigger buffer resize for renting
                new Random().NextBytes(tempData);
                ss.ToData(tempData);
            }

            // since serialization is done sequentially, we expect less than maxPoolSize creations
            Assert.LessOrEqual(countingPolicy.CreateCount, maxPoolSize, "Too many ObjectDataOutput instances created");

            // We expect maxPool*2 since there are two buffer rents when object is already in the pool.
            // First one is while writing data to buffer (data bigger than buffer so resizing),
            // Second one is on TryReset during returning to pool. It resizes the buffer to default size.

            /* +1 because at the very first write, ObjectDataOutput has no buffer,
            and EnsureAvailable rents one during partitionHash write*/
            Assert.AreEqual(maxPoolSize * 2 + 1, myBufferPool.RentCount, "Unexpected number of buffer rents");
            Assert.AreEqual(maxPoolSize * 2, myBufferPool.ReturnCount, "Unexpected number of buffer returns");
        }

        private class CountingPooledObjectPolicy : IPooledObjectPolicy<ObjectDataOutput>
        {
            private readonly Func<ObjectDataOutput> _factory;
            public int CreateCount { get; private set; }

            public CountingPooledObjectPolicy(Func<ObjectDataOutput> factory)
            {
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public ObjectDataOutput Create()
            {
                CreateCount++;
                return _factory();
            }

            public bool Return(ObjectDataOutput obj) => obj.TryReset();
        }
        private class MyBufferPool : IBufferPool
        {
            private readonly DefaultBufferPool _inner = new DefaultBufferPool();
            public int RentCount { get; private set; }
            public int ReturnCount { get; private set; }

            public ManualResetEvent HoldReturnEvent { get; } = new ManualResetEvent(false);
            public byte[] Rent(int minSize)
            {
                RentCount++;
                TestContext.Progress.WriteLine($"[DataInputOutputTest] MyBufferPool.Rent called. minSize={minSize}, RentCount={RentCount}");
                return _inner.Rent(minSize);
            }

            public void Return(byte[] buffer)
            {
                ReturnCount++;
                TestContext.Progress.WriteLine($"[DataInputOutputTest] MyBufferPool.Return called. buffer.length={buffer.Length}, ReturnCount={ReturnCount}");
                _inner.Return(buffer);
            }
        }
    }
}
