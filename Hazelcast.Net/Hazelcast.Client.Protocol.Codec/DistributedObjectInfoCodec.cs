using Hazelcast.Client.Protocol.Util;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class DistributedObjectInfoCodec
	{
		private DistributedObjectInfoCodec()
		{
		}

		public static DistributedObjectInfo Decode(ClientMessage clientMessage)
		{
			string serviceName = clientMessage.GetStringUtf8();
			string name = clientMessage.GetStringUtf8();
			return new DistributedObjectInfo(serviceName, name);
		}

		public static void Encode(DistributedObjectInfo info, ClientMessage clientMessage)
		{
			clientMessage.Set(info.GetServiceName()).Set(info.GetName());
		}

		public static int CalculateDataSize(DistributedObjectInfo info)
		{
			return ParameterUtil.CalculateStringDataSize(info.GetServiceName()) + ParameterUtil.CalculateStringDataSize(info.GetName());
		}
	}
}
