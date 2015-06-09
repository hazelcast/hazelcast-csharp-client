using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicLongSetCodec
	{
		public static readonly AtomicLongMessageType RequestType = AtomicLongMessageType.AtomiclongSet;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly AtomicLongMessageType Type = RequestType;

			public string name;

			public long newValue;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, long newValue)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, long newValue)
		{
			int requiredDataSize = AtomicLongSetCodec.RequestParameters.CalculateDataSize(name, newValue);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(newValue);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongSetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicLongSetCodec.RequestParameters parameters = new AtomicLongSetCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			long newValue;
			newValue = clientMessage.GetLong();
			parameters.newValue = newValue;
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
			int requiredDataSize = AtomicLongSetCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongSetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicLongSetCodec.ResponseParameters parameters = new AtomicLongSetCodec.ResponseParameters();
			return parameters;
		}
	}
}
