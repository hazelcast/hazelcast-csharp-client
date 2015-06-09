using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class SemaphoreAcquireCodec
	{
		public static readonly SemaphoreMessageType RequestType = SemaphoreMessageType.SemaphoreAcquire;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly SemaphoreMessageType Type = RequestType;

			public string name;

			public int permits;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, int permits)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, int permits)
		{
			int requiredDataSize = SemaphoreAcquireCodec.RequestParameters.CalculateDataSize(name, permits);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(permits);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreAcquireCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			SemaphoreAcquireCodec.RequestParameters parameters = new SemaphoreAcquireCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			int permits;
			permits = clientMessage.GetInt();
			parameters.permits = permits;
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
			int requiredDataSize = SemaphoreAcquireCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreAcquireCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			SemaphoreAcquireCodec.ResponseParameters parameters = new SemaphoreAcquireCodec.ResponseParameters();
			return parameters;
		}
	}
}
