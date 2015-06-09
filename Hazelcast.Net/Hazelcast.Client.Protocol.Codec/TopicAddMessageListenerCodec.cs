using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TopicAddMessageListenerCodec
	{
		public static readonly TopicMessageType RequestType = TopicMessageType.TopicAddmessagelistener;

		public const int ResponseType = 104;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly TopicMessageType Type = RequestType;

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
			int requiredDataSize = TopicAddMessageListenerCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TopicAddMessageListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TopicAddMessageListenerCodec.RequestParameters parameters = new TopicAddMessageListenerCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			return parameters;
		}

		public class ResponseParameters
		{
			public string response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(string response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(response);
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(string response)
		{
			int requiredDataSize = TopicAddMessageListenerCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TopicAddMessageListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TopicAddMessageListenerCodec.ResponseParameters parameters = new TopicAddMessageListenerCodec.ResponseParameters();
			string response;
			response = null;
			response = clientMessage.GetStringUtf8();
			parameters.response = response;
			return parameters;
		}

		//************************ EVENTS *************************//
		public static ClientMessage EncodeTopicEvent(IData item, long publishTime, string uuid)
		{
			int dataSize = ClientMessage.HeaderSize;
			dataSize += ParameterUtil.CalculateDataSize(item);
			dataSize += Bits.LongSizeInBytes;
			dataSize += ParameterUtil.CalculateStringDataSize(uuid);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
			clientMessage.SetMessageType(EventMessageConst.EventTopic);
			clientMessage.AddFlag(ClientMessage.ListenerEventFlag);
			clientMessage.Set(item);
			clientMessage.Set(publishTime);
			clientMessage.Set(uuid);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public abstract class AbstractEventHandler
		{
			public virtual void Handle(ClientMessage clientMessage)
			{
				int messageType = clientMessage.GetMessageType();
				if (messageType == EventMessageConst.EventTopic)
				{
					IData item;
					item = null;
					item = clientMessage.GetData();
					long publishTime;
					publishTime = clientMessage.GetLong();
					string uuid;
					uuid = null;
					uuid = clientMessage.GetStringUtf8();
					Handle(item, publishTime, uuid);
					return;
				}
				Logger.GetLogger(base.GetType()).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
			}

			public abstract void Handle(IData item, long publishTime, string uuid);
		}
	}
}
