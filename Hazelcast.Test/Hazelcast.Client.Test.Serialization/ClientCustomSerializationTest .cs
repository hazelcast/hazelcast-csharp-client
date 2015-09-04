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
    }

    [Serializable]
    class CustomSerializableType
    {
        public string Value
        {
            get; set;
        }

        protected bool Equals(CustomSerializableType other)
        {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomSerializableType) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }

    class CustomSerializer : IStreamSerializer<CustomSerializableType>
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
                result = (CustomSerializableType)bf.Deserialize(ms);
            }
            return result;
        }
    }

}
