namespace Hazelcast.IO.Serialization
{
    internal interface IInputOutputFactory
    {
        IBufferObjectDataInput CreateInput(Data data, ISerializationService service);

        IBufferObjectDataInput CreateInput(byte[] buffer, ISerializationService service);

        IBufferObjectDataOutput CreateOutput(int size, ISerializationService service);
    }
}