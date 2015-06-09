using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicLongCompareAndSetCodec
	{
		public static readonly AtomicLongMessageType RequestType = AtomicLongMessageType.AtomiclongCompareandset;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly AtomicLongMessageType Type = RequestType;

			public string name;

			public long expected;

			public long updated;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, long expected, long updated)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.LongSizeInBytes;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, long expected, long updated)
		{
			int requiredDataSize = AtomicLongCompareAndSetCodec.RequestParameters.CalculateDataSize(name, expected, updated);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(expected);
			clientMessage.Set(updated);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongCompareAndSetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicLongCompareAndSetCodec.RequestParameters parameters = new AtomicLongCompareAndSetCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			long expected;
			expected = clientMessage.GetLong();
			parameters.expected = expected;
			long updated;
			updated = clientMessage.GetLong();
			parameters.updated = updated;
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
			int requiredDataSize = AtomicLongCompareAndSetCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongCompareAndSetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicLongCompareAndSetCodec.ResponseParameters parameters = new AtomicLongCompareAndSetCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
