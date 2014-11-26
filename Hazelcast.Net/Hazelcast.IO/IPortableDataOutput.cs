
namespace Hazelcast.IO
{
	internal interface IPortableDataOutput : IBufferObjectDataOutput
	{
		DynamicByteBuffer GetHeaderBuffer();

		byte[] GetPortableHeader();
	}
}
