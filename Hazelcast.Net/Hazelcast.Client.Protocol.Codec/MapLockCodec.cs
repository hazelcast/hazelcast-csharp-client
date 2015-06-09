using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapLockCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapLock;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public IData key;

			public long threadId;

			public long ttl;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData key, long threadId, long ttl)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(key);
				dataSize += Bits.LongSizeInBytes;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData key, long threadId, long ttl)
		{
			int requiredDataSize = MapLockCodec.RequestParameters.CalculateDataSize(name, key, threadId, ttl);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(key);
			clientMessage.Set(threadId);
			clientMessage.Set(ttl);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapLockCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapLockCodec.RequestParameters parameters = new MapLockCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData key;
			key = null;
			key = clientMessage.GetData();
			parameters.key = key;
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
			int requiredDataSize = MapLockCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapLockCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapLockCodec.ResponseParameters parameters = new MapLockCodec.ResponseParameters();
			return parameters;
		}
	}
}
