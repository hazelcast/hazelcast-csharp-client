using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class LockTryLockCodec
	{
		public static readonly LockMessageType RequestType = LockMessageType.LockTrylock;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly LockMessageType Type = RequestType;

			public string name;

			public long threadId;

			public long timeout;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, long threadId, long timeout)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.LongSizeInBytes;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, long threadId, long timeout)
		{
			int requiredDataSize = LockTryLockCodec.RequestParameters.CalculateDataSize(name, threadId, timeout);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(threadId);
			clientMessage.Set(timeout);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static LockTryLockCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			LockTryLockCodec.RequestParameters parameters = new LockTryLockCodec.RequestParameters();
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
			int requiredDataSize = LockTryLockCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static LockTryLockCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			LockTryLockCodec.ResponseParameters parameters = new LockTryLockCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
