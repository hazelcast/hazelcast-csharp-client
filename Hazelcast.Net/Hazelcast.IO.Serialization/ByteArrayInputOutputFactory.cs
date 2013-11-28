namespace Hazelcast.IO.Serialization
{
    internal sealed class ByteArrayInputOutputFactory : IInputOutputFactory
    {
        public IBufferObjectDataInput CreateInput(Data data, ISerializationService service)
        {
            return new ByteArrayObjectDataInput(data, service);
        }

        public IBufferObjectDataInput CreateInput(byte[] buffer, ISerializationService service)
        {
            return new ByteArrayObjectDataInput(buffer, service);
        }

        public IBufferObjectDataOutput CreateOutput(int size, ISerializationService service)
        {
            return new ByteArrayObjectDataOutput(size, service);
        }
    }
}