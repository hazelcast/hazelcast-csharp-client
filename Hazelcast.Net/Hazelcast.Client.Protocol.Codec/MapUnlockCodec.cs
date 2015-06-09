using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapUnlockCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapUnlock;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public IData key;

			public long threadId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData key, long threadId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(key);
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData key, long threadId)
		{
			int requiredDataSize = MapUnlockCodec.RequestParameters.CalculateDataSize(name, key, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(key);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapUnlockCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapUnlockCodec.RequestParameters parameters = new MapUnlockCodec.RequestParameters();
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
			int requiredDataSize = MapUnlockCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapUnlockCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapUnlockCodec.ResponseParameters parameters = new MapUnlockCodec.ResponseParameters();
			return parameters;
		}
	}
}
