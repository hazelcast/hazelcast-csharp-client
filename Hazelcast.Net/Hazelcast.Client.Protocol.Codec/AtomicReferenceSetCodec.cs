using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicReferenceSetCodec
	{
		public static readonly AtomicReferenceMessageType RequestType = AtomicReferenceMessageType.AtomicreferenceSet;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly AtomicReferenceMessageType Type = RequestType;

			public string name;

			public IData newValue;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData newValue)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.BooleanSizeInBytes;
				if (newValue != null)
				{
					dataSize += ParameterUtil.CalculateDataSize(newValue);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData newValue)
		{
			int requiredDataSize = AtomicReferenceSetCodec.RequestParameters.CalculateDataSize(name, newValue);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			bool newValue_isNull;
			if (newValue == null)
			{
				newValue_isNull = true;
				clientMessage.Set(newValue_isNull);
			}
			else
			{
				newValue_isNull = false;
				clientMessage.Set(newValue_isNull);
				clientMessage.Set(newValue);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicReferenceSetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicReferenceSetCodec.RequestParameters parameters = new AtomicReferenceSetCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData newValue;
			newValue = null;
			bool newValue_isNull = clientMessage.GetBoolean();
			if (!newValue_isNull)
			{
				newValue = clientMessage.GetData();
				parameters.newValue = newValue;
			}
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
			int requiredDataSize = AtomicReferenceSetCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicReferenceSetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicReferenceSetCodec.ResponseParameters parameters = new AtomicReferenceSetCodec.ResponseParameters();
			return parameters;
		}
	}
}
