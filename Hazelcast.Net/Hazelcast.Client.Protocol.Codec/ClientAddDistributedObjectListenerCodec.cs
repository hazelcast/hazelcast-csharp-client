using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientAddDistributedObjectListenerCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientAdddistributedobjectlistener;

		public const int ResponseType = 104;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly ClientMessageType Type = RequestType;

			//************************ REQUEST *************************//
			public static int CalculateDataSize()
			{
				int dataSize = ClientMessage.HeaderSize;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest()
		{
			int requiredDataSize = ClientAddDistributedObjectListenerCodec.RequestParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientAddDistributedObjectListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientAddDistributedObjectListenerCodec.RequestParameters parameters = new ClientAddDistributedObjectListenerCodec.RequestParameters();
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
			int requiredDataSize = ClientAddDistributedObjectListenerCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientAddDistributedObjectListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientAddDistributedObjectListenerCodec.ResponseParameters parameters = new ClientAddDistributedObjectListenerCodec.ResponseParameters();
			string response;
			response = null;
			response = clientMessage.GetStringUtf8();
			parameters.response = response;
			return parameters;
		}

		//************************ EVENTS *************************//
		public static ClientMessage EncodeDistributedObjectEvent(string name, string serviceName, string eventType)
		{
			int dataSize = ClientMessage.HeaderSize;
			dataSize += ParameterUtil.CalculateStringDataSize(name);
			dataSize += ParameterUtil.CalculateStringDataSize(serviceName);
			dataSize += ParameterUtil.CalculateStringDataSize(eventType);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
			clientMessage.SetMessageType(EventMessageConst.EventDistributedobject);
			clientMessage.AddFlag(ClientMessage.ListenerEventFlag);
			clientMessage.Set(name);
			clientMessage.Set(serviceName);
			clientMessage.Set(eventType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public abstract class AbstractEventHandler
		{
			public virtual void Handle(ClientMessage clientMessage)
			{
				int messageType = clientMessage.GetMessageType();
				if (messageType == EventMessageConst.EventDistributedobject)
				{
					string name;
					name = null;
					name = clientMessage.GetStringUtf8();
					string serviceName;
					serviceName = null;
					serviceName = clientMessage.GetStringUtf8();
					string eventType;
					eventType = null;
					eventType = clientMessage.GetStringUtf8();
					Handle(name, serviceName, eventType);
					return;
				}
				Logger.GetLogger(base.GetType()).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
			}

			public abstract void Handle(string name, string serviceName, string eventType);
		}
	}
}
