using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MultiMapForceUnlockCodec
	{
		public static readonly MultiMapMessageType RequestType = MultiMapMessageType.MultimapForceunlock;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MultiMapMessageType Type = RequestType;

			public string name;

			public IData key;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData key)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(key);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData key)
		{
			int requiredDataSize = MultiMapForceUnlockCodec.RequestParameters.CalculateDataSize(name, key);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(key);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapForceUnlockCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MultiMapForceUnlockCodec.RequestParameters parameters = new MultiMapForceUnlockCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData key;
			key = null;
			key = clientMessage.GetData();
			parameters.key = key;
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
			int requiredDataSize = MultiMapForceUnlockCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapForceUnlockCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MultiMapForceUnlockCodec.ResponseParameters parameters = new MultiMapForceUnlockCodec.ResponseParameters();
			return parameters;
		}
	}
}
