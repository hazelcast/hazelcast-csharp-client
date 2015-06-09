using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class LockIsLockedByCurrentThreadCodec
	{
		public static readonly LockMessageType RequestType = LockMessageType.LockIslockedbycurrentthread;

		public const int ResponseType = 101;

		public const bool Retryable = true;

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
			int requiredDataSize = LockIsLockedByCurrentThreadCodec.RequestParameters.CalculateDataSize(name, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static LockIsLockedByCurrentThreadCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			LockIsLockedByCurrentThreadCodec.RequestParameters parameters = new LockIsLockedByCurrentThreadCodec.RequestParameters();
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
			int requiredDataSize = LockIsLockedByCurrentThreadCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static LockIsLockedByCurrentThreadCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			LockIsLockedByCurrentThreadCodec.ResponseParameters parameters = new LockIsLockedByCurrentThreadCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
