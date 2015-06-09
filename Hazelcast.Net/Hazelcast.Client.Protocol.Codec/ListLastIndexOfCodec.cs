using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ListLastIndexOfCodec
	{
		public static readonly ListMessageType RequestType = ListMessageType.ListLastindexof;

		public const int ResponseType = 102;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly ListMessageType Type = RequestType;

			public string name;

			public IData value;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData value)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(value);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData value)
		{
			int requiredDataSize = ListLastIndexOfCodec.RequestParameters.CalculateDataSize(name, value);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(value);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListLastIndexOfCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ListLastIndexOfCodec.RequestParameters parameters = new ListLastIndexOfCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData value;
			value = null;
			value = clientMessage.GetData();
			parameters.value = value;
			return parameters;
		}

		public class ResponseParameters
		{
			public int response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(int response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.IntSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(int response)
		{
			int requiredDataSize = ListLastIndexOfCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListLastIndexOfCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ListLastIndexOfCodec.ResponseParameters parameters = new ListLastIndexOfCodec.ResponseParameters();
			int response;
			response = clientMessage.GetInt();
			parameters.response = response;
			return parameters;
		}
	}
}
