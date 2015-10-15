using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class ByteArrayInputOutputFactory : IInputOutputFactory
    {
        private readonly ByteOrder byteOrder;

        public ByteArrayInputOutputFactory(ByteOrder byteOrder)
        {
            this.byteOrder = byteOrder;
        }

        public IBufferObjectDataInput CreateInput(IData data, ISerializationService service)
        {
            return new ByteArrayObjectDataInput(data.ToByteArray(), HeapData.DataOffset, service, byteOrder);
        }

        public IBufferObjectDataInput CreateInput(byte[] buffer, ISerializationService service)
        {
            return new ByteArrayObjectDataInput(buffer, service, byteOrder);
        }

        public IBufferObjectDataOutput CreateOutput(int size, ISerializationService service)
        {
            return new ByteArrayObjectDataOutput(size, service, byteOrder);
        }

        public ByteOrder GetByteOrder()
        {
            return byteOrder;
        }
    }
}