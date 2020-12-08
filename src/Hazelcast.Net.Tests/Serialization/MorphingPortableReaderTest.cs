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
    public class MorphingPortableReaderTest
    {
        private IPortableReader reader;
        private SerializationService service1;
        private SerializationService service2;

        [TearDown]
        public virtual void After()
        {
            service1.Dispose();
            service2.Dispose();
        }

        [SetUp]
        public virtual void Before()
        {
            service1 = (SerializationService) new SerializationServiceBuilder(new NullLoggerFactory())
                .AddPortableFactory(SerializationTestsConstants.PORTABLE_FACTORY_ID, new PortableFactoryFunc(
                    i => new MorphingPortableBase())).Build();
            service2 = (SerializationService) new SerializationServiceBuilder(new NullLoggerFactory())
                .AddPortableFactory(SerializationTestsConstants.PORTABLE_FACTORY_ID,
                    new PortableFactoryFunc(
                        i => new MorphingPortable())).Build();
            var data = service1.ToData(new MorphingPortableBase(unchecked(1), true, (char) 2, 3, 4, 5, 1f, 2d, "test"));
            var input = service2.CreateObjectDataInput(data);
            var portableSerializer = service2.PortableSerializer;
            reader = portableSerializer.CreateMorphingReader(input);
        }

        [Test]
        public virtual void TestReadBoolean()
        {
            var aBoolean = reader.ReadBoolean("boolean");
            Assert.IsTrue(aBoolean);
            Assert.IsFalse(reader.ReadBoolean("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadBoolean_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadBoolean("string"); });
        }

        [Test]
        public virtual void TestReadByte()
        {
            var aByte = reader.ReadByte("byte");
            Assert.AreEqual(1, aByte);
            Assert.AreEqual(0, reader.ReadByte("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadByte_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadByte("string"); });
        }

        [Test]
        public virtual void TestReadByteArray()
        {
            Assert.IsNull(reader.ReadByteArray("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadByteArray_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadByteArray("byte"); });
        }

        [Test]
        public virtual void TestReadChar()
        {
            var aChar = reader.ReadChar("char");
            Assert.AreEqual(2, aChar);
            Assert.AreEqual(0, reader.ReadChar("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadChar_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadChar("string"); });
        }

        [Test]
        public virtual void TestReadCharArray()
        {
            Assert.IsNull(reader.ReadCharArray("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadCharArray_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadCharArray("byte"); });
        }

        [Test]
        public virtual void TestReadDouble()
        {
            var aByte = reader.ReadDouble("byte");
            var aShort = reader.ReadDouble("short");
            var aChar = reader.ReadDouble("char");
            var aInt = reader.ReadDouble("int");
            var aFloat = reader.ReadDouble("float");
            var aLong = reader.ReadDouble("long");
            var aDouble = reader.ReadDouble("double");
            Assert.AreEqual(1, aByte, 0);
            Assert.AreEqual(3, aShort, 0);
            Assert.AreEqual(2, aChar, 0);
            Assert.AreEqual(4, aInt, 0);
            Assert.AreEqual(5, aLong, 0);
            Assert.AreEqual(1f, aFloat, 0);
            Assert.AreEqual(2d, aDouble, 0);
            Assert.AreEqual(0, reader.ReadDouble("NO SUCH FIELD"), 0);
        }

        [Test]
        public void TestReadDouble_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadDouble("string"); });
        }

        [Test]
        public virtual void TestReadDoubleArray()
        {
            Assert.IsNull(reader.ReadDoubleArray("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadDoubleArray_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadByteArray("byte"); });
        }

        [Test]
        public virtual void TestReadFloat()
        {
            var aByte = reader.ReadFloat("byte");
            var aShort = reader.ReadFloat("short");
            var aChar = reader.ReadFloat("char");
            var aInt = reader.ReadFloat("int");
            var aFloat = reader.ReadFloat("float");
            Assert.AreEqual(1, aByte, 0);
            Assert.AreEqual(3, aShort, 0);
            Assert.AreEqual(2, aChar, 0);
            Assert.AreEqual(4, aInt, 0);
            Assert.AreEqual(1f, aFloat, 0);
            Assert.AreEqual(0, reader.ReadFloat("NO SUCH FIELD"), 0);
        }

        [Test]
        public void TestReadFloat_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadFloat("string"); });
        }

        [Test]
        public virtual void TestReadFloatArray()
        {
            Assert.IsNull(reader.ReadFloatArray("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadFloatArray_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadByteArray("byte"); });
        }

        [Test]
        public virtual void TestReadInt()
        {
            var aByte = reader.ReadInt("byte");
            var aShort = reader.ReadInt("short");
            var aChar = reader.ReadInt("char");
            var aInt = reader.ReadInt("int");
            Assert.AreEqual(1, aByte);
            Assert.AreEqual(3, aShort);
            Assert.AreEqual(2, aChar);
            Assert.AreEqual(4, aInt);
            Assert.AreEqual(0, reader.ReadInt("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadInt_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadInt("string"); });
        }

        [Test]
        public virtual void TestReadIntArray()
        {
            Assert.IsNull(reader.ReadIntArray("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadIntArray_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadIntArray("byte"); });
        }

        [Test]
        public virtual void TestReadLong()
        {
            var aByte = reader.ReadLong("byte");
            var aShort = reader.ReadLong("short");
            var aChar = reader.ReadLong("char");
            var aInt = reader.ReadLong("int");
            var aLong = reader.ReadLong("long");
            Assert.AreEqual(1, aByte);
            Assert.AreEqual(3, aShort);
            Assert.AreEqual(2, aChar);
            Assert.AreEqual(4, aInt);
            Assert.AreEqual(5, aLong);
            Assert.AreEqual(0, reader.ReadLong("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadLong_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadLong("string"); });
        }

        [Test]
        public virtual void TestReadLongArray()
        {
            Assert.IsNull(reader.ReadLongArray("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadLongArray_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadByteArray("byte"); });
        }

        [Test]
        public virtual void TestReadPortable()
        {
            Assert.IsNull(reader.ReadPortable<MorphingPortable>("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadPortable_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadByteArray("byte"); });
        }

        [Test]
        public virtual void TestReadPortableArray()
        {
            Assert.IsNull(reader.ReadPortableArray<MorphingPortable>("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadPortableArray_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadByteArray("byte"); });
        }

        [Test]
        public virtual void TestReadShort()
        {
            int aByte = reader.ReadShort("byte");
            int aShort = reader.ReadShort("short");
            Assert.AreEqual(1, aByte);
            Assert.AreEqual(3, aShort);
            Assert.AreEqual(0, reader.ReadShort("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadShort_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadShort("string"); });
        }

        [Test]
        public virtual void TestReadShortArray()
        {
            Assert.IsNull(reader.ReadShortArray("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadShortArray_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadByteArray("byte"); });
        }

        [Test]
        public virtual void TestReadUTF()
        {
            var aString = reader.ReadUTF("string");
            Assert.AreEqual("test", aString);
            Assert.IsNull(reader.ReadUTF("NO SUCH FIELD"));
        }

        [Test]
        public void TestReadUTF_IncompatibleClass()
        {
            Assert.Throws<InvalidPortableFieldException>(() => { reader.ReadUTF("byte"); });
        }
    }
}
