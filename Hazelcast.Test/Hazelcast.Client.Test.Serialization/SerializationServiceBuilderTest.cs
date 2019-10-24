// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    public class SerializationServiceBuilderTest
    {
        [Test]
        public void AddDataSerializableFactory()
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
        public void AddDataSerializableFactoryClass()
        {
            var service1 = new SerializationServiceBuilder().Build();
            var data = service1.ToData(new DataSerializableBasicType());

            var config = new SerializationConfig();
            config.AddDataSerializableFactoryClass(1, typeof (MyDataSerializableFactory));
            var service = new SerializationServiceBuilder().SetConfig(config).Build();

            var obj = service.ToObject<object>(data);

            Assert.IsInstanceOf<DataSerializableBasicType>(obj);
        }

        [Test]
        public void AddDataSerializableFactoryClassWithBadId()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var config = new SerializationConfig();
                config.AddDataSerializableFactoryClass(-1, typeof(MyDataSerializableFactory));
                var service = new SerializationServiceBuilder().SetConfig(config).Build();
            });
        }

        [Test]
		public void AddDataSerializableFactoryClassWithDuplicateId()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            var config = new SerializationConfig();
            config.AddDataSerializableFactory(1, new MyDataSerializableFactory());
            config.AddDataSerializableFactoryClass(1, typeof (MyDataSerializableFactory));
            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        });
		}

        [Test]
		public void AddDataSerializableFactoryClassWithNoEmptyConstructor()
		{
			Assert.Throws<HazelcastSerializationException>(() =>
        {
            var config = new SerializationConfig();
            config.AddDataSerializableFactoryClass(1, typeof (SerializableFactory));
            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        });
		}

        [Test]
		public void AddDataSerializableFactoryWitDuplicateId()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            var config = new SerializationConfig();
            config.AddDataSerializableFactory(1, new MyDataSerializableFactory());
            var service = new SerializationServiceBuilder().SetConfig(config).
                AddDataSerializableFactory(1, new MyDataSerializableFactory()).Build();
        });
		}

        [Test]
		public void AddDataSerializableFactoryWithBadId()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            var config = new SerializationConfig();
            config.AddDataSerializableFactory(-1, new MyDataSerializableFactory());
            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        });
		}

        [Test]
        public void AddPortableFactory()
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
        public void AddPortableFactoryClass()
        {
            var service1 = new SerializationServiceBuilder().Build();
            var data = service1.ToData(new KitchenSinkPortable());

            var config = new SerializationConfig();
            config.AddPortableFactoryClass(1, typeof (KitchenSinkPortableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();

            var obj = service.ToObject<object>(data);
            Assert.IsInstanceOf<KitchenSinkPortable>(obj);
        }

        [Test]
		public void AddPortableFactoryClassWhichDoesNotImplementPortableFactory()
		{
			Assert.Throws<HazelcastSerializationException>(() =>
        {
            var config = new SerializationConfig();
            config.AddPortableFactoryClass(1, typeof (SerializableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        });
		}

        [Test]
		public void AddPortableFactoryClassWithBadId()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            var config = new SerializationConfig();
            config.AddPortableFactoryClass(-1, typeof (KitchenSinkPortableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        });
		}

        [Test]
		public void AddPortableFactoryClassWithDuplicateId()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            var config = new SerializationConfig();
            config.AddPortableFactory(1, new KitchenSinkPortableFactory());
            config.AddPortableFactoryClass(1, typeof (KitchenSinkPortableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        });
		}

        [Test]
		public void AddPortableFactoryClassWithNoEmptyConstructor()
		{
			Assert.Throws<MissingMethodException>(() =>
        {
            var config = new SerializationConfig();
            config.AddPortableFactoryClass(1, typeof (PortableFactory));

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        });
		}

        [Test]
		public void AddPortableFactoryWithBadId()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            var config = new SerializationConfig();
            config.AddPortableFactory(-1, new KitchenSinkPortableFactory());

            var service = new SerializationServiceBuilder().SetConfig(config).Build();
        });
		}

        [Test]
		public void AddPortableFactoryWithDuplicateId()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            var config = new SerializationConfig();
            config.AddPortableFactory(1, new KitchenSinkPortableFactory());

            var service = new SerializationServiceBuilder().AddPortableFactory(1,
                new KitchenSinkPortableFactory()).SetConfig(config).Build();
        });
		}

        public void HazelcastInstanceAware()
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