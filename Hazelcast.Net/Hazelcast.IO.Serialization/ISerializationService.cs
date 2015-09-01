using System;
using System.IO;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    public interface ISerializationService
    {
        IData ToData(object obj);

		IData ToData(object obj, IPartitioningStrategy strategy);

		T ToObject<T>(object data);

        void WriteObject(IObjectDataOutput output, object obj);

        T ReadObject<T>(IObjectDataInput input);

		void WriteData(IObjectDataOutput output, IData data);

        IData ReadData(IObjectDataInput input);

		void DisposeData(IData data);

		IBufferObjectDataInput CreateObjectDataInput(byte[] data);

		IBufferObjectDataInput CreateObjectDataInput(IData data);

		IBufferObjectDataOutput CreateObjectDataOutput(int size);

        ObjectDataOutputStream CreateObjectDataOutputStream(BinaryWriter binaryWriter);

        ObjectDataInputStream CreateObjectDataInputStream(BinaryReader binaryReader);

		void Register(Type type, ISerializer serializer);

		void RegisterGlobal(ISerializer serializer);

		IPortableContext GetPortableContext();

		/// <exception cref="System.IO.IOException"></exception>
		IPortableReader CreatePortableReader(IData data);

		IManagedContext GetManagedContext();

		ByteOrder GetByteOrder();

		void Destroy();
	}
}
