using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapLoadAllCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapLoadall;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public bool replaceExistingValues;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, bool replaceExistingValues)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, bool replaceExistingValues)
		{
			int requiredDataSize = MapLoadAllCodec.RequestParameters.CalculateDataSize(name, replaceExistingValues);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(replaceExistingValues);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapLoadAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapLoadAllCodec.RequestParameters parameters = new MapLoadAllCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			bool replaceExistingValues;
			replaceExistingValues = clientMessage.GetBoolean();
			parameters.replaceExistingValues = replaceExistingValues;
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
			int requiredDataSize = MapLoadAllCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapLoadAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapLoadAllCodec.ResponseParameters parameters = new MapLoadAllCodec.ResponseParameters();
			return parameters;
		}
	}
}
