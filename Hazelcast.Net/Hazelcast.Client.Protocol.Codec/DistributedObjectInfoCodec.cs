using Hazelcast.Client.Protocol.Util;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class DistributedObjectInfoCodec
	{
		public static DistributedObjectInfo Decode(IClientMessage clientMessage)
		{
			string serviceName = clientMessage.GetStringUtf8();
			string name = clientMessage.GetStringUtf8();
			return new DistributedObjectInfo(serviceName, name);
		}
	}
}
