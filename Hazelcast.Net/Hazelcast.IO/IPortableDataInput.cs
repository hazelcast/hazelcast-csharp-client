using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    internal interface IPortableDataInput : IBufferObjectDataInput
    {
        ByteBuffer GetHeaderBuffer();
    }
}