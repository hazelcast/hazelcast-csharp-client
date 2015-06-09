using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapAddIndexCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapAddindex;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public string attribute;

			public bool ordered;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string attribute, bool ordered)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(attribute);
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string attribute, bool ordered)
		{
			int requiredDataSize = MapAddIndexCodec.RequestParameters.CalculateDataSize(name, attribute, ordered);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(attribute);
			clientMessage.Set(ordered);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapAddIndexCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapAddIndexCodec.RequestParameters parameters = new MapAddIndexCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			string attribute;
			attribute = null;
			attribute = clientMessage.GetStringUtf8();
			parameters.attribute = attribute;
			bool ordered;
			ordered = clientMessage.GetBoolean();
			parameters.ordered = ordered;
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
			int requiredDataSize = MapAddIndexCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapAddIndexCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapAddIndexCodec.ResponseParameters parameters = new MapAddIndexCodec.ResponseParameters();
			return parameters;
		}
	}
}
