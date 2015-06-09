using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ReplicatedMapSizeCodec
	{
		public static readonly ReplicatedMapMessageType RequestType = ReplicatedMapMessageType.ReplicatedmapSize;

		public const int ResponseType = 102;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly ReplicatedMapMessageType Type = RequestType;

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
			int requiredDataSize = ReplicatedMapSizeCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ReplicatedMapSizeCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ReplicatedMapSizeCodec.RequestParameters parameters = new ReplicatedMapSizeCodec.RequestParameters();
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
			int requiredDataSize = ReplicatedMapSizeCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ReplicatedMapSizeCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ReplicatedMapSizeCodec.ResponseParameters parameters = new ReplicatedMapSizeCodec.ResponseParameters();
			int response;
			response = clientMessage.GetInt();
			parameters.response = response;
			return parameters;
		}
	}
}
