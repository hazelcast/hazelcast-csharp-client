// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using Hazelcast.Config;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    [TestFixture]
    public class PortableSerializationTest
    {
        private static readonly ByteOrder[] ByteOrders =
        {
            ByteOrder.BigEndian, ByteOrder.LittleEndian,
            ByteOrder.NativeOrder()
        };

        internal static ISerializationService CreateSerializationService(int version)
        {
            return CreateSerializationService(version, ByteOrder.BigEndian);
        }

        internal static ISerializationService CreateSerializationService(int version, ByteOrder order)
        {
            return new SerializationServiceBuilder()
                .SetUseNativeByteOrder(false).SetByteOrder(order).SetPortableVersion(version)
                .AddPortableFactory(TestSerializationConstants.PORTABLE_FACTORY_ID, new TestPortableFactory())
                .AddDataSerializableFactory(TestSerializationConstants.DATA_SERIALIZABLE_FACTORY_ID,
                    GetDataSerializableFactory())
                .Build();
        }

        private static ArrayDataSerializableFactory GetDataSerializableFactory()
        {
            return new ArrayDataSerializableFactory(new Func<IIdentifiedDataSerializable>[]
            {
                () => new SampleIdentifiedDataSerializable(),
                () => new ByteArrayDataSerializable(),
                () => new DataDataSerializable(),
                () => new ComplexDataSerializable(),
            });
        }

        internal static IClassDefinition CreateNamedPortableClassDefinition(int portableVersion)
        {
            var builder = new ClassDefinitionBuilder(TestSerializationConstants.PORTABLE_FACTORY_ID,
                TestSerializationConstants.NAMED_PORTABLE, portableVersion);
            builder.AddUTFField("name");
            builder.AddIntField("myint");
            return builder.Build();
        }

        private static void AssertRepeatedSerialisationGivesSameByteArrays(ISerializationService ss, IPortable p)
        {
            var data1 = ss.ToData(p);
            for (var k = 0; k < 100; k++)
            {
                var data2 = ss.ToData(p);
                Assert.AreEqual(data1, data2);
            }
        }

        public class TestObject1 : IPortable
        {
            private IPortable[] portables;

            public TestObject1()
            {
            }

            public TestObject1(IPortable[] p)
            {
                portables = p;
            }

            public int GetFactoryId()
            {
                return TestSerializationConstants.PORTABLE_FACTORY_ID;
            }

            public int GetClassId()
            {
                return 1;
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WritePortableArray("list", portables);
            }

            public void ReadPortable(IPortableReader reader)
            {
                portables = reader.ReadPortableArray("list");
            }
        }

        public class TestObject2 : IPortable
        {
            private readonly string shortString;

            public TestObject2()
            {
                shortString = "Hello World";
            }

            public int GetFactoryId()
            {
                return TestSerializationConstants.PORTABLE_FACTORY_ID;
            }

            public int GetClassId()
            {
                return 2;
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteUTF("shortString", shortString);
            }

            public void ReadPortable(IPortableReader reader)
            {
                throw new NotImplementedException();
            }
        }

        public class TestPortableFactory : IPortableFactory
        {
            public IPortable Create(int classId)
            {
                switch (classId)
                {
                    case TestSerializationConstants.MAIN_PORTABLE:
                        return new MainPortable();
                    case TestSerializationConstants.INNER_PORTABLE:
                        return new InnerPortable();
                    case TestSerializationConstants.NAMED_PORTABLE:
                        return new NamedPortable();
                    case TestSerializationConstants.RAW_DATA_PORTABLE:
                        return new RawDataPortable();
                    case TestSerializationConstants.INVALID_RAW_DATA_PORTABLE:
                        return new InvalidRawDataPortable();
                    case TestSerializationConstants.INVALID_RAW_DATA_PORTABLE_2:
                        return new InvalidRawDataPortable2();
                    case TestSerializationConstants.OBJECT_CARRYING_PORTABLE:
                        return new ObjectCarryingPortable();
                }
                return null;
            }
        }

        //https://github.com/hazelcast/hazelcast/issues/1096
        [Test]
        public void Test_1096_ByteArrayContentSame()
        {
            var ss = new SerializationServiceBuilder()
                .AddPortableFactory(TestSerializationConstants.PORTABLE_FACTORY_ID, new TestPortableFactory()).Build();

            AssertRepeatedSerialisationGivesSameByteArrays(ss, new NamedPortable("issue-1096", 1096));

            AssertRepeatedSerialisationGivesSameByteArrays(ss, new InnerPortable(new byte[3], new char[5], new short[2],
                new int[10], new long[7], new float[9], new double[1], new[] {new NamedPortable("issue-1096", 1096)}));

            AssertRepeatedSerialisationGivesSameByteArrays(ss,
                new RawDataPortable(1096L, "issue-1096".ToCharArray(), new NamedPortable("issue-1096", 1096), 1096,
                    "issue-1096", new ByteArrayDataSerializable(new byte[1])));
        }

        //https://github.com/hazelcast/hazelcast/issues/2172
        [Test]
        public void Test_Issue2172_WritePortableArray()
        {
            var ss = new SerializationServiceBuilder().SetInitialOutputBufferSize(16).Build();
            var testObject2s = new TestObject2[100];
            for (var i = 0; i < testObject2s.Length; i++)
            {
                testObject2s[i] = new TestObject2();
            }

            var testObject1 = new TestObject1(testObject2s);
            ss.ToData(testObject1);
        }

        [Test, TestCaseSource("ByteOrders")]
        public void TestBasics(ByteOrder byteOrder)
        {
            var ss = CreateSerializationService(1, byteOrder);
            var ss2 = CreateSerializationService(2, byteOrder);

            var nn = new NamedPortable[5];
            for (var i = 0; i < nn.Length; i++)
            {
                nn[i] = new NamedPortable("named-portable-" + i, i);
            }

            var inner = new InnerPortable(new byte[] {0, 1, 2}, new[] {'c', 'h', 'a', 'r'},
                new short[] {3, 4, 5}, new[] {9, 8, 7, 6}, new long[] {0, 1, 5, 7, 9, 11},
                new[] {0.6543f, -3.56f, 45.67f}, new[] {456.456, 789.789, 321.321}, nn);

            var main = new MainPortable(113, true, 'x', -500, 56789, -50992225L, 900.5678f, -897543.3678909d,
                "this is main portable object created for testing!", inner);

            var data = ss.ToData(main);

            Assert.AreEqual(main, ss.ToObject<MainPortable>(data));
            Assert.AreEqual(main, ss2.ToObject<MainPortable>(data));
        }

        [Test]
        public void TestClassDefinitionConfig()
        {
            int portableVersion = 1;
            var serializationConfig = new SerializationConfig();
            serializationConfig.AddPortableFactory(TestSerializationConstants.PORTABLE_FACTORY_ID,
                new TestPortableFactory());
            serializationConfig.SetPortableVersion(portableVersion);
            serializationConfig
                .AddClassDefinition(
                    new ClassDefinitionBuilder(TestSerializationConstants.PORTABLE_FACTORY_ID,
                            TestSerializationConstants.RAW_DATA_PORTABLE, portableVersion)
                        .AddLongField("l")
                        .AddCharArrayField("c")
                        .AddPortableField("p", CreateNamedPortableClassDefinition(portableVersion))
                        .Build())
                .AddClassDefinition(
                    new ClassDefinitionBuilder(TestSerializationConstants.PORTABLE_FACTORY_ID,
                            TestSerializationConstants.NAMED_PORTABLE, portableVersion)
                        .AddUTFField("name").AddIntField("myint").Build());

            var serializationService = new SerializationServiceBuilder()
                .SetConfig(serializationConfig)
                .AddDataSerializableFactory(TestSerializationConstants.DATA_SERIALIZABLE_FACTORY_ID,
                    GetDataSerializableFactory())
                .Build();
            var p = new RawDataPortable(DateTime.Now.ToFileTime(), "test chars".ToCharArray(),
                new NamedPortable("named portable", 34567),
                9876, "Testing raw portable", new ByteArrayDataSerializable(Encoding.UTF8.GetBytes("test bytes")));

            var data = serializationService.ToData(p);
            Assert.AreEqual(p, serializationService.ToObject<RawDataPortable>(data));
        }

        [Test]
        public void TestClassDefinitionConfigWithErrors()
        {
            int portableVersion = 1;
            var serializationConfig = new SerializationConfig();
            serializationConfig.AddPortableFactory(TestSerializationConstants.PORTABLE_FACTORY_ID,
                new TestPortableFactory());
            serializationConfig.SetPortableVersion(1);
            serializationConfig.AddClassDefinition(
                new ClassDefinitionBuilder(TestSerializationConstants.PORTABLE_FACTORY_ID,
                        TestSerializationConstants.RAW_DATA_PORTABLE, 1)
                    .AddLongField("l")
                    .AddCharArrayField("c")
                    .AddPortableField("p", CreateNamedPortableClassDefinition(1))
                    .Build());

            try
            {
                new SerializationServiceBuilder().SetConfig(serializationConfig).Build();
                Assert.Fail("Should throw HazelcastSerializationException!");
            }
            catch (HazelcastSerializationException)
            {
            }

            new SerializationServiceBuilder().SetConfig(serializationConfig).SetCheckClassDefErrors(false).Build();

            // -- OR --

            serializationConfig.SetCheckClassDefErrors(false);
            new SerializationServiceBuilder().SetConfig(serializationConfig).Build();
        }

        [Test]
        public void TestRawData()
        {
            int portableVersion = 1;
            var serializationService = CreateSerializationService(1, ByteOrder.BigEndian);
            var p = new RawDataPortable(DateTime.Now.ToFileTime(), "test chars".ToCharArray(),
                new NamedPortable("named portable", 34567),
                9876, "Testing raw portable", new ByteArrayDataSerializable(Encoding.UTF8.GetBytes("test bytes")));
            var builder = new ClassDefinitionBuilder(p.GetFactoryId(), p.GetClassId(), portableVersion);
            builder.AddLongField("l").AddCharArrayField("c")
                .AddPortableField("p", CreateNamedPortableClassDefinition(portableVersion));
            serializationService.GetPortableContext().RegisterClassDefinition(builder.Build());

            var data = serializationService.ToData(p);
            Assert.AreEqual(p, serializationService.ToObject<RawDataPortable>(data));
        }

        [Test]
        public void TestRawDataInvalidWrite()
        {
            int portableVersion = 1;
            Assert.Throws<HazelcastSerializationException>(() =>
            {
                var serializationService = CreateSerializationService(1, ByteOrder.BigEndian);
                var p = new InvalidRawDataPortable(DateTime.Now.ToFileTime(), "test chars".ToCharArray(),
                    new NamedPortable("named portable", 34567),
                    9876, "Testing raw portable", new ByteArrayDataSerializable(Encoding.UTF8.GetBytes("test bytes")));
                var builder = new ClassDefinitionBuilder(p.GetFactoryId(), p.GetClassId(), portableVersion);
                builder.AddLongField("l").AddCharArrayField("c")
                    .AddPortableField("p", CreateNamedPortableClassDefinition(portableVersion));
                serializationService.GetPortableContext().RegisterClassDefinition(builder.Build());

                var data = serializationService.ToData(p);
                Assert.AreEqual(p, serializationService.ToObject<RawDataPortable>(data));
            });
        }

        [Test]
        public void TestRawDataInvaliRead()
        {
            int portableVersion = 1;
            Assert.Throws<HazelcastSerializationException>(() =>
            {
                var serializationService = CreateSerializationService(1, ByteOrder.BigEndian);
                var p = new InvalidRawDataPortable2(DateTime.Now.ToFileTime(), "test chars".ToCharArray(),
                    new NamedPortable("named portable", 34567),
                    9876, "Testing raw portable", new ByteArrayDataSerializable(Encoding.UTF8.GetBytes("test bytes")));
                var builder = new ClassDefinitionBuilder(p.GetFactoryId(), p.GetClassId(), portableVersion);
                builder.AddLongField("l").AddCharArrayField("c")
                    .AddPortableField("p", CreateNamedPortableClassDefinition(portableVersion));
                serializationService.GetPortableContext().RegisterClassDefinition(builder.Build());

                var data = serializationService.ToData(p);
                Assert.AreEqual(p, serializationService.ToObject<RawDataPortable>(data));
            });
        }

        [Test]
        public void TestRawDataWithoutRegistering()
        {
            var serializationService = CreateSerializationService(1, ByteOrder.BigEndian);
            var p = new RawDataPortable(DateTime.Now.ToFileTime(), "test chars".ToCharArray(),
                new NamedPortable("named portable", 34567),
                9876, "Testing raw portable", new ByteArrayDataSerializable(Encoding.UTF8.GetBytes("test bytes")));

            var data = serializationService.ToData(p);
            Assert.AreEqual(p, serializationService.ToObject<RawDataPortable>(data));
        }

        [Test]
        public void TestSerializationService_CreatePortableReader()
        {
            var serializationService = new SerializationServiceBuilder().Build();

            var timestamp1 = TestSupport.RandomLong();
            var child = new ChildPortableObject(timestamp1);
            var timestamp2 = TestSupport.RandomLong();
            var parent = new ParentPortableObject(timestamp2, child);
            var timestamp3 = timestamp1 + timestamp2;
            var grandParent = new GrandParentPortableObject(timestamp3, parent);

            var data = serializationService.ToData(grandParent);
            var reader = serializationService.CreatePortableReader(data);

            Assert.AreEqual(grandParent.timestamp, reader.ReadLong("timestamp"));
            Assert.AreEqual(parent.timestamp, reader.ReadLong("child.timestamp"));
            Assert.AreEqual(child.timestamp, reader.ReadLong("child.child.timestamp"));
        }

        [Test]
        public void TestWriteDataWithPortable()
        {
            var ss = new SerializationServiceBuilder()
                .AddPortableFactory(TestSerializationConstants.PORTABLE_FACTORY_ID,
                    new PortableFactoryFunc(i => new NamedPortableV2()))
                .AddDataSerializableFactory(TestSerializationConstants.DATA_SERIALIZABLE_FACTORY_ID,
                    GetDataSerializableFactory())
                .Build();

            var ss2 = new SerializationServiceBuilder()
                .AddPortableFactory(TestSerializationConstants.PORTABLE_FACTORY_ID,
                    new PortableFactoryFunc(i => new NamedPortable()))
                .AddDataSerializableFactory(TestSerializationConstants.DATA_SERIALIZABLE_FACTORY_ID,
                    GetDataSerializableFactory())
                .SetPortableVersion(5)
                .Build();

            IPortable p1 = new NamedPortableV2("test", 456, 500);
            object o1 = new DataDataSerializable(ss.ToData(p1));

            var data = ss.ToData(o1);

            var o2 = ss2.ToObject<DataDataSerializable>(data);
            Assert.AreEqual(o1, o2);

            var p2 = ss2.ToObject<IPortable>(o2.Data);
            Assert.AreEqual(p1, p2);
        }

        [Test]
        public void TestWriteObjectWithCustomSerializable()
        {
            var config = new SerializationConfig();
            var sc = new SerializerConfig()
                .SetImplementation(new CustomSerializer())
                .SetTypeClass(typeof(CustomSerializableType));
            config.AddSerializerConfig(sc);
            var serializationService =
                new SerializationServiceBuilder().SetPortableVersion(1)
                    .AddPortableFactory(TestSerializationConstants.PORTABLE_FACTORY_ID, new TestPortableFactory())
                    .SetConfig(config).Build();

            var foo = new CustomSerializableType {Value = "foo"};

            var objectCarryingPortable1 = new ObjectCarryingPortable(foo);
            var data = serializationService.ToData(objectCarryingPortable1);
            var objectCarryingPortable2 = serializationService.ToObject<ObjectCarryingPortable>(data);
            Assert.AreEqual(objectCarryingPortable1, objectCarryingPortable2);
        }

        [Test]
        public void TestWriteObjectWithIdentifiedDataSerializable()
        {
            var serializationService = CreateSerializationService(1, ByteOrder.NativeOrder());

            var serializable = new SampleIdentifiedDataSerializable('c', 2);
            var objectCarryingPortable1 = new ObjectCarryingPortable(serializable);
            var data = serializationService.ToData(objectCarryingPortable1);
            var objectCarryingPortable2 = serializationService.ToObject<ObjectCarryingPortable>(data);
            Assert.AreEqual(objectCarryingPortable1, objectCarryingPortable2);
        }

        [Test]
        public void TestWriteObjectWithPortable()
        {
            var ss = new SerializationServiceBuilder()
                .AddPortableFactory(TestSerializationConstants.PORTABLE_FACTORY_ID,
                    new PortableFactoryFunc(i => new NamedPortableV2()))
                .AddDataSerializableFactory(TestSerializationConstants.DATA_SERIALIZABLE_FACTORY_ID,
                    GetDataSerializableFactory())
                .Build();

            var ss2 = new SerializationServiceBuilder()
                .AddPortableFactory(TestSerializationConstants.PORTABLE_FACTORY_ID,
                    new PortableFactoryFunc(i => new NamedPortable()))
                .AddDataSerializableFactory(TestSerializationConstants.DATA_SERIALIZABLE_FACTORY_ID,
                    GetDataSerializableFactory())
                .SetPortableVersion(5)
                .Build();

            var o1 = new ComplexDataSerializable(new NamedPortableV2("test", 123, 500),
                new ByteArrayDataSerializable(new byte[3]), null);
            var data = ss.ToData(o1);

            object o2 = ss2.ToObject<ComplexDataSerializable>(data);
            Assert.AreEqual(o1, o2);
        }

        [Test]
        public void TestWriteReadWithNullPortableArray()
        {
            int portableVersion = 1;
            var builder0 = new ClassDefinitionBuilder(TestSerializationConstants.PORTABLE_FACTORY_ID, 1, portableVersion);
            var builder1 = new ClassDefinitionBuilder(TestSerializationConstants.PORTABLE_FACTORY_ID, 2, portableVersion);
            builder0.AddPortableArrayField("list", builder1.Build());

            var ss = new SerializationServiceBuilder()
                .SetPortableVersion(portableVersion)
                .AddClassDefinition(builder0.Build())
                .AddClassDefinition(builder1.Build())
                .Build();

            var data = ss.ToData(new TestObject1());

            var ss2 = new SerializationServiceBuilder()
                .AddPortableFactory(1, new PortableFactoryFunc(classId =>
                {
                    switch (classId)
                    {
                        case 1:
                            return new TestObject1();
                        case 2:
                            return new TestObject2();
                    }
                    return null;
                })).Build();
            var obj = ss2.ToObject<object>(data);
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf<TestObject1>(obj);
        }
    }

    public class PortableFactoryFunc : IPortableFactory
    {
        private readonly Func<int, IPortable> _func;

        public PortableFactoryFunc(Func<int, IPortable> func)
        {
            _func = func;
        }

        public IPortable Create(int classId)
        {
            return _func(classId);
        }
    }

    public class GrandParentPortableObject : IPortable
    {
        internal ParentPortableObject child;
        internal long timestamp;

        public GrandParentPortableObject(long timestamp)
        {
            this.timestamp = timestamp;
            child = new ParentPortableObject(timestamp);
        }

        public GrandParentPortableObject(long timestamp, ParentPortableObject child)
        {
            this.timestamp = timestamp;
            this.child = child;
        }

        public virtual int GetFactoryId()
        {
            return 1;
        }

        public virtual int GetClassId()
        {
            return 1;
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual void ReadPortable(IPortableReader reader)
        {
            timestamp = reader.ReadLong("timestamp");
            child = reader.ReadPortable<ParentPortableObject>("child");
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("timestamp", timestamp);
            writer.WritePortable("child", child);
        }
    }

    public class ParentPortableObject : IPortable
    {
        internal ChildPortableObject child;
        internal long timestamp;

        public ParentPortableObject(long timestamp)
        {
            this.timestamp = timestamp;
            child = new ChildPortableObject(timestamp);
        }

        public ParentPortableObject(long timestamp, ChildPortableObject child)
        {
            this.timestamp = timestamp;
            this.child = child;
        }

        public virtual int GetFactoryId()
        {
            return 2;
        }

        public virtual int GetClassId()
        {
            return 2;
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual void ReadPortable(IPortableReader reader)
        {
            timestamp = reader.ReadLong("timestamp");
            child = reader.ReadPortable<ChildPortableObject>("child");
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("timestamp", timestamp);
            writer.WritePortable("child", child);
        }
    }

    public class ChildPortableObject : IPortable
    {
        internal long timestamp;

        public ChildPortableObject(long timestamp)
        {
            this.timestamp = timestamp;
        }

        public virtual int GetFactoryId()
        {
            return 3;
        }

        public virtual int GetClassId()
        {
            return 3;
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual void ReadPortable(IPortableReader reader)
        {
            timestamp = reader.ReadLong("timestamp");
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("timestamp", timestamp);
        }
    }
}