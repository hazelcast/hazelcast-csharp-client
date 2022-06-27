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
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Tests.Serialization.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    internal class DataInputOutputTest
    {
        private static readonly object[] Endiannesses =
        {
            new object[]{Endianness.BigEndian, 0 },
            new object[]{Endianness.BigEndian, 1 },
            new object[]{Endianness.BigEndian, 2 },
            new object[]{Endianness.BigEndian, 3 },
            new object[]{Endianness.BigEndian, 4 },
            new object[]{Endianness.BigEndian, 10 },

            new object[]{Endianness.LittleEndian, 0 },
            new object[]{Endianness.LittleEndian, 1 },
            new object[]{Endianness.LittleEndian, 2 },
            new object[]{Endianness.LittleEndian, 3 },
            new object[]{Endianness.LittleEndian, 4 },
            new object[]{Endianness.LittleEndian, 10 }
            
            //Endianness.NativeOrder()
        };

        [TestCaseSource(nameof(Endiannesses))]
        public virtual void TestDataInputOutputWithPortable(Endianness endianness, int arraySize)
        {
            var portable = KitchenSinkPortable.Generate(arraySize);

            var config = new SerializationOptions();
            config.AddPortableFactory(KitchenSinkPortableFactory.FactoryId, typeof(KitchenSinkPortableFactory));

            using var ss = new SerializationServiceBuilder(new NullLoggerFactory()).SetConfig(config)
                .SetEndianness(endianness).Build();

            IObjectDataOutput output = ss.CreateObjectDataOutput(1024);
            output.WriteObject(portable);
            var data = output.ToByteArray();

            IObjectDataInput input = ss.CreateObjectDataInput(data);
            var readObject = input.ReadObject<IPortable>();

            Assert.AreEqual(portable, readObject);
        }

        [TestCaseSource(nameof(Endiannesses))]
        public virtual void TestInputOutputWithPortableReader(Endianness endianness, int arraySize)
        {
            var portable = KitchenSinkPortable.Generate(arraySize);

            var config = new SerializationOptions();
            config.AddPortableFactory(KitchenSinkPortableFactory.FactoryId, typeof(KitchenSinkPortableFactory));

            using var ss = new SerializationServiceBuilder(new NullLoggerFactory()).SetConfig(config)
                .SetEndianness(endianness).Build();

            var data = ss.ToData(portable);
            var reader = ss.CreatePortableReader(data);

            var actual = new KitchenSinkPortable();
            actual.ReadPortable(reader);

            Assert.AreEqual(portable, actual);
        }

        [TestCaseSource(nameof(Endiannesses))]
        public virtual void TestReadWrite(Endianness endianness, int arraySize)
        {
            var obj = KitchenSinkDataSerializable.Generate(arraySize);
            obj.Serializable = KitchenSinkDataSerializable.Generate(arraySize);

            using var ss = new SerializationServiceBuilder(new NullLoggerFactory())
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
            ss.WriteObject(output, null);

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
            ss.WriteObject(output, null);

            var input = ss.CreateObjectDataInput(output.ToByteArray());
            ss.ReadObject<int>(input);
        });
        }

        [Test]
        public void TestNullValue_When_NullableType()
        {
            var ss = new SerializationServiceBuilder(new NullLoggerFactory())
               .Build();

            var output = ss.CreateObjectDataOutput(1024);
            ss.WriteObject(output, 1);
            ss.WriteObject(output, null);

            var input = ss.CreateObjectDataInput(output.ToByteArray());
            Assert.AreEqual(1, ss.ReadObject<int?>(input));
            Assert.IsNull(ss.ReadObject<int?>(input));
        }
    }
}
