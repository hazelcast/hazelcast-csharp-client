using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
	internal interface IInputOutputFactory
	{
		IBufferObjectDataInput CreateInput(IData data, ISerializationService service);

		IBufferObjectDataInput CreateInput(byte[] buffer, ISerializationService service);

		IBufferObjectDataOutput CreateOutput(int size, ISerializationService service);

		ByteOrder GetByteOrder();
	}
}
