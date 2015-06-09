using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapReplaceIfSameCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapReplaceifsame;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public IData key;

			public IData testValue;

			public IData value;

			public long threadId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData key, IData testValue, IData value, long threadId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(key);
				dataSize += ParameterUtil.CalculateDataSize(testValue);
				dataSize += ParameterUtil.CalculateDataSize(value);
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData key, IData testValue, IData value, long threadId)
		{
			int requiredDataSize = MapReplaceIfSameCodec.RequestParameters.CalculateDataSize(name, key, testValue, value, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(key);
			clientMessage.Set(testValue);
			clientMessage.Set(value);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapReplaceIfSameCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapReplaceIfSameCodec.RequestParameters parameters = new MapReplaceIfSameCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData key;
			key = null;
			key = clientMessage.GetData();
			parameters.key = key;
			IData testValue;
			testValue = null;
			testValue = clientMessage.GetData();
			parameters.testValue = testValue;
			IData value;
			value = null;
			value = clientMessage.GetData();
			parameters.value = value;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
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
			int requiredDataSize = MapReplaceIfSameCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapReplaceIfSameCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapReplaceIfSameCodec.ResponseParameters parameters = new MapReplaceIfSameCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
