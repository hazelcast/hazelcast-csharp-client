using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ReplicatedMapIsEmptyCodec
	{
		public static readonly ReplicatedMapMessageType RequestType = ReplicatedMapMessageType.ReplicatedmapIsempty;

		public const int ResponseType = 101;

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
			int requiredDataSize = ReplicatedMapIsEmptyCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ReplicatedMapIsEmptyCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ReplicatedMapIsEmptyCodec.RequestParameters parameters = new ReplicatedMapIsEmptyCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
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
			int requiredDataSize = ReplicatedMapIsEmptyCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ReplicatedMapIsEmptyCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ReplicatedMapIsEmptyCodec.ResponseParameters parameters = new ReplicatedMapIsEmptyCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
