using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class QueueCompareAndRemoveAllCodec
	{
		public static readonly QueueMessageType RequestType = QueueMessageType.QueueCompareandremoveall;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly QueueMessageType Type = RequestType;

			public string name;

			public ICollection<IData> dataList;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, ICollection<IData> dataList)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				foreach (IData dataList_item in dataList)
				{
					dataSize += ParameterUtil.CalculateDataSize(dataList_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, ICollection<IData> dataList)
		{
			int requiredDataSize = QueueCompareAndRemoveAllCodec.RequestParameters.CalculateDataSize(name, dataList);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(dataList.Count);
			foreach (IData dataList_item in dataList)
			{
				clientMessage.Set(dataList_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static QueueCompareAndRemoveAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			QueueCompareAndRemoveAllCodec.RequestParameters parameters = new QueueCompareAndRemoveAllCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			ICollection<IData> dataList;
			dataList = null;
			int dataList_size = clientMessage.GetInt();
			dataList = new AList<IData>(dataList_size);
			for (int dataList_index = 0; dataList_index < dataList_size; dataList_index++)
			{
				IData dataList_item;
				dataList_item = clientMessage.GetData();
				dataList.AddItem(dataList_item);
			}
			parameters.dataList = dataList;
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
			int requiredDataSize = QueueCompareAndRemoveAllCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static QueueCompareAndRemoveAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			QueueCompareAndRemoveAllCodec.ResponseParameters parameters = new QueueCompareAndRemoveAllCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
