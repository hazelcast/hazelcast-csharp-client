using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    public interface ISerializationService
    {
        IBufferObjectDataInput CreateObjectDataInput(byte[] data);
        IBufferObjectDataInput CreateObjectDataInput(IData data);
        IBufferObjectDataOutput CreateObjectDataOutput(int size);

        /// <exception cref="System.IO.IOException"></exception>
        IPortableReader CreatePortableReader(IData data);

        void Destroy();
        void DisposeData(IData data);
        ByteOrder GetByteOrder();
        IManagedContext GetManagedContext();
        IPortableContext GetPortableContext();
        byte GetVersion();
        T ReadObject<T>(IObjectDataInput input);
        IData ToData(object obj);
        IData ToData(object obj, IPartitioningStrategy strategy);
        T ToObject<T>(object data);
        void WriteObject(IObjectDataOutput output, object obj);
    }
}