using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
	internal interface SocketWritable
	{
		bool WriteTo(ByteBuffer destination);

		void OnEnqueue();
	}
}
