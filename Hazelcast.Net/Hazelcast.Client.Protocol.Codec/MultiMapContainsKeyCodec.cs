using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MultiMapContainsKeyCodec
	{
		public static readonly MultiMapMessageType RequestType = MultiMapMessageType.MultimapContainskey;

		public const int ResponseType = 101;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly MultiMapMessageType Type = RequestType;

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
			int requiredDataSize = MultiMapContainsKeyCodec.RequestParameters.CalculateDataSize(name, key, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(key);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapContainsKeyCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MultiMapContainsKeyCodec.RequestParameters parameters = new MultiMapContainsKeyCodec.RequestParameters();
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
			int requiredDataSize = MultiMapContainsKeyCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapContainsKeyCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MultiMapContainsKeyCodec.ResponseParameters parameters = new MultiMapContainsKeyCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
