using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionalMultiMapRemoveCodec
	{
		public static readonly TransactionalMultiMapMessageType RequestType = TransactionalMultiMapMessageType.TransactionalmultimapRemove;

		public const int ResponseType = 106;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionalMultiMapMessageType Type = RequestType;

			public string name;

			public string txnId;

			public long threadId;

			public IData key;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string txnId, long threadId, IData key)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(txnId);
				dataSize += Bits.LongSizeInBytes;
				dataSize += ParameterUtil.CalculateDataSize(key);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string txnId, long threadId, IData key)
		{
			int requiredDataSize = TransactionalMultiMapRemoveCodec.RequestParameters.CalculateDataSize(name, txnId, threadId, key);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(txnId);
			clientMessage.Set(threadId);
			clientMessage.Set(key);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalMultiMapRemoveCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionalMultiMapRemoveCodec.RequestParameters parameters = new TransactionalMultiMapRemoveCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			string txnId;
			txnId = null;
			txnId = clientMessage.GetStringUtf8();
			parameters.txnId = txnId;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
			IData key;
			key = null;
			key = clientMessage.GetData();
			parameters.key = key;
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
			int requiredDataSize = TransactionalMultiMapRemoveCodec.ResponseParameters.CalculateDataSize(list);
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

		public static TransactionalMultiMapRemoveCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionalMultiMapRemoveCodec.ResponseParameters parameters = new TransactionalMultiMapRemoveCodec.ResponseParameters();
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
