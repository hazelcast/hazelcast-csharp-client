using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class QueueAddListenerCodec
	{
		public static readonly QueueMessageType RequestType = QueueMessageType.QueueAddlistener;

		public const int ResponseType = 104;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly QueueMessageType Type = RequestType;

			public string name;

			public bool includeValue;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, bool includeValue)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, bool includeValue)
		{
			int requiredDataSize = QueueAddListenerCodec.RequestParameters.CalculateDataSize(name, includeValue);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(includeValue);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static QueueAddListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			QueueAddListenerCodec.RequestParameters parameters = new QueueAddListenerCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			bool includeValue;
			includeValue = clientMessage.GetBoolean();
			parameters.includeValue = includeValue;
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
			int requiredDataSize = QueueAddListenerCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static QueueAddListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			QueueAddListenerCodec.ResponseParameters parameters = new QueueAddListenerCodec.ResponseParameters();
			string response;
			response = null;
			response = clientMessage.GetStringUtf8();
			parameters.response = response;
			return parameters;
		}

		//************************ EVENTS *************************//
		public static ClientMessage EncodeItemEvent(IData item, string uuid, int eventType)
		{
			int dataSize = ClientMessage.HeaderSize;
			dataSize += Bits.BooleanSizeInBytes;
			if (item != null)
			{
				dataSize += ParameterUtil.CalculateDataSize(item);
			}
			dataSize += ParameterUtil.CalculateStringDataSize(uuid);
			dataSize += Bits.IntSizeInBytes;
			ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
			clientMessage.SetMessageType(EventMessageConst.EventItem);
			clientMessage.AddFlag(ClientMessage.ListenerEventFlag);
			bool item_isNull;
			if (item == null)
			{
				item_isNull = true;
				clientMessage.Set(item_isNull);
			}
			else
			{
				item_isNull = false;
				clientMessage.Set(item_isNull);
				clientMessage.Set(item);
			}
			clientMessage.Set(uuid);
			clientMessage.Set(eventType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public abstract class AbstractEventHandler
		{
			public virtual void Handle(ClientMessage clientMessage)
			{
				int messageType = clientMessage.GetMessageType();
				if (messageType == EventMessageConst.EventItem)
				{
					IData item;
					item = null;
					bool item_isNull = clientMessage.GetBoolean();
					if (!item_isNull)
					{
						item = clientMessage.GetData();
					}
					string uuid;
					uuid = null;
					uuid = clientMessage.GetStringUtf8();
					int eventType;
					eventType = clientMessage.GetInt();
					Handle(item, uuid, eventType);
					return;
				}
				Logger.GetLogger(base.GetType()).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
			}

			public abstract void Handle(IData item, string uuid, int eventType);
		}
	}
}
