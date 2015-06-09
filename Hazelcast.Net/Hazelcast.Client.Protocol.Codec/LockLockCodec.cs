using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class LockLockCodec
	{
		public static readonly LockMessageType RequestType = LockMessageType.LockLock;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly LockMessageType Type = RequestType;

			public string name;

			public long leaseTime;

			public long threadId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, long leaseTime, long threadId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.LongSizeInBytes;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, long leaseTime, long threadId)
		{
			int requiredDataSize = LockLockCodec.RequestParameters.CalculateDataSize(name, leaseTime, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(leaseTime);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static LockLockCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			LockLockCodec.RequestParameters parameters = new LockLockCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			long leaseTime;
			leaseTime = clientMessage.GetLong();
			parameters.leaseTime = leaseTime;
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
			int requiredDataSize = LockLockCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static LockLockCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			LockLockCodec.ResponseParameters parameters = new LockLockCodec.ResponseParameters();
			return parameters;
		}
	}
}
