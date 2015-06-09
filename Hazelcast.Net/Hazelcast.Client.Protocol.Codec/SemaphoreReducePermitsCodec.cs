using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class SemaphoreReducePermitsCodec
	{
		public static readonly SemaphoreMessageType RequestType = SemaphoreMessageType.SemaphoreReducepermits;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly SemaphoreMessageType Type = RequestType;

			public string name;

			public int reduction;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, int reduction)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, int reduction)
		{
			int requiredDataSize = SemaphoreReducePermitsCodec.RequestParameters.CalculateDataSize(name, reduction);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(reduction);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreReducePermitsCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			SemaphoreReducePermitsCodec.RequestParameters parameters = new SemaphoreReducePermitsCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			int reduction;
			reduction = clientMessage.GetInt();
			parameters.reduction = reduction;
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
			int requiredDataSize = SemaphoreReducePermitsCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreReducePermitsCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			SemaphoreReducePermitsCodec.ResponseParameters parameters = new SemaphoreReducePermitsCodec.ResponseParameters();
			return parameters;
		}
	}
}
