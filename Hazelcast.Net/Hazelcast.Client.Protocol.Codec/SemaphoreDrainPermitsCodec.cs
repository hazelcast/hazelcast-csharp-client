using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class SemaphoreDrainPermitsCodec
	{
		public static readonly SemaphoreMessageType RequestType = SemaphoreMessageType.SemaphoreDrainpermits;

		public const int ResponseType = 102;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly SemaphoreMessageType Type = RequestType;

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
			int requiredDataSize = SemaphoreDrainPermitsCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreDrainPermitsCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			SemaphoreDrainPermitsCodec.RequestParameters parameters = new SemaphoreDrainPermitsCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
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
			int requiredDataSize = SemaphoreDrainPermitsCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SemaphoreDrainPermitsCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			SemaphoreDrainPermitsCodec.ResponseParameters parameters = new SemaphoreDrainPermitsCodec.ResponseParameters();
			int response;
			response = clientMessage.GetInt();
			parameters.response = response;
			return parameters;
		}
	}
}
