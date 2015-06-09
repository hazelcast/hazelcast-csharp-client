using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicLongIncrementAndGetCodec
	{
		public static readonly AtomicLongMessageType RequestType = AtomicLongMessageType.AtomiclongIncrementandget;

		public const int ResponseType = 103;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly AtomicLongMessageType Type = RequestType;

			public string name;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name)
		{
			int requiredDataSize = AtomicLongIncrementAndGetCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongIncrementAndGetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicLongIncrementAndGetCodec.RequestParameters parameters = new AtomicLongIncrementAndGetCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
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
			int requiredDataSize = AtomicLongIncrementAndGetCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongIncrementAndGetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicLongIncrementAndGetCodec.ResponseParameters parameters = new AtomicLongIncrementAndGetCodec.ResponseParameters();
			long response;
			response = clientMessage.GetLong();
			parameters.response = response;
			return parameters;
		}
	}
}
