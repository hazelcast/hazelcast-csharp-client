using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientDestroyProxyCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientDestroyproxy;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ClientMessageType Type = RequestType;

			public string name;

			public string serviceName;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string serviceName)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(serviceName);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string serviceName)
		{
			int requiredDataSize = ClientDestroyProxyCodec.RequestParameters.CalculateDataSize(name, serviceName);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(serviceName);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientDestroyProxyCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientDestroyProxyCodec.RequestParameters parameters = new ClientDestroyProxyCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			string serviceName;
			serviceName = null;
			serviceName = clientMessage.GetStringUtf8();
			parameters.serviceName = serviceName;
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
			int requiredDataSize = ClientDestroyProxyCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientDestroyProxyCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientDestroyProxyCodec.ResponseParameters parameters = new ClientDestroyProxyCodec.ResponseParameters();
			return parameters;
		}
	}
}
