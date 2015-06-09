using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ReplicatedMapClearCodec
	{
		public static readonly ReplicatedMapMessageType RequestType = ReplicatedMapMessageType.ReplicatedmapClear;

		public const int ResponseType = 100;

		public const bool Retryable = false;

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
			int requiredDataSize = ReplicatedMapClearCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ReplicatedMapClearCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ReplicatedMapClearCodec.RequestParameters parameters = new ReplicatedMapClearCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
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
			int requiredDataSize = ReplicatedMapClearCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ReplicatedMapClearCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ReplicatedMapClearCodec.ResponseParameters parameters = new ReplicatedMapClearCodec.ResponseParameters();
			return parameters;
		}
	}
}
