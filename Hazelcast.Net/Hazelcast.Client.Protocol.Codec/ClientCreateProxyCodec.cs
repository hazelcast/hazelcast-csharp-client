using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientCreateProxyCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientCreateproxy;

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
			int requiredDataSize = ClientCreateProxyCodec.RequestParameters.CalculateDataSize(name, serviceName);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(serviceName);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientCreateProxyCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientCreateProxyCodec.RequestParameters parameters = new ClientCreateProxyCodec.RequestParameters();
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
			int requiredDataSize = ClientCreateProxyCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientCreateProxyCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientCreateProxyCodec.ResponseParameters parameters = new ClientCreateProxyCodec.ResponseParameters();
			return parameters;
		}
	}
}
