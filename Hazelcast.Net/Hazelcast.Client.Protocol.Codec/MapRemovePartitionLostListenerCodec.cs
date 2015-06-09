using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapRemovePartitionLostListenerCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapRemovepartitionlostlistener;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public string registrationId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string registrationId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(registrationId);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string registrationId)
		{
			int requiredDataSize = MapRemovePartitionLostListenerCodec.RequestParameters.CalculateDataSize(name, registrationId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(registrationId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapRemovePartitionLostListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapRemovePartitionLostListenerCodec.RequestParameters parameters = new MapRemovePartitionLostListenerCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
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
			int requiredDataSize = MapRemovePartitionLostListenerCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapRemovePartitionLostListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapRemovePartitionLostListenerCodec.ResponseParameters parameters = new MapRemovePartitionLostListenerCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
