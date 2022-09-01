﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    [TestFixture]
    public class ClientCustomSerializationTest
    {
        [Test]
        public virtual void TestCustomSerializer()
        {
            var config = new SerializationOptions();

            var sc = new SerializerOptions
            {
                SerializedType = typeof (CustomSerializableType),
                Creator = () => new CustomSerializer()
            };

            config.Serializers.Add(sc);
            var ss = new SerializationServiceBuilder(config, new NullLoggerFactory()).Build();

            var foo = new CustomSerializableType {Value = "fooooo"};

            var d = ss.ToData(foo);
            var newFoo = ss.ToObject<CustomSerializableType>(d);

            Assert.AreEqual(newFoo.Value, foo.Value);
        }

        [Test]
        public void TestGlobalSerializer()
        {
            var options = new SerializationOptions
            {
                GlobalSerializer = new GlobalSerializerOptions
                {
                    Creator = () => ServiceFactory.CreateInstance<ISerializer, GlobalSerializer>()
                }
            };

            var ss = new SerializationServiceBuilder(options, new NullLoggerFactory()).Build();

            var foo = new CustomSerializableType {Value = "fooooo"};

            var d = ss.ToData(foo);
            var newFoo = ss.ToObject<CustomSerializableType>(d);

            Assert.AreEqual(newFoo.Value, foo.Value);
        }

        [Test]
        public void TestGlobalSerializerOverride()
        {
            var config = new SerializationOptions();
            var globalListSerializer = new GlobalListSerializer();
            config.GlobalSerializer = new GlobalSerializerOptions
            {
                Creator = () => globalListSerializer,
                OverrideClrSerialization = true
            };

            var ss = new SerializationServiceBuilder(config, new NullLoggerFactory()).Build();

            var list = new List<string> {"foo", "bar"};

            var d = ss.ToData(list);
            var input = ss.CreateObjectDataInput(d);

            var actual = (List<string>)globalListSerializer.Read(input);

            Assert.AreEqual(list, actual);
        }
    }

    [Serializable]
    internal class CustomSerializableType
    {
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CustomSerializableType) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        protected bool Equals(CustomSerializableType other)
        {
            return string.Equals(Value, other.Value);
        }
    }


    internal class CustomSerializer : IStreamSerializer<CustomSerializableType>
    {
        public int TypeId => 10;

        public void Dispose()
        { }

        // this is a test, it's OK to use BinaryFormatter here

        public void Write(IObjectDataOutput output, CustomSerializableType t)
        {
#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete and should not be used
            byte[] array;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, t);
                array = ms.ToArray();
            }

            output.WriteInt(array.Length);
            output.Write(array);
#pragma warning restore SYSLIB0011
        }

        public CustomSerializableType Read(IObjectDataInput input)
        {
#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete and should not be used
            var bf = new BinaryFormatter();
            var len = input.ReadInt();

            var buffer = new byte[len];
            input.Read(buffer);

            CustomSerializableType result = null;
            using (var ms = new MemoryStream(buffer))
            {
                result = (CustomSerializableType) bf.Deserialize(ms);
            }
            return result;
#pragma warning restore SYSLIB0011
        }
    }

    public class GlobalSerializer : IStreamSerializer<object>
    {
        public int TypeId => 20;

        public void Dispose()
        { }

        public void Write(IObjectDataOutput output, object obj)
        {
            if (!(obj is CustomSerializableType)) throw new ArgumentException("Unexpected type " + obj.GetType());

            new CustomSerializer().Write(output, (CustomSerializableType) obj);
        }

        public object Read(IObjectDataInput input)
        {
            return new CustomSerializer().Read(input);
        }
    }

    public class GlobalListSerializer : IStreamSerializer<object>
    {
        public int TypeId => 50;

        public void Dispose()
        { }

        public void Write(IObjectDataOutput output, object obj)
        {
            if (obj is IList<string>)
            {
                IList<string> list = (IList<string>) obj;
                output.WriteInt(list.Count);
                foreach (var o in list)
                {
                    output.WriteString(o);
                }
            }
        }

        public object Read(IObjectDataInput input)
        {
            int size = input.ReadInt();
            List<string> list = new List<string>(size);
            for (int i = 0; i < size; i++)
            {
                list.Add(input.ReadString());
            }
            return list;
        }
    }
}
