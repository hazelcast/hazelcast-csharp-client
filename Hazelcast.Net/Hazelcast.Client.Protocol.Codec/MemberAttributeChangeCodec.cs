using Hazelcast.Client.Protocol.Util;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.IO;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MemberAttributeChangeCodec
	{
		private MemberAttributeChangeCodec()
		{
		}

		public static MemberAttributeChange Decode(ClientMessage clientMessage)
		{
			string uuid = clientMessage.GetStringUtf8();
			string key = clientMessage.GetStringUtf8();
			MemberAttributeOperationType operationType = (MemberAttributeOperationType)clientMessage.GetInt();
			object value = null;
			if (operationType == MemberAttributeOperationType.PUT)
			{
				value = clientMessage.GetStringUtf8();
			}
			return new MemberAttributeChange(uuid, operationType, key, value);
		}

		public static void Encode(MemberAttributeChange memberAttributeChange, ClientMessage clientMessage)
		{
			clientMessage.Set(memberAttributeChange.Uuid);
			clientMessage.Set(memberAttributeChange.Key);
			MemberAttributeOperationType operationType = memberAttributeChange.OperationType;
			clientMessage.Set((int)operationType);
			if (operationType == MemberAttributeOperationType.PUT)
			{
				clientMessage.Set(memberAttributeChange.Value.ToString());
			}
		}

		public static int CalculateDataSize(MemberAttributeChange memberAttributeChange)
		{
			if (memberAttributeChange == null)
			{
				return Bits.BooleanSizeInBytes;
			}
			int dataSize = ParameterUtil.CalculateStringDataSize(memberAttributeChange.Uuid);
			dataSize += ParameterUtil.CalculateStringDataSize(memberAttributeChange.Key);
			//operation type
			dataSize += Bits.IntSizeInBytes;
			MemberAttributeOperationType operationType = memberAttributeChange.OperationType;
			if (operationType == MemberAttributeOperationType.PUT)
			{
				dataSize += ParameterUtil.CalculateStringDataSize(memberAttributeChange.Value.ToString());
			}
			return dataSize;
		}
	}
}
