using Hazelcast.Client.Protocol;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientPingCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientPing;

		public const int ResponseType = 100;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly ClientMessageType Type = RequestType;

			//************************ REQUEST *************************//
			public static int CalculateDataSize()
			{
				int dataSize = ClientMessage.HeaderSize;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest()
		{
			int requiredDataSize = ClientPingCodec.RequestParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientPingCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientPingCodec.RequestParameters parameters = new ClientPingCodec.RequestParameters();
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
			int requiredDataSize = ClientPingCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientPingCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientPingCodec.ResponseParameters parameters = new ClientPingCodec.ResponseParameters();
			return parameters;
		}
	}
}
