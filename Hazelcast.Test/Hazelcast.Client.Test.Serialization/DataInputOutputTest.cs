// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    internal class DataInputOutputTest
    {
        private static readonly ByteOrder[] ByteOrders =
        {
            ByteOrder.BigEndian, ByteOrder.LittleEndian,
            ByteOrder.NativeOrder()
        };

        [TestCaseSource("ByteOrders")]
        public virtual void TestDataInputOutputWithPortable(ByteOrder byteOrder)
        {
            var portable = KitchenSinkPortable.Generate();

            var config = new SerializationConfig();
            config.AddPortableFactoryClass(KitchenSinkPortableFactory.FactoryId, typeof (KitchenSinkPortableFactory));

            var ss = new SerializationServiceBuilder().SetConfig(config).
                SetUseNativeByteOrder(false).SetByteOrder(byteOrder).Build();

            IObjectDataOutput output = ss.CreateObjectDataOutput(1024);
            output.WriteObject(portable);
            var data = output.ToByteArray();

            IObjectDataInput input = ss.CreateObjectDataInput(data);
            var readObject = input.ReadObject<IPortable>();

            Assert.AreEqual(portable, readObject);

            ss.Destroy();
        }

        [TestCaseSource("ByteOrders")]
        public virtual void TestInputOutputWithPortableReader(ByteOrder byteOrder)
        {
            var portable = KitchenSinkPortable.Generate();

            var config = new SerializationConfig();
            config.AddPortableFactoryClass(KitchenSinkPortableFactory.FactoryId, typeof (KitchenSinkPortableFactory));

            var ss = new SerializationServiceBuilder().SetConfig(config).
                SetUseNativeByteOrder(false).SetByteOrder(byteOrder).Build();

            var data = ss.ToData(portable);
            var reader = ss.CreatePortableReader(data);

            var actual = new KitchenSinkPortable();
            actual.ReadPortable(reader);

            Assert.AreEqual(portable, actual);

            ss.Destroy();
        }

        [TestCaseSource("ByteOrders")]
        public virtual void TestReadWrite(ByteOrder byteOrder)
        {
            var obj = KitchenSinkDataSerializable.Generate();
            obj.Serializable = KitchenSinkDataSerializable.Generate();

            var ss = new SerializationServiceBuilder()
                .AddDataSerializableFactory(1, new ArrayDataSerializableFactory(new Func<IIdentifiedDataSerializable>[]
                {
                    () => new KitchenSinkDataSerializable(),
                }))
                .SetUseNativeByteOrder(false).SetByteOrder(byteOrder).Build();

            IObjectDataOutput output = ss.CreateObjectDataOutput(1024);
            output.WriteObject(obj);

            IObjectDataInput input = ss.CreateObjectDataInput(output.ToByteArray());
            var readObj = input.ReadObject<object>();
            Assert.AreEqual(obj, readObj);

            ss.Destroy();
        }

        [Test]
        public void TestNullValue_When_ReferenceType()
        {
            var ss = new SerializationServiceBuilder()
               .Build();

            var output = ss.CreateObjectDataOutput(1024);
            ss.WriteObject(output, null);

            var input = ss.CreateObjectDataInput(output.ToByteArray());
            Assert.IsNull(ss.ReadObject<object>(input));
        }

        [Test, ExpectedException(typeof (HazelcastSerializationException))]
        public void TestNullValue_When_ValueType()
        {
            var ss = new SerializationServiceBuilder()
               .Build();

            var output = ss.CreateObjectDataOutput(1024);
            ss.WriteObject(output, null);

            var input = ss.CreateObjectDataInput(output.ToByteArray());
            ss.ReadObject<int>(input);
        }

        [Test]
        public void TestNullValue_When_NullableType()
        {
            var ss = new SerializationServiceBuilder()
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