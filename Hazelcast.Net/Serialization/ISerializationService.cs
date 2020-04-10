using System;
using System.Collections.Generic;
using System.Text;

namespace Hazelcast.Serialization
{
    public interface ISerializationService
    {
        //IBufferObjectDataInput CreateObjectDataInput(byte[] data);
        //IBufferObjectDataInput CreateObjectDataInput(IData data);
        //IBufferObjectDataOutput CreateObjectDataOutput(int size);

        /// <exception cref="System.IO.IOException"></exception>
        //IPortableReader CreatePortableReader(IData data);

        void Destroy();
        void DisposeData(IData data);
        //ByteOrder GetByteOrder();
        //IPortableContext GetPortableContext();
        byte GetVersion();
        //T ReadObject<T>(IObjectDataInput input);
        IData ToData(object obj);
        //IData ToData(object obj, IPartitioningStrategy strategy);
        T ToObject<T>(object data);
        object ToObject(object data);
        //void WriteObject(IObjectDataOutput output, object obj);
        //IBufferObjectDataOutput CreateObjectDataOutput();
    }
}
