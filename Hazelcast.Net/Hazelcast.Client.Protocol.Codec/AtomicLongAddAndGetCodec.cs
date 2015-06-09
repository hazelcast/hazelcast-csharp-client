using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class AtomicLongAddAndGetCodec
	{
		public static readonly AtomicLongMessageType RequestType = AtomicLongMessageType.AtomiclongAddandget;

		public const int ResponseType = 103;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly AtomicLongMessageType Type = RequestType;

			public string name;

			public long delta;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, long delta)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, long delta)
		{
			int requiredDataSize = AtomicLongAddAndGetCodec.RequestParameters.CalculateDataSize(name, delta);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(delta);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongAddAndGetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			AtomicLongAddAndGetCodec.RequestParameters parameters = new AtomicLongAddAndGetCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			long delta;
			delta = clientMessage.GetLong();
			parameters.delta = delta;
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
			int requiredDataSize = AtomicLongAddAndGetCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static AtomicLongAddAndGetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			AtomicLongAddAndGetCodec.ResponseParameters parameters = new AtomicLongAddAndGetCodec.ResponseParameters();
			long response;
			response = clientMessage.GetLong();
			parameters.response = response;
			return parameters;
		}
	}
}
