using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class SemaphoreTryAcquireCodec
	{
		public static readonly SemaphoreMessageType RequestType = SemaphoreMessageType.SemaphoreTryacquire;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly SemaphoreMessageType Type = RequestType;

			public string name;

			public int permits;

			public long timeout;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, int permits, long timeout)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, int permits, long timeout)
		{
			int requiredDataSize = SemaphoreTryAcquireCodec.RequestParameters.CalculateDataSize(name, permits, timeout);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(permits);
			clientMessage.Set(timeout);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreTryAcquireCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			SemaphoreTryAcquireCodec.RequestParameters parameters = new SemaphoreTryAcquireCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			int permits;
			permits = clientMessage.GetInt();
			parameters.permits = permits;
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
			int requiredDataSize = SemaphoreTryAcquireCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreTryAcquireCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			SemaphoreTryAcquireCodec.ResponseParameters parameters = new SemaphoreTryAcquireCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
