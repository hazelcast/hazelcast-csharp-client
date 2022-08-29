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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void AssertGuidByteOrder()
        {
            var guid = new Guid("00010203-0405-0607-0809-0a0b0c0d0e0f");
            var a = guid.ToByteArray();

            // verify the order of Guid bytes in the byte array
            Assert.AreEqual(0x03, a[00]);
            Assert.AreEqual(0x02, a[01]);
            Assert.AreEqual(0x01, a[02]);
            Assert.AreEqual(0x00, a[03]);
            Assert.AreEqual(0x05, a[04]);
            Assert.AreEqual(0x04, a[05]);
            Assert.AreEqual(0x07, a[06]);
            Assert.AreEqual(0x06, a[07]);
            Assert.AreEqual(0x08, a[08]);
            Assert.AreEqual(0x09, a[09]);
            Assert.AreEqual(0x0a, a[10]);
            Assert.AreEqual(0x0b, a[11]);
            Assert.AreEqual(0x0c, a[12]);
            Assert.AreEqual(0x0d, a[13]);
            Assert.AreEqual(0x0e, a[14]);
            Assert.AreEqual(0x0f, a[15]);
        }

        [Test]
        public void SerializationInfoSupports()
        {
            var thing = new SerializableThing
            {
                GuidValue = Guid.NewGuid()
            };

            var bytes = SerializeToBytes(thing);
            var result = DeserializeFromBytes<SerializableThing>(bytes);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.GuidValue, Is.EqualTo(thing.GuidValue));
        }

        // this is a test, it's OK to use BinaryFormatter here

        private static byte[] SerializeToBytes<T>(T e)
            where T : ISerializable
        {
#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete and should not be used
            using var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, e);
            return stream.GetBuffer();
#pragma warning restore SYSLIB0011
        }

        private static T DeserializeFromBytes<T>(byte[] bytes)
            where T : ISerializable
        {
#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete and should not be used
            using var stream = new MemoryStream(bytes);
            return (T) new BinaryFormatter().Deserialize(stream);
#pragma warning restore SYSLIB0011
        }

        [Serializable]
        private class SerializableThing : ISerializable
        {
            public SerializableThing()
            { }

            public Guid GuidValue { get; set; }

            protected SerializableThing(SerializationInfo info, StreamingContext context)
            {
                GuidValue = info.GetGuid(nameof(GuidValue));
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null) throw new ArgumentNullException(nameof(info));

                info.AddValue(nameof(GuidValue), GuidValue);
            }
        }
    }
}
