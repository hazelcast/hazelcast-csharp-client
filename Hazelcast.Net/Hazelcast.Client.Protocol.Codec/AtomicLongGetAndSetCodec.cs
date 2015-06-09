using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicLongGetAndSetCodec
	{
		public static readonly AtomicLongMessageType RequestType = AtomicLongMessageType.AtomiclongGetandset;

		public const int ResponseType = 103;

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
			int requiredDataSize = AtomicLongGetAndSetCodec.RequestParameters.CalculateDataSize(name, newValue);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(newValue);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongGetAndSetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicLongGetAndSetCodec.RequestParameters parameters = new AtomicLongGetAndSetCodec.RequestParameters();
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
			public long response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(long response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(long response)
		{
			int requiredDataSize = AtomicLongGetAndSetCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongGetAndSetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicLongGetAndSetCodec.ResponseParameters parameters = new AtomicLongGetAndSetCodec.ResponseParameters();
			long response;
			response = clientMessage.GetLong();
			parameters.response = response;
			return parameters;
		}
	}
}
