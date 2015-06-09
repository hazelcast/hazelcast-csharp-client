using Hazelcast.Client.Protocol;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientRemoveAllListenersCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientRemovealllisteners;

		public const int ResponseType = 100;

		public const bool Retryable = false;

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
			int requiredDataSize = ClientRemoveAllListenersCodec.RequestParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientRemoveAllListenersCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientRemoveAllListenersCodec.RequestParameters parameters = new ClientRemoveAllListenersCodec.RequestParameters();
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
			int requiredDataSize = ClientRemoveAllListenersCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientRemoveAllListenersCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientRemoveAllListenersCodec.ResponseParameters parameters = new ClientRemoveAllListenersCodec.ResponseParameters();
			return parameters;
		}
	}
}
