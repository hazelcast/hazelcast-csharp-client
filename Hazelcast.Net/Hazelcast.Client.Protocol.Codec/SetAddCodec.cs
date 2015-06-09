using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class SetAddCodec
	{
		public static readonly SetMessageType RequestType = SetMessageType.SetAdd;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly SetMessageType Type = RequestType;

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
			int requiredDataSize = SetAddCodec.RequestParameters.CalculateDataSize(name, value);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(value);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SetAddCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			SetAddCodec.RequestParameters parameters = new SetAddCodec.RequestParameters();
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
			int requiredDataSize = SetAddCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SetAddCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			SetAddCodec.ResponseParameters parameters = new SetAddCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
