using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    [TestFixture]
    public class ClientCustomSerializationTest
    {
        [Test]
        public virtual void TestCustomSerialize()
        {
            var config = new SerializationConfig();

            var sc = new SerializerConfig()
                .SetImplementation(new CustomSerializer())
                .SetTypeClass(typeof (CustomSerializableType));

            config.AddSerializerConfig(sc);
            var ss = new SerializationServiceBuilder().SetConfig(config).Build();

            var foo = new CustomSerializableType {Value = "fooooo"};

            var d = ss.ToData(foo);
            var newFoo = ss.ToObject<CustomSerializableType>(d);

            Assert.AreEqual(newFoo.Value, foo.Value);
        }

        [Test]
        public void TestGlobalSerializer()
        {
            var config = new SerializationConfig();
            var globalConfig = new GlobalSerializerConfig();

            globalConfig.SetClassName(typeof (GlobalSerializer).AssemblyQualifiedName);
            config.SetGlobalSerializerConfig(globalConfig);

            var ss = new SerializationServiceBuilder().SetConfig(config).Build();

            var foo = new CustomSerializableType {Value = "fooooo"};

            var d = ss.ToData(foo);
            var newFoo = ss.ToObject<CustomSerializableType>(d);

            Assert.AreEqual(newFoo.Value, foo.Value);
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
        public int GetTypeId()
        {
            return 10;
        }

        public void Destroy()
        {
            //NOOP
        }

        public void Write(IObjectDataOutput output, CustomSerializableType t)
        {
            byte[] array;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, t);
                array = ms.ToArray();
            }

            output.WriteInt(array.Length);
            output.Write(array);
        }

        public CustomSerializableType Read(IObjectDataInput input)
        {
            var bf = new BinaryFormatter();
            var len = input.ReadInt();

            var buffer = new byte[len];
            input.ReadFully(buffer);

            CustomSerializableType result = null;
            using (var ms = new MemoryStream(buffer))
            {
                result = (CustomSerializableType) bf.Deserialize(ms);
            }
            return result;
        }
    }

    public class GlobalSerializer : IStreamSerializer<object>
    {
        public int GetTypeId()
        {
            return 20;
        }

        public void Destroy()
        {
        }

        public void Write(IObjectDataOutput output, object obj)
        {
            if (!(obj is CustomSerializableType)) throw new ArgumentException("Unexpected type " + obj.GetType());
            
            new CustomSerializer().Write(output, (CustomSerializableType)obj);
        }

        public object Read(IObjectDataInput input)
        {
            return new CustomSerializer().Read(input);
        }
    }
}