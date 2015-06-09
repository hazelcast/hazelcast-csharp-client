using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicReferenceCompareAndSetCodec
	{
		public static readonly AtomicReferenceMessageType RequestType = AtomicReferenceMessageType.AtomicreferenceCompareandset;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly AtomicReferenceMessageType Type = RequestType;

			public string name;

			public IData expected;

			public IData updated;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData expected, IData updated)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.BooleanSizeInBytes;
				if (expected != null)
				{
					dataSize += ParameterUtil.CalculateDataSize(expected);
				}
				dataSize += Bits.BooleanSizeInBytes;
				if (updated != null)
				{
					dataSize += ParameterUtil.CalculateDataSize(updated);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData expected, IData updated)
		{
			int requiredDataSize = AtomicReferenceCompareAndSetCodec.RequestParameters.CalculateDataSize(name, expected, updated);
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
			bool updated_isNull;
			if (updated == null)
			{
				updated_isNull = true;
				clientMessage.Set(updated_isNull);
			}
			else
			{
				updated_isNull = false;
				clientMessage.Set(updated_isNull);
				clientMessage.Set(updated);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicReferenceCompareAndSetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicReferenceCompareAndSetCodec.RequestParameters parameters = new AtomicReferenceCompareAndSetCodec.RequestParameters();
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
			IData updated;
			updated = null;
			bool updated_isNull = clientMessage.GetBoolean();
			if (!updated_isNull)
			{
				updated = clientMessage.GetData();
				parameters.updated = updated;
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
			int requiredDataSize = AtomicReferenceCompareAndSetCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicReferenceCompareAndSetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicReferenceCompareAndSetCodec.ResponseParameters parameters = new AtomicReferenceCompareAndSetCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
