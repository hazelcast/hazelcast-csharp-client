using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Serialization.Portable;

namespace Hazelcast.Serialization
{
    public interface ISerializationService
    {
        IBufferObjectDataInput CreateObjectDataInput(byte[] data);
        IBufferObjectDataInput CreateObjectDataInput(IData data);
        IBufferObjectDataOutput CreateObjectDataOutput(int size);
        IPortableReader CreatePortableReader(IData data);
        void Destroy();
        void DisposeData(IData data);
        Endianness Endianness { get; }
        IPortableContext GetPortableContext();
        byte GetVersion();
        T ReadObject<T>(IObjectDataInput input);
        IData ToData(object obj);
        IData ToData(object obj, IPartitioningStrategy strategy);
        T ToObject<T>(object data);
        object ToObject(object data);
        void WriteObject(IObjectDataOutput output, object obj);
        IBufferObjectDataOutput CreateObjectDataOutput();
    }
}
