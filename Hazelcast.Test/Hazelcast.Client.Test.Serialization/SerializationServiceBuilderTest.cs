// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    public class SerializationServiceBuilderTest
    {
        [Test]
        public void TestAddDataSerializableFactory()
        {
            var service1 = new SerializationServiceBuilder().Build();
            var data = service1.ToData(new DataSerializableBasicType());

            var config = new SerializationConfig();
            config.AddDataSerializableFactory(1, new MyDataSerializableFactory());
            var service = new SerializationServiceBuilder().SetConfig(config).Build();

            var obj = service.ToObject<object>(data);

            Assert.IsInstanceOf<DataSerializableBasicType>(obj);
        }

        [Test]
        public void TestAddDataSerializableFactoryClass()
        {
            var service1 = new SerializationServiceBuilder().Build();
            var data = service1.ToData(new DataSerializableBasicType());

            var config = new SerializationConfig();
            config.AddDataSerializableFactoryClass(1, typeof(MyDataSerializableFactory));
            var service = new SerializationServiceBuilder().SetConfig(config).Build();

            var obj = service.ToObject<object>(data);

            Assert.IsInstanceOf<DataSerializableBasicType>(obj);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddDataSerializableFactoryWithBadId()
        {
            var config = new SerializationConfig();
            config.AddDataSerializableFactory(-1, new MyDataSerializableFactory());
            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddDataSerializableFactoryClassWithBadId()
        {
            var config = new SerializationConfig();
            config.AddDataSerializableFactoryClass(-1, typeof(MyDataSerializableFactory));
            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddDataSerializableFactoryWitDuplicateId()
        {
            var config = new SerializationConfig();
            config.AddDataSerializableFactory(1, new MyDataSerializableFactory());
            var service = new SerializationServiceBuilder().SetConfig(config).
                AddDataSerializableFactory(1, new MyDataSerializableFactory()).Build();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddDataSerializableFactoryClassWithDuplicateId()
        {
            var config = new SerializationConfig();
            config.AddDataSerializableFactory(1, new MyDataSerializableFactory());
            config.AddDataSerializableFactoryClass(1, typeof(MyDataSerializableFactory));
            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        }

        [Test, ExpectedException(typeof(HazelcastSerializationException))]
        public void TestAddDataSerializableFactoryClassWithNoEmptyConstructor()
        {
            var config = new SerializationConfig();
            config.AddDataSerializableFactoryClass(1, typeof(SerializableFactory));
            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        }

        [Test]
        public void TestAddPortableFactory()
        {
            var service1 = new SerializationServiceBuilder().Build();
            var data = service1.ToData(new KitchenSinkPortable());

            var config = new SerializationConfig();
            config.AddPortableFactory(1, new KitchenSinkPortableFactory());

            var service = new SerializationServiceBuilder().SetConfig(config).Build();

            var obj = service.ToObject<object>(data);
            Assert.IsInstanceOf<KitchenSinkPortable>(obj);
        }

        [Test]
        public void TestAddPortableFactoryClass()
        {
            var service1 = new SerializationServiceBuilder().Build();
            var data = service1.ToData(new KitchenSinkPortable());

            var config = new SerializationConfig();
            config.AddPortableFactoryClass(1, typeof(KitchenSinkPortableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();

            var obj = service.ToObject<object>(data);
            Assert.IsInstanceOf<KitchenSinkPortable>(obj);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddPortableFactoryWithBadId()
        {
            var config = new SerializationConfig();
            config.AddPortableFactory(-1, new KitchenSinkPortableFactory());

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddPortableFactoryClassWithBadId()
        {
            var config = new SerializationConfig();
            config.AddPortableFactoryClass(-1, typeof(KitchenSinkPortableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddPortableFactoryWithDuplicateId()
        {
            var config = new SerializationConfig();
            config.AddPortableFactory(1, new KitchenSinkPortableFactory());

            var service = new SerializationServiceBuilder().AddPortableFactory(1,
            new KitchenSinkPortableFactory()).SetConfig(config).Build();
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TestAddPortableFactoryClassWithDuplicateId()
        {
            var config = new SerializationConfig();
            config.AddPortableFactory(1, new KitchenSinkPortableFactory());
            config.AddPortableFactoryClass(1, typeof(KitchenSinkPortableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        }

        [Test, ExpectedException(typeof(MissingMethodException))]
        public void TestAddPortableFactoryClassWithNoEmptyConstructor()
        {
            var config = new SerializationConfig();
            config.AddPortableFactoryClass(1, typeof(PortableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        }

        [Test, ExpectedException(typeof(HazelcastSerializationException))]
        public void TestAddPortableFactoryClassWhichDoesNotImplementPortableFactory()
        {
            var config = new SerializationConfig();
            config.AddPortableFactoryClass(1, typeof(SerializableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
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