using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientRemovePartitionLostListenerCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientRemovepartitionlostlistener;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ClientMessageType Type = RequestType;

			public string registrationId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string registrationId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(registrationId);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string registrationId)
		{
			int requiredDataSize = ClientRemovePartitionLostListenerCodec.RequestParameters.CalculateDataSize(registrationId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(registrationId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientRemovePartitionLostListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientRemovePartitionLostListenerCodec.RequestParameters parameters = new ClientRemovePartitionLostListenerCodec.RequestParameters();
			string registrationId;
			registrationId = null;
			registrationId = clientMessage.GetStringUtf8();
			parameters.registrationId = registrationId;
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
			int requiredDataSize = ClientRemovePartitionLostListenerCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientRemovePartitionLostListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientRemovePartitionLostListenerCodec.ResponseParameters parameters = new ClientRemovePartitionLostListenerCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
