using System.IO;
using Hazelcast.Core;

namespace Hazelcast.IO.Serialization
{
    internal interface ISerializationService
    {
        Data ToData(object obj);

        Data ToData(object obj, IPartitioningStrategy strategy);

        T ToObject<T>(object input);

        void WriteObject(IObjectDataOutput objectDataOutput, object obj);

        object ReadObject(IObjectDataInput objectDataInput);

        IBufferObjectDataInput CreateObjectDataInput(byte[] data);

        IBufferObjectDataInput CreateObjectDataInput(Data data);

        IBufferObjectDataOutput CreateObjectDataOutput(int size);

        ObjectDataOutputStream CreateObjectDataOutputStream(BinaryWriter binaryWriter);

        ObjectDataInputStream CreateObjectDataInputStream(BinaryReader binaryReader);

        ObjectDataOutputStream CreateObjectDataOutputStream(BinaryWriter binaryWriter, bool bigEndian);

        ObjectDataInputStream CreateObjectDataInputStream(BinaryReader binaryReader, bool bigEndian);

        void Register<T>(ISerializer serializer);

        void RegisterGlobal(ISerializer serializer);

        IPortableContext GetPortableContext();

        IPortableReader CreatePortableReader(Data data);
    }
}