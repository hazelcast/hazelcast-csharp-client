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

using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    internal class PortableClassVersionTest
    {
        internal const int FactoryId = TestSerializationConstants.PORTABLE_FACTORY_ID;

        [Test]
        public virtual void TestDifferentClassAndServiceVersions()
        {
            var serializationService =
                new SerializationServiceBuilder().SetPortableVersion(1)
                    .AddPortableFactory(FactoryId, new PortableFactoryFunc(i => new NamedPortable()))
                    .Build();
            var serializationService2 =
                new SerializationServiceBuilder().SetPortableVersion(2)
                    .AddPortableFactory(FactoryId, new PortableFactoryFunc(i => new NamedPortableV2()))
                    .Build();
            TestDifferentClassVersions(serializationService, serializationService2);
        }

        /// <exception cref="System.IO.IOException" />
        [Test]
        public virtual void TestDifferentClassAndServiceVersionsUsingDataWriteAndRead()
        {
            var serializationService =
                new SerializationServiceBuilder().SetPortableVersion(1)
                    .AddPortableFactory(FactoryId, new PortableFactoryFunc(i => new NamedPortable()))
                    .Build();
            var serializationService2 =
                new SerializationServiceBuilder().SetPortableVersion(2)
                    .AddPortableFactory(FactoryId, new PortableFactoryFunc(i => new NamedPortableV2()))
                    .Build();
            TestDifferentClassVersionsUsingDataWriteAndRead(serializationService, serializationService2);
        }

        [Test]
        public virtual void TestDifferentClassVersions()
        {
            var serializationService =
                new SerializationServiceBuilder().AddPortableFactory(FactoryId,
                    new PortableFactoryFunc(i => new NamedPortable()))
                    .Build();
            var serializationService2 =
                new SerializationServiceBuilder().AddPortableFactory(FactoryId,
                    new PortableFactoryFunc(i => new NamedPortableV2()))
                    .Build();
            TestDifferentClassVersions(serializationService, serializationService2);
        }

        /// <exception cref="System.IO.IOException" />
        [Test]
        public virtual void TestDifferentClassVersionsUsingDataWriteAndRead()
        {
            var serializationService =
                new SerializationServiceBuilder().AddPortableFactory(FactoryId,
                    new PortableFactoryFunc(i => new NamedPortable()))
                    .Build();
            var serializationService2 =
                new SerializationServiceBuilder().AddPortableFactory(FactoryId,
                    new PortableFactoryFunc(i => new NamedPortableV2()))
                    .Build();
            TestDifferentClassVersionsUsingDataWriteAndRead(serializationService, serializationService2);
        }

        [Test]
        public virtual void TestPreDefinedDifferentVersionsWithInnerPortable()
        {
            var serializationService = PortableSerializationTest.CreateSerializationService(1);
            serializationService.GetPortableContext().RegisterClassDefinition(CreateInnerPortableClassDefinition());
            var serializationService2 = PortableSerializationTest.CreateSerializationService(2);
            serializationService2.GetPortableContext().RegisterClassDefinition(CreateInnerPortableClassDefinition());
            var nn = new NamedPortable[1];
            nn[0] = new NamedPortable("name", 123);
            var inner = new InnerPortable(new byte[] {0, 1, 2}, new[] {'c', 'h', 'a', 'r'}, new short[] {3, 4, 5},
                new[] {9, 8, 7, 6}, new long[] {0, 1, 5, 7, 9, 11}, new[] {0.6543f, -3.56f, 45.67f}, new[]
                {
                    456.456
                    , 789.789, 321.321
                }, nn);
            var mainWithInner = new MainPortable(unchecked(113), true, 'x', -500, 56789, -50992225L, 900.5678f,
                -897543.3678909d, "this is main portable object created for testing!", inner);
            TestPreDefinedDifferentVersions(serializationService, serializationService2, mainWithInner);
        }

        [Test]
        public virtual void TestPreDefinedDifferentVersionsWithNullInnerPortable()
        {
            var innerPortableClassDefinition = CreateInnerPortableClassDefinition();
            var serializationService = PortableSerializationTest.CreateSerializationService(1);
            serializationService.GetPortableContext().RegisterClassDefinition(innerPortableClassDefinition);
            var serializationService2 = PortableSerializationTest.CreateSerializationService(2);
            serializationService2.GetPortableContext().RegisterClassDefinition(innerPortableClassDefinition);
            var mainWithNullInner = new MainPortable(unchecked(113), true, 'x', -500, 56789, -50992225L, 900.5678f,
                -897543.3678909d, "this is main portable object created for testing!", null);
            TestPreDefinedDifferentVersions(serializationService, serializationService2, mainWithNullInner);
        }

        internal static IClassDefinition CreateInnerPortableClassDefinition()
        {
            var builder = new ClassDefinitionBuilder(FactoryId, TestSerializationConstants.INNER_PORTABLE);
            builder.AddByteArrayField("b");
            builder.AddCharArrayField("c");
            builder.AddShortArrayField("s");
            builder.AddIntArrayField("i");
            builder.AddLongArrayField("l");
            builder.AddFloatArrayField("f");
            builder.AddDoubleArrayField("d");
            builder.AddPortableArrayField("nn", PortableSerializationTest.CreateNamedPortableClassDefinition());
            return builder.Build();
        }

        internal static void TestDifferentClassVersions(ISerializationService serializationService,
            ISerializationService serializationService2)
        {
            var p1 = new NamedPortable("named-portable", 123);
            var data = serializationService.ToData(p1);
            var p2 = new NamedPortableV2("named-portable", 123);
            var data2 = serializationService2.ToData(p2);
            var o1 = serializationService2.ToObject<NamedPortableV2>(data);
            var o2 = serializationService.ToObject<NamedPortable>(data2);
            Assert.AreEqual(o1.name, o2.name);
        }

        /// <exception cref="System.IO.IOException" />
        internal static void TestDifferentClassVersionsUsingDataWriteAndRead(ISerializationService serializationService,
            ISerializationService serializationService2)
        {
            var p1 = new NamedPortable("portable-v1", 111);
            var data = serializationService.ToData(p1);
            // emulate socket write by writing data to stream
            var @out = serializationService.CreateObjectDataOutput(1024);
            @out.WriteData(data);
            var bytes = @out.ToByteArray();
            // emulate socket read by reading data from stream
            var @in = serializationService2.CreateObjectDataInput(bytes);
            data = @in.ReadData();
            // read data
            var object1 = serializationService2.ToObject<object>(data);
            // serialize new portable version
            var p2 = new NamedPortableV2("portable-v2", 123);
            var data2 = serializationService2.ToData(p2);
            // de-serialize back using old version
            var object2 = serializationService.ToObject<object>(data2);
            Assert.IsNotNull(object1, "object1 should not be null!");
            Assert.IsNotNull(object2, "object2 should not be null!");
            Assert.IsInstanceOf<NamedPortableV2>(object1, "Should be instance of NamedPortableV2: " + object1.GetType());
            Assert.IsInstanceOf<NamedPortable>(object2, "Should be instance of NamedPortable: " + object2.GetType());
        }

        internal static void TestPreDefinedDifferentVersions(ISerializationService serializationService,
            ISerializationService serializationService2, MainPortable mainPortable)
        {
            var data = serializationService.ToData(mainPortable);
            Assert.AreEqual(mainPortable, serializationService2.ToObject<MainPortable>(data));
        }
    }
}