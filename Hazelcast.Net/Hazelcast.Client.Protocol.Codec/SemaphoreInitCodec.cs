using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class SemaphoreInitCodec
	{
		public static readonly SemaphoreMessageType RequestType = SemaphoreMessageType.SemaphoreInit;

		public const int ResponseType = 101;

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
			int requiredDataSize = SemaphoreInitCodec.RequestParameters.CalculateDataSize(name, permits);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(permits);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreInitCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			SemaphoreInitCodec.RequestParameters parameters = new SemaphoreInitCodec.RequestParameters();
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
			int requiredDataSize = SemaphoreInitCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreInitCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			SemaphoreInitCodec.ResponseParameters parameters = new SemaphoreInitCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
