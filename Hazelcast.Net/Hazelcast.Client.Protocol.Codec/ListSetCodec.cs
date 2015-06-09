using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ListSetCodec
	{
		public static readonly ListMessageType RequestType = ListMessageType.ListSet;

		public const int ResponseType = 105;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ListMessageType Type = RequestType;

			public string name;

			public int index;

			public IData value;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, int index, IData value)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				dataSize += ParameterUtil.CalculateDataSize(value);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, int index, IData value)
		{
			int requiredDataSize = ListSetCodec.RequestParameters.CalculateDataSize(name, index, value);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(index);
			clientMessage.Set(value);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListSetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ListSetCodec.RequestParameters parameters = new ListSetCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			int index;
			index = clientMessage.GetInt();
			parameters.index = index;
			IData value;
			value = null;
			value = clientMessage.GetData();
			parameters.value = value;
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
			int requiredDataSize = ListSetCodec.ResponseParameters.CalculateDataSize(response);
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

		public static ListSetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ListSetCodec.ResponseParameters parameters = new ListSetCodec.ResponseParameters();
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
