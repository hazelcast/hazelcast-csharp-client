// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
    public class SerializationServiceBuilderTest
    {
        [Test]
        public void TestAddDataSerializableFactory()
        {
            var service1 = new SerializationServiceBuilder(new SerializationOptions(), new NullLoggerFactory()).Build();
            var data = service1.ToData(new DataSerializableBasicType());

            var options = new SerializationOptions();
            options.AddDataSerializableFactory(1, new MyDataSerializableFactory());
            var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();

            var obj = service.ToObject<object>(data);

            Assert.IsInstanceOf<DataSerializableBasicType>(obj);
        }

        [Test]
        public void TestAddDataSerializableFactoryClass()
        {
            var service1 = new SerializationServiceBuilder(new SerializationOptions(), new NullLoggerFactory()).Build();
            var data = service1.ToData(new DataSerializableBasicType());

            var options = new SerializationOptions();
            options.AddDataSerializableFactoryClass(1, typeof (MyDataSerializableFactory));
            var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();

            var obj = service.ToObject<object>(data);

            Assert.IsInstanceOf<DataSerializableBasicType>(obj);
        }

        [Test]
        public void TestAddDataSerializableFactoryClassWithBadId()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var options = new SerializationOptions();
                options.AddDataSerializableFactoryClass(-1, typeof(MyDataSerializableFactory));
                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();
            });
        }

        [Test]
        public void TestAddDataSerializableFactoryClassWithDuplicateId()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var options = new SerializationOptions();
                options.AddDataSerializableFactory(1, new MyDataSerializableFactory());
                options.AddDataSerializableFactoryClass(1, typeof (MyDataSerializableFactory));
                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();
            });
        }

        [Test]
        public void TestAddDataSerializableFactoryClassWithNoEmptyConstructor()
        {
            Assert.Throws<ServiceFactoryException>(() =>
            {
                var options = new SerializationOptions();
                options.AddDataSerializableFactoryClass(1, typeof (SerializableFactory));
                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();
            });
        }

        [Test]
        public void TestAddDataSerializableFactoryWitDuplicateId()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var options = new SerializationOptions();
                options.AddDataSerializableFactory(1, new MyDataSerializableFactory());
                var service = new SerializationServiceBuilder(options, new NullLoggerFactory())
                    .AddDataSerializableFactory(1, new MyDataSerializableFactory()).Build();
            });
        }

        [Test]
        public void TestAddDataSerializableFactoryWithBadId()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var options = new SerializationOptions();
                options.AddDataSerializableFactory(-1, new MyDataSerializableFactory());
                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();
            });
        }

        [Test]
        public void TestAddPortableFactory()
        {
            var service1 = new SerializationServiceBuilder(new SerializationOptions(), new NullLoggerFactory()).Build();
            var data = service1.ToData(new KitchenSinkPortable());

            var options = new SerializationOptions();
            options.AddPortableFactory(1, new KitchenSinkPortableFactory());

            var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();

            var obj = service.ToObject<object>(data);
            Assert.IsInstanceOf<KitchenSinkPortable>(obj);
        }

        [Test]
        public void TestAddPortableFactory2()
        {
            var service1 = new SerializationServiceBuilder(new SerializationOptions(), new NullLoggerFactory()).Build();
            var data = service1.ToData(new KitchenSinkPortable());

            var options = new SerializationOptions();
            options.AddPortableFactory(1, typeof (KitchenSinkPortableFactory));

            var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();

            var obj = service.ToObject<object>(data);
            Assert.IsInstanceOf<KitchenSinkPortable>(obj);
        }

        [Test]
        public void TestAddPortableFactoryWhichDoesNotImplementPortableFactory()
        {
            Assert.Throws<ServiceFactoryException>(() =>
            {
                var options = new SerializationOptions();
                options.AddPortableFactory(1, typeof (SerializableFactory));

                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();
            });
        }

        [Test]
        public void TestAddPortableFactoryWithBadId()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var options = new SerializationOptions();
                options.AddPortableFactory(-1, typeof (KitchenSinkPortableFactory));

                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();
            });
        }

        [Test]
        public void TestAddPortableFactoryWithDuplicateId()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var options = new SerializationOptions();
                options.AddPortableFactory(1, new KitchenSinkPortableFactory());
                options.AddPortableFactory(1, typeof (KitchenSinkPortableFactory));

                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();
            });
        }

        [Test]
        public void TestAddPortableFactoryWithNoEmptyConstructor()
        {
            Assert.Throws<ServiceFactoryException>(() =>
            {
                var options = new SerializationOptions();
                options.AddPortableFactory(1, typeof (PortableFactory));

                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();
            });
        }

        [Test]
        public void TestAddPortableFactory2WithBadId()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var options = new SerializationOptions();
                options.AddPortableFactory(-1, new KitchenSinkPortableFactory());

                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();
            });
        }

        [Test]
        public void TestAddPortableFactory2WithDuplicateId()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var options = new SerializationOptions();
                options.AddPortableFactory(1, new KitchenSinkPortableFactory());

                var service = new SerializationServiceBuilder(options, new NullLoggerFactory()).AddPortableFactory(1,
                    new KitchenSinkPortableFactory()).Build();
            });
        }

        public void TestHazelcastInstanceAware()
        {
        }


        private class SerializableFactory : IDataSerializableFactory
        {
            public SerializableFactory(int x)
            {
            }

            public IIdentifiedDataSerializable Create(int typeId)
            {
                throw new NotImplementedException();
            }
        }

        private class PortableFactory : IPortableFactory
        {
            public PortableFactory(int x)
            {
            }

            public IPortable Create(int classId)
            {
                throw new NotImplementedException();
            }
        }
    }
}
