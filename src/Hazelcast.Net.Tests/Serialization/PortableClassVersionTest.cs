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

using Hazelcast.Serialization;
using Hazelcast.Tests.Serialization.Objects;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    internal class PortableClassVersionTest
    {
        internal const int FactoryId = SerializationTestsConstants.PORTABLE_FACTORY_ID;

        [Test]
        public virtual void TestDifferentClassAndServiceVersions()
        {
            var serializationService =
                new SerializationServiceBuilder(new NullLoggerFactory()).SetPortableVersion(1)
                    .AddPortableFactory(FactoryId, new PortableFactoryFunc(i => new NamedPortable()))
                    .Build();
            var serializationService2 =
                new SerializationServiceBuilder(new NullLoggerFactory()).SetPortableVersion(2)
                    .AddPortableFactory(FactoryId, new PortableFactoryFunc(i => new NamedPortableV2()))
                    .Build();
            TestDifferentClassVersions(serializationService, serializationService2);
        }

        /// <exception cref="System.IO.IOException" />
        [Test]
        public virtual void TestDifferentClassAndServiceVersionsUsingDataWriteAndRead()
        {
            var serializationService =
                new SerializationServiceBuilder(new NullLoggerFactory()).SetPortableVersion(1)
                    .AddPortableFactory(FactoryId, new PortableFactoryFunc(i => new NamedPortable()))
                    .Build();
            var serializationService2 =
                new SerializationServiceBuilder(new NullLoggerFactory()).SetPortableVersion(2)
                    .AddPortableFactory(FactoryId, new PortableFactoryFunc(i => new NamedPortableV2()))
                    .Build();
            TestDifferentClassVersionsUsingDataWriteAndRead(serializationService, serializationService2);
        }

        [Test]
        public virtual void TestDifferentClassVersions()
        {
            var serializationService =
                new SerializationServiceBuilder(new NullLoggerFactory()).AddPortableFactory(FactoryId,
                    new PortableFactoryFunc(i => new NamedPortable()))
                    .Build();
            var serializationService2 =
                new SerializationServiceBuilder(new NullLoggerFactory()).AddPortableFactory(FactoryId,
                    new PortableFactoryFunc(i => new NamedPortableV2()))
                    .Build();
            TestDifferentClassVersions(serializationService, serializationService2);
        }

        /// <exception cref="System.IO.IOException" />
        [Test]
        public virtual void TestDifferentClassVersionsUsingDataWriteAndRead()
        {
            var serializationService =
                new SerializationServiceBuilder(new NullLoggerFactory()).AddPortableFactory(FactoryId,
                    new PortableFactoryFunc(i => new NamedPortable()))
                    .Build();
            var serializationService2 =
                new SerializationServiceBuilder(new NullLoggerFactory()).AddPortableFactory(FactoryId,
                    new PortableFactoryFunc(i => new NamedPortableV2()))
                    .Build();
            TestDifferentClassVersionsUsingDataWriteAndRead(serializationService, serializationService2);
        }

        [Test]
        public virtual void TestPreDefinedDifferentVersionsWithInnerPortable()
        {
            var serializationService = PortableSerializationTest.CreateSerializationService(1);
            serializationService.GetPortableContext().RegisterClassDefinition(CreateInnerPortableClassDefinition(1));
            var serializationService2 = PortableSerializationTest.CreateSerializationService(2);
            serializationService2.GetPortableContext().RegisterClassDefinition(CreateInnerPortableClassDefinition(2));
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
            var serializationService = PortableSerializationTest.CreateSerializationService(1);
            serializationService.GetPortableContext().RegisterClassDefinition(CreateInnerPortableClassDefinition(1));
            var serializationService2 = PortableSerializationTest.CreateSerializationService(2);
            serializationService2.GetPortableContext().RegisterClassDefinition(CreateInnerPortableClassDefinition(2));
            var mainWithNullInner = new MainPortable(unchecked(113), true, 'x', -500, 56789, -50992225L, 900.5678f,
                -897543.3678909d, "this is main portable object created for testing!", null);
            TestPreDefinedDifferentVersions(serializationService, serializationService2, mainWithNullInner);
        }

        internal static IClassDefinition CreateInnerPortableClassDefinition(int portableVersion)
        {
            var builder = new ClassDefinitionBuilder(FactoryId, SerializationTestsConstants.INNER_PORTABLE, portableVersion);
            builder.AddByteArrayField("b");
            builder.AddCharArrayField("c");
            builder.AddShortArrayField("s");
            builder.AddIntArrayField("i");
            builder.AddLongArrayField("l");
            builder.AddFloatArrayField("f");
            builder.AddDoubleArrayField("d");
            builder.AddPortableArrayField("nn", PortableSerializationTest.CreateNamedPortableClassDefinition(portableVersion));

            return builder.Build();
        }

        internal static void TestDifferentClassVersions(ISerializationService serializationService,
            ISerializationService serializationService2)
        {
            NamedPortable portableV1 = new NamedPortable("named-portable", 123);
            IData dataV1 = serializationService.ToData(portableV1);

            NamedPortableV2 portableV2 = new NamedPortableV2("named-portable", 123, 500);
            IData dataV2 = serializationService2.ToData(portableV2);

            NamedPortable v1FromV2 = serializationService.ToObject<NamedPortable>(dataV2);
            Assert.AreEqual(portableV2.name, v1FromV2.name);
            Assert.AreEqual(portableV2.k, v1FromV2.k);

            NamedPortableV2 v2FromV1 = serializationService2.ToObject<NamedPortableV2>(dataV1);
            Assert.AreEqual(portableV1.name, v2FromV1.name);
            Assert.AreEqual(portableV1.k, v2FromV1.k);

            Assert.AreEqual(v2FromV1.v, 0);
            //Assert.IsNull(v2FromV1.v);

        }

        /// <exception cref="System.IO.IOException" />
        internal static void TestDifferentClassVersionsUsingDataWriteAndRead(ISerializationService serializationService,
            ISerializationService serializationService2)
        {
            NamedPortable portableV1 = new NamedPortable("portable-v1", 111);
            IData dataV1 = serializationService.ToData(portableV1);


            // emulate socket write by writing data to stream
            var @out = serializationService.CreateObjectDataOutput(1024);
            @out.WriteData(dataV1);
            var bytes = @out.ToByteArray();
            // emulate socket read by reading data from stream
            var @in = serializationService2.CreateObjectDataInput(bytes);
            dataV1 = @in.ReadData();

            // serialize new portable version
            var portableV2 = new NamedPortableV2("portable-v2", 123, 500);
            var dataV2 = serializationService2.ToData(portableV2);

            NamedPortable v1FromV2 = serializationService.ToObject<NamedPortable>(dataV2);
            Assert.AreEqual(portableV2.name, v1FromV2.name);
            Assert.AreEqual(portableV2.k, v1FromV2.k);

            NamedPortableV2 v2FromV1 = serializationService2.ToObject<NamedPortableV2>(dataV1);
            Assert.AreEqual(portableV1.name, v2FromV1.name);
            Assert.AreEqual(portableV1.k, v2FromV1.k);

            Assert.AreEqual(v2FromV1.v, 0);
        }

        internal static void TestPreDefinedDifferentVersions(ISerializationService serializationService,
            ISerializationService serializationService2, MainPortable mainPortable)
        {
            var data = serializationService.ToData(mainPortable);
            Assert.AreEqual(mainPortable, serializationService2.ToObject<MainPortable>(data));
        }
    }
}