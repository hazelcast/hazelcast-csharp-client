using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TopicRemoveMessageListenerCodec
	{
		public static readonly TopicMessageType RequestType = TopicMessageType.TopicRemovemessagelistener;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TopicMessageType Type = RequestType;

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
			int requiredDataSize = TopicRemoveMessageListenerCodec.RequestParameters.CalculateDataSize(name, registrationId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(registrationId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TopicRemoveMessageListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TopicRemoveMessageListenerCodec.RequestParameters parameters = new TopicRemoveMessageListenerCodec.RequestParameters();
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
			int requiredDataSize = TopicRemoveMessageListenerCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TopicRemoveMessageListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TopicRemoveMessageListenerCodec.ResponseParameters parameters = new TopicRemoveMessageListenerCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
