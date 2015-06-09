using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicLongAlterAndGetCodec
	{
		public static readonly AtomicLongMessageType RequestType = AtomicLongMessageType.AtomiclongAlterandget;

		public const int ResponseType = 103;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly AtomicLongMessageType Type = RequestType;

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
			int requiredDataSize = AtomicLongAlterAndGetCodec.RequestParameters.CalculateDataSize(name, function);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(function);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongAlterAndGetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicLongAlterAndGetCodec.RequestParameters parameters = new AtomicLongAlterAndGetCodec.RequestParameters();
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
			public long response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(long response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(long response)
		{
			int requiredDataSize = AtomicLongAlterAndGetCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongAlterAndGetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicLongAlterAndGetCodec.ResponseParameters parameters = new AtomicLongAlterAndGetCodec.ResponseParameters();
			long response;
			response = clientMessage.GetLong();
			parameters.response = response;
			return parameters;
		}
	}
}
