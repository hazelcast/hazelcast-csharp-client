using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientGetDistributedObjectCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientGetdistributedobject;

		public const int ResponseType = 112;

		public const bool Retryable = false;

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
			int requiredDataSize = ClientGetDistributedObjectCodec.RequestParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientGetDistributedObjectCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientGetDistributedObjectCodec.RequestParameters parameters = new ClientGetDistributedObjectCodec.RequestParameters();
			return parameters;
		}

		public class ResponseParameters
		{
			public ICollection<DistributedObjectInfo> infoCollection;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(ICollection<DistributedObjectInfo> infoCollection)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.IntSizeInBytes;
				foreach (DistributedObjectInfo infoCollection_item in infoCollection)
				{
					dataSize += DistributedObjectInfoCodec.CalculateDataSize(infoCollection_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(ICollection<DistributedObjectInfo> infoCollection)
		{
			int requiredDataSize = ClientGetDistributedObjectCodec.ResponseParameters.CalculateDataSize(infoCollection);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(infoCollection.Count);
			foreach (DistributedObjectInfo infoCollection_item in infoCollection)
			{
				DistributedObjectInfoCodec.Encode(infoCollection_item, clientMessage);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientGetDistributedObjectCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientGetDistributedObjectCodec.ResponseParameters parameters = new ClientGetDistributedObjectCodec.ResponseParameters();
			ICollection<DistributedObjectInfo> infoCollection;
			infoCollection = null;
			int infoCollection_size = clientMessage.GetInt();
			infoCollection = new List<DistributedObjectInfo>(infoCollection_size);
			for (int infoCollection_index = 0; infoCollection_index < infoCollection_size; infoCollection_index++)
			{
				DistributedObjectInfo infoCollection_item;
				infoCollection_item = DistributedObjectInfoCodec.Decode(clientMessage);
				infoCollection.Add(infoCollection_item);
			}
			parameters.infoCollection = infoCollection;
			return parameters;
		}
	}
}
