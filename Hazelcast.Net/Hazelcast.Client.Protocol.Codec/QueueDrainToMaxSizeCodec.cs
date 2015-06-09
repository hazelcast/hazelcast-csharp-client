using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class QueueDrainToMaxSizeCodec
	{
		public static readonly QueueMessageType RequestType = QueueMessageType.QueueDraintomaxsize;

		public const int ResponseType = 106;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly QueueMessageType Type = RequestType;

			public string name;

			public int maxSize;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, int maxSize)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, int maxSize)
		{
			int requiredDataSize = QueueDrainToMaxSizeCodec.RequestParameters.CalculateDataSize(name, maxSize);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(maxSize);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static QueueDrainToMaxSizeCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			QueueDrainToMaxSizeCodec.RequestParameters parameters = new QueueDrainToMaxSizeCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			int maxSize;
			maxSize = clientMessage.GetInt();
			parameters.maxSize = maxSize;
			return parameters;
		}

		public class ResponseParameters
		{
			public ICollection<IData> list;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(ICollection<IData> list)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.IntSizeInBytes;
				foreach (IData list_item in list)
				{
					dataSize += ParameterUtil.CalculateDataSize(list_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(ICollection<IData> list)
		{
			int requiredDataSize = QueueDrainToMaxSizeCodec.ResponseParameters.CalculateDataSize(list);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(list.Count);
			foreach (IData list_item in list)
			{
				clientMessage.Set(list_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static QueueDrainToMaxSizeCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			QueueDrainToMaxSizeCodec.ResponseParameters parameters = new QueueDrainToMaxSizeCodec.ResponseParameters();
			IList<IData> list;
			list = null;
			int list_size = clientMessage.GetInt();
			list = new AList<IData>(list_size);
			for (int list_index = 0; list_index < list_size; list_index++)
			{
				IData list_item;
				list_item = clientMessage.GetData();
				list.AddItem(list_item);
			}
			parameters.list = list;
			return parameters;
		}
	}
}
