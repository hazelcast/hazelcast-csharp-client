using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicReferenceAlterCodec
	{
		public static readonly AtomicReferenceMessageType RequestType = AtomicReferenceMessageType.AtomicreferenceAlter;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly AtomicReferenceMessageType Type = RequestType;

			public string name;

			public IData function;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData function)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(function);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData function)
		{
			int requiredDataSize = AtomicReferenceAlterCodec.RequestParameters.CalculateDataSize(name, function);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(function);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicReferenceAlterCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicReferenceAlterCodec.RequestParameters parameters = new AtomicReferenceAlterCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData function;
			function = null;
			function = clientMessage.GetData();
			parameters.function = function;
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
			int requiredDataSize = AtomicReferenceAlterCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicReferenceAlterCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicReferenceAlterCodec.ResponseParameters parameters = new AtomicReferenceAlterCodec.ResponseParameters();
			return parameters;
		}
	}
}
