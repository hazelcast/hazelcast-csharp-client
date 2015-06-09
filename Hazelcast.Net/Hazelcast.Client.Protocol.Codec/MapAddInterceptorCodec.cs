using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapAddInterceptorCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapAddinterceptor;

		public const int ResponseType = 104;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public IData interceptor;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData interceptor)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(interceptor);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData interceptor)
		{
			int requiredDataSize = MapAddInterceptorCodec.RequestParameters.CalculateDataSize(name, interceptor);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(interceptor);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapAddInterceptorCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapAddInterceptorCodec.RequestParameters parameters = new MapAddInterceptorCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData interceptor;
			interceptor = null;
			interceptor = clientMessage.GetData();
			parameters.interceptor = interceptor;
			return parameters;
		}

		public class ResponseParameters
		{
			public string response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(string response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(response);
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(string response)
		{
			int requiredDataSize = MapAddInterceptorCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapAddInterceptorCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapAddInterceptorCodec.ResponseParameters parameters = new MapAddInterceptorCodec.ResponseParameters();
			string response;
			response = null;
			response = clientMessage.GetStringUtf8();
			parameters.response = response;
			return parameters;
		}
	}
}
