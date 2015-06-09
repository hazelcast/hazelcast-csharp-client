using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TopicPublishCodec
	{
		public static readonly TopicMessageType RequestType = TopicMessageType.TopicPublish;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TopicMessageType Type = RequestType;

			public string name;

			public IData message;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData message)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(message);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData message)
		{
			int requiredDataSize = TopicPublishCodec.RequestParameters.CalculateDataSize(name, message);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(message);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TopicPublishCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TopicPublishCodec.RequestParameters parameters = new TopicPublishCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData message;
			message = null;
			message = clientMessage.GetData();
			parameters.message = message;
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
			int requiredDataSize = TopicPublishCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TopicPublishCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TopicPublishCodec.ResponseParameters parameters = new TopicPublishCodec.ResponseParameters();
			return parameters;
		}
	}
}
