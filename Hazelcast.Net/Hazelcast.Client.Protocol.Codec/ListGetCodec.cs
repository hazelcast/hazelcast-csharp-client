using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ListGetCodec
	{
		public static readonly ListMessageType RequestType = ListMessageType.ListGet;

		public const int ResponseType = 105;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly ListMessageType Type = RequestType;

			public string name;

			public int index;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, int index)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, int index)
		{
			int requiredDataSize = ListGetCodec.RequestParameters.CalculateDataSize(name, index);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(index);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListGetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ListGetCodec.RequestParameters parameters = new ListGetCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			int index;
			index = clientMessage.GetInt();
			parameters.index = index;
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
			int requiredDataSize = ListGetCodec.ResponseParameters.CalculateDataSize(response);
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

		public static ListGetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ListGetCodec.ResponseParameters parameters = new ListGetCodec.ResponseParameters();
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
