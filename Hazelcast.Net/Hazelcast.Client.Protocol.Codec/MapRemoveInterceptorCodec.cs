using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapRemoveInterceptorCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapRemoveinterceptor;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public string id;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string id)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(id);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string id)
		{
			int requiredDataSize = MapRemoveInterceptorCodec.RequestParameters.CalculateDataSize(name, id);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(id);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapRemoveInterceptorCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapRemoveInterceptorCodec.RequestParameters parameters = new MapRemoveInterceptorCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			string id;
			id = null;
			id = clientMessage.GetStringUtf8();
			parameters.id = id;
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
			int requiredDataSize = MapRemoveInterceptorCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapRemoveInterceptorCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapRemoveInterceptorCodec.ResponseParameters parameters = new MapRemoveInterceptorCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
