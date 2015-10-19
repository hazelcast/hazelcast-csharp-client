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
        public virtual void TestReadWrite(ByteOrder byteOrder)
        {
            var obj = KitchenSinkDataSerializable.Generate();
            obj.Serializable = KitchenSinkDataSerializable.Generate();

            var ss = new SerializationServiceBuilder().SetUseNativeByteOrder(false).SetByteOrder(byteOrder).Build();
            
            IObjectDataOutput output = ss.CreateObjectDataOutput(1024);
            output.WriteObject(obj);

            IObjectDataInput input = ss.CreateObjectDataInput(output.ToByteArray());
            var readObj = input.ReadObject<object>();
            Assert.AreEqual(obj, readObj);

            ss.Destroy();
        }
    }
}