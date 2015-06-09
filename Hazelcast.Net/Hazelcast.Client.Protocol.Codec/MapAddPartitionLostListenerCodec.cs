using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapAddPartitionLostListenerCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapAddpartitionlostlistener;

		public const int ResponseType = 104;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

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
			int requiredDataSize = MapAddPartitionLostListenerCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapAddPartitionLostListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapAddPartitionLostListenerCodec.RequestParameters parameters = new MapAddPartitionLostListenerCodec.RequestParameters();
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
			int requiredDataSize = MapAddPartitionLostListenerCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapAddPartitionLostListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapAddPartitionLostListenerCodec.ResponseParameters parameters = new MapAddPartitionLostListenerCodec.ResponseParameters();
			string response;
			response = null;
			response = clientMessage.GetStringUtf8();
			parameters.response = response;
			return parameters;
		}

		//************************ EVENTS *************************//
		public static ClientMessage EncodeMapPartitionLostEvent(int partitionId, string uuid)
		{
			int dataSize = ClientMessage.HeaderSize;
			dataSize += Bits.IntSizeInBytes;
			dataSize += ParameterUtil.CalculateStringDataSize(uuid);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
			clientMessage.SetMessageType(EventMessageConst.EventMappartitionlost);
			clientMessage.AddFlag(ClientMessage.ListenerEventFlag);
			clientMessage.Set(partitionId);
			clientMessage.Set(uuid);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public abstract class AbstractEventHandler
		{
			public virtual void Handle(ClientMessage clientMessage)
			{
				int messageType = clientMessage.GetMessageType();
				if (messageType == EventMessageConst.EventMappartitionlost)
				{
					int partitionId;
					partitionId = clientMessage.GetInt();
					string uuid;
					uuid = null;
					uuid = clientMessage.GetStringUtf8();
					Handle(partitionId, uuid);
					return;
				}
				Logger.GetLogger(base.GetType()).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
			}

			public abstract void Handle(int partitionId, string uuid);
		}
	}
}
