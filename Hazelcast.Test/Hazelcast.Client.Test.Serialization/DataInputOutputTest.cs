using System.IO;
using System.Text;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    internal class DataInputOutputTest
    {
        private static readonly ByteOrder[] ByteOrders =
        {
            ByteOrder.BigEndian, ByteOrder.LittleEndian,
            ByteOrder.NativeOrder()
        };

        [TestCaseSource("ByteOrders")]
        public virtual void TestDataInputOutputWithPortable(ByteOrder byteOrder)
        {
            var portable = KitchenSinkPortable.Generate();

            var config = new SerializationConfig();
            config.AddPortableFactoryClass(KitchenSinkPortableFactory.FactoryId, typeof (KitchenSinkPortableFactory));

            var ss = new SerializationServiceBuilder().SetConfig(config).
                SetUseNativeByteOrder(false).SetByteOrder(byteOrder).Build();

            IObjectDataOutput output = ss.CreateObjectDataOutput(1024);
            output.WriteObject(portable);
            var data = output.ToByteArray();

            IObjectDataInput input = ss.CreateObjectDataInput(data);
            var readObject = input.ReadObject<IPortable>();

            Assert.AreEqual(portable, readObject);

            ss.Destroy();
        }

        [TestCaseSource("ByteOrders")]
        public virtual void TestInputOutputWithPortableReader(ByteOrder byteOrder)
        {
            var portable = KitchenSinkPortable.Generate();

            var config = new SerializationConfig();
            config.AddPortableFactoryClass(KitchenSinkPortableFactory.FactoryId, typeof(KitchenSinkPortableFactory));

            var ss = new SerializationServiceBuilder().SetConfig(config).
                SetUseNativeByteOrder(false).SetByteOrder(byteOrder).Build();

            var data = ss.ToData(portable);
            var reader  = ss.CreatePortableReader(data);

            var actual = new KitchenSinkPortable();
            actual.ReadPortable(reader);

            Assert.AreEqual(portable, actual);

            ss.Destroy();
        }
        

        [TestCaseSource("ByteOrders")]
        public virtual void TestDataStreamsWithDataSerializable(ByteOrder byteOrder)
        {
            var obj = KitchenSinkDataSerializable.Generate();
            obj.Serializable = KitchenSinkDataSerializable.Generate();

            var ss = new SerializationServiceBuilder().SetUseNativeByteOrder(false).SetByteOrder(byteOrder).Build();
            
            var stream = new MemoryStream();
            IObjectDataOutput output = ss.CreateObjectDataOutputStream(stream);
            output.WriteObject(obj);
            var data1 = stream.ToArray();
            IObjectDataOutput out2 = ss.CreateObjectDataOutput(1024);
            out2.WriteObject(obj);
            var data2 = out2.ToByteArray();
            Assert.AreEqual(data1.Length, data2.Length);
            Assert.AreEqual(data1, data2);

            IObjectDataInput input = ss.CreateObjectDataInputStream(new MemoryStream(data2));
            var object1 = input.ReadObject<object>();
            IObjectDataInput in2 = ss.CreateObjectDataInput(data1);
            var object2 = in2.ReadObject<object>();
            Assert.AreEqual(obj, object1);
            Assert.AreEqual(obj, object2);

            ss.Destroy();
        }
    }
}