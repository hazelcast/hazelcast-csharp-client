using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ConditionAwaitCodec
	{
		public static readonly ConditionMessageType RequestType = ConditionMessageType.ConditionAwait;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ConditionMessageType Type = RequestType;

			public string name;

			public long threadId;

			public long timeout;

			public string lockName;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, long threadId, long timeout, string lockName)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.LongSizeInBytes;
				dataSize += Bits.LongSizeInBytes;
				dataSize += ParameterUtil.CalculateStringDataSize(lockName);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, long threadId, long timeout, string lockName)
		{
			int requiredDataSize = ConditionAwaitCodec.RequestParameters.CalculateDataSize(name, threadId, timeout, lockName);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(threadId);
			clientMessage.Set(timeout);
			clientMessage.Set(lockName);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ConditionAwaitCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ConditionAwaitCodec.RequestParameters parameters = new ConditionAwaitCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
			long timeout;
			timeout = clientMessage.GetLong();
			parameters.timeout = timeout;
			string lockName;
			lockName = null;
			lockName = clientMessage.GetStringUtf8();
			parameters.lockName = lockName;
			return parameters;
		}

		public class ResponseParameters
		{
			public bool response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(bool response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(bool response)
		{
			int requiredDataSize = ConditionAwaitCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ConditionAwaitCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ConditionAwaitCodec.ResponseParameters parameters = new ConditionAwaitCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
