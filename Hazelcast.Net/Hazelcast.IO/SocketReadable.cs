using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
	internal interface SocketReadable
	{
		bool ReadFrom(ByteBuffer source);
	}
}
