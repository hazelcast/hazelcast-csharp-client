using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ConditionSignalAllCodec
	{
		public static readonly ConditionMessageType RequestType = ConditionMessageType.ConditionSignalall;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ConditionMessageType Type = RequestType;

			public string name;

			public long threadId;

			public string lockName;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, long threadId, string lockName)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.LongSizeInBytes;
				dataSize += ParameterUtil.CalculateStringDataSize(lockName);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, long threadId, string lockName)
		{
			int requiredDataSize = ConditionSignalAllCodec.RequestParameters.CalculateDataSize(name, threadId, lockName);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(threadId);
			clientMessage.Set(lockName);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ConditionSignalAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ConditionSignalAllCodec.RequestParameters parameters = new ConditionSignalAllCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
			string lockName;
			lockName = null;
			lockName = clientMessage.GetStringUtf8();
			parameters.lockName = lockName;
			return parameters;
		}

		public class ResponseParameters
		{
			//************************ RESPONSE *************************//
			public static int CalculateDataSize()
			{
				int dataSize = ClientMessage.HeaderSize;
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse()
		{
			int requiredDataSize = ConditionSignalAllCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ConditionSignalAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ConditionSignalAllCodec.ResponseParameters parameters = new ConditionSignalAllCodec.ResponseParameters();
			return parameters;
		}
	}
}
