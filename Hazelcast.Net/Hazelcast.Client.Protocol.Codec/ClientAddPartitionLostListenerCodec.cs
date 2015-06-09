using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientAddPartitionLostListenerCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientAddpartitionlostlistener;

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
			int requiredDataSize = ClientAddPartitionLostListenerCodec.RequestParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientAddPartitionLostListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientAddPartitionLostListenerCodec.RequestParameters parameters = new ClientAddPartitionLostListenerCodec.RequestParameters();
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
			int requiredDataSize = ClientAddPartitionLostListenerCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientAddPartitionLostListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientAddPartitionLostListenerCodec.ResponseParameters parameters = new ClientAddPartitionLostListenerCodec.ResponseParameters();
			string response;
			response = null;
			response = clientMessage.GetStringUtf8();
			parameters.response = response;
			return parameters;
		}

		//************************ EVENTS *************************//
		public static ClientMessage EncodePartitionLostEvent(int partitionId, int lostBackupCount, Address source)
		{
			int dataSize = ClientMessage.HeaderSize;
			dataSize += Bits.IntSizeInBytes;
			dataSize += Bits.IntSizeInBytes;
			dataSize += Bits.BooleanSizeInBytes;
			if (source != null)
			{
				dataSize += AddressCodec.CalculateDataSize(source);
			}
			ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
			clientMessage.SetMessageType(EventMessageConst.EventPartitionlost);
			clientMessage.AddFlag(ClientMessage.ListenerEventFlag);
			clientMessage.Set(partitionId);
			clientMessage.Set(lostBackupCount);
			bool source_isNull;
			if (source == null)
			{
				source_isNull = true;
				clientMessage.Set(source_isNull);
			}
			else
			{
				source_isNull = false;
				clientMessage.Set(source_isNull);
				AddressCodec.Encode(source, clientMessage);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public abstract class AbstractEventHandler
		{
			public virtual void Handle(ClientMessage clientMessage)
			{
				int messageType = clientMessage.GetMessageType();
				if (messageType == EventMessageConst.EventPartitionlost)
				{
					int partitionId;
					partitionId = clientMessage.GetInt();
					int lostBackupCount;
					lostBackupCount = clientMessage.GetInt();
					Address source;
					source = null;
					bool source_isNull = clientMessage.GetBoolean();
					if (!source_isNull)
					{
						source = AddressCodec.Decode(clientMessage);
					}
					Handle(partitionId, lostBackupCount, source);
					return;
				}
				Logger.GetLogger(base.GetType()).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
			}

			public abstract void Handle(int partitionId, int lostBackupCount, Address source);
		}
	}
}
