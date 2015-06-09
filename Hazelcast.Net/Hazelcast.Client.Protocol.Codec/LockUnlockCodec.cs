using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class LockUnlockCodec
	{
		public static readonly LockMessageType RequestType = LockMessageType.LockUnlock;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly LockMessageType Type = RequestType;

			public string name;

			public long threadId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, long threadId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, long threadId)
		{
			int requiredDataSize = LockUnlockCodec.RequestParameters.CalculateDataSize(name, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static LockUnlockCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			LockUnlockCodec.RequestParameters parameters = new LockUnlockCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
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
			int requiredDataSize = LockUnlockCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static LockUnlockCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			LockUnlockCodec.ResponseParameters parameters = new LockUnlockCodec.ResponseParameters();
			return parameters;
		}
	}
}
