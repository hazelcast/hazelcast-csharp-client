using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class CountDownLatchAwaitCodec
	{
		public static readonly CountDownLatchMessageType RequestType = CountDownLatchMessageType.CountdownlatchAwait;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly CountDownLatchMessageType Type = RequestType;

			public string name;

			public long timeout;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, long timeout)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, long timeout)
		{
			int requiredDataSize = CountDownLatchAwaitCodec.RequestParameters.CalculateDataSize(name, timeout);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(timeout);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static CountDownLatchAwaitCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			CountDownLatchAwaitCodec.RequestParameters parameters = new CountDownLatchAwaitCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			long timeout;
			timeout = clientMessage.GetLong();
			parameters.timeout = timeout;
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
			int requiredDataSize = CountDownLatchAwaitCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static CountDownLatchAwaitCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			CountDownLatchAwaitCodec.ResponseParameters parameters = new CountDownLatchAwaitCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
