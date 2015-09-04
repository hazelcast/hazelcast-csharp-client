using System.IO;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    internal class DataInputOutputTest
    {
        private readonly Person person = new Person(111, 123L,
            89.56d, "test-person", new Address("street", 987));

        [Test]
        public virtual void TestDataStreamsBigEndian()
        {
            TestDataStreams(person, ByteOrder.BigEndian);
        }

        [Test]
        public virtual void TestDataStreamsLittleEndian()
        {
            TestDataStreams(person, ByteOrder.LittleEndian);
        }

        [Test]
        public virtual void TestDataStreamsNativeOrder()
        {
            TestDataStreams(person, ByteOrder.NativeOrder());
        }

        protected internal virtual SerializationServiceBuilder CreateSerializationServiceBuilder()
        {
            return new SerializationServiceBuilder();
        }

        private void TestDataStreams(object obj, ByteOrder byteOrder)
        {
            var ss = CreateSerializationServiceBuilder().SetUseNativeByteOrder(false).SetByteOrder(byteOrder).Build();
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            IObjectDataOutput output = ss.CreateObjectDataOutputStream(writer);
            output.WriteObject(obj);
            var data1 = stream.ToArray();
            IObjectDataOutput out2 = ss.CreateObjectDataOutput(1024);
            out2.WriteObject(obj);
            var data2 = out2.ToByteArray();
            Assert.AreEqual(data1.Length, data2.Length);
            Assert.AreEqual(data1, data2);

            var bin = new BinaryReader(new MemoryStream(data2));
            IObjectDataInput input = ss.CreateObjectDataInputStream(bin);
            var object1 = input.ReadObject<object>();
            IObjectDataInput in2 = ss.CreateObjectDataInput(data1);
            var object2 = in2.ReadObject<object>();
            Assert.AreEqual(obj, object1);
            Assert.AreEqual(obj, object2);
        }
    }
}