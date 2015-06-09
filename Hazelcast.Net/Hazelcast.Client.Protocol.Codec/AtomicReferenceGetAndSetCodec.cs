using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicReferenceGetAndSetCodec
	{
		public static readonly AtomicReferenceMessageType RequestType = AtomicReferenceMessageType.AtomicreferenceGetandset;

		public const int ResponseType = 105;

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
			int requiredDataSize = AtomicReferenceGetAndSetCodec.RequestParameters.CalculateDataSize(name, newValue);
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

		public static AtomicReferenceGetAndSetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicReferenceGetAndSetCodec.RequestParameters parameters = new AtomicReferenceGetAndSetCodec.RequestParameters();
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
			public IData response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(IData response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.BooleanSizeInBytes;
				if (response != null)
				{
					dataSize += ParameterUtil.CalculateDataSize(response);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(IData response)
		{
			int requiredDataSize = AtomicReferenceGetAndSetCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			bool response_isNull;
			if (response == null)
			{
				response_isNull = true;
				clientMessage.Set(response_isNull);
			}
			else
			{
				response_isNull = false;
				clientMessage.Set(response_isNull);
				clientMessage.Set(response);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicReferenceGetAndSetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicReferenceGetAndSetCodec.ResponseParameters parameters = new AtomicReferenceGetAndSetCodec.ResponseParameters();
			IData response;
			response = null;
			bool response_isNull = clientMessage.GetBoolean();
			if (!response_isNull)
			{
				response = clientMessage.GetData();
				parameters.response = response;
			}
			return parameters;
		}
	}
}
