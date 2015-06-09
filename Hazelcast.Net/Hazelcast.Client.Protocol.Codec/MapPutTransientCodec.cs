using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapPutTransientCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapPuttransient;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public IData key;

			public IData value;

			public long threadId;

			public long ttl;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData key, IData value, long threadId, long ttl)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(key);
				dataSize += ParameterUtil.CalculateDataSize(value);
				dataSize += Bits.LongSizeInBytes;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData key, IData value, long threadId, long ttl)
		{
			int requiredDataSize = MapPutTransientCodec.RequestParameters.CalculateDataSize(name, key, value, threadId, ttl);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(key);
			clientMessage.Set(value);
			clientMessage.Set(threadId);
			clientMessage.Set(ttl);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapPutTransientCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapPutTransientCodec.RequestParameters parameters = new MapPutTransientCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData key;
			key = null;
			key = clientMessage.GetData();
			parameters.key = key;
			IData value;
			value = null;
			value = clientMessage.GetData();
			parameters.value = value;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
			long ttl;
			ttl = clientMessage.GetLong();
			parameters.ttl = ttl;
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
			int requiredDataSize = MapPutTransientCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapPutTransientCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapPutTransientCodec.ResponseParameters parameters = new MapPutTransientCodec.ResponseParameters();
			return parameters;
		}
	}
}
