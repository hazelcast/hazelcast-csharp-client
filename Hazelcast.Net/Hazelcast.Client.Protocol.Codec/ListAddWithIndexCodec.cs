using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ListAddWithIndexCodec
	{
		public static readonly ListMessageType RequestType = ListMessageType.ListAddwithindex;

		public const int ResponseType = 100;

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
			int requiredDataSize = ListAddWithIndexCodec.RequestParameters.CalculateDataSize(name, index, value);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(index);
			clientMessage.Set(value);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListAddWithIndexCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ListAddWithIndexCodec.RequestParameters parameters = new ListAddWithIndexCodec.RequestParameters();
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
			//************************ RESPONSE *************************//
			public static int CalculateDataSize()
			{
				int dataSize = ClientMessage.HeaderSize;
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse()
		{
			int requiredDataSize = ListAddWithIndexCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListAddWithIndexCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ListAddWithIndexCodec.ResponseParameters parameters = new ListAddWithIndexCodec.ResponseParameters();
			return parameters;
		}
	}
}
