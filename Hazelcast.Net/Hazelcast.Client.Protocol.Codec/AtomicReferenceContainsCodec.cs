using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicReferenceContainsCodec
	{
		public static readonly AtomicReferenceMessageType RequestType = AtomicReferenceMessageType.AtomicreferenceContains;

		public const int ResponseType = 101;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly AtomicReferenceMessageType Type = RequestType;

			public string name;

			public IData expected;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData expected)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.BooleanSizeInBytes;
				if (expected != null)
				{
					dataSize += ParameterUtil.CalculateDataSize(expected);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData expected)
		{
			int requiredDataSize = AtomicReferenceContainsCodec.RequestParameters.CalculateDataSize(name, expected);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			bool expected_isNull;
			if (expected == null)
			{
				expected_isNull = true;
				clientMessage.Set(expected_isNull);
			}
			else
			{
				expected_isNull = false;
				clientMessage.Set(expected_isNull);
				clientMessage.Set(expected);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicReferenceContainsCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicReferenceContainsCodec.RequestParameters parameters = new AtomicReferenceContainsCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData expected;
			expected = null;
			bool expected_isNull = clientMessage.GetBoolean();
			if (!expected_isNull)
			{
				expected = clientMessage.GetData();
				parameters.expected = expected;
			}
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
			int requiredDataSize = AtomicReferenceContainsCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicReferenceContainsCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicReferenceContainsCodec.ResponseParameters parameters = new AtomicReferenceContainsCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
