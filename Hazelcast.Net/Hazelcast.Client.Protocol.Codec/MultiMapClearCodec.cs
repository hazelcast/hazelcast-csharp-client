using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MultiMapClearCodec
	{
		public static readonly MultiMapMessageType RequestType = MultiMapMessageType.MultimapClear;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MultiMapMessageType Type = RequestType;

			public string name;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name)
		{
			int requiredDataSize = MultiMapClearCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapClearCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MultiMapClearCodec.RequestParameters parameters = new MultiMapClearCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
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
			int requiredDataSize = MultiMapClearCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapClearCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MultiMapClearCodec.ResponseParameters parameters = new MultiMapClearCodec.ResponseParameters();
			return parameters;
		}
	}
}
