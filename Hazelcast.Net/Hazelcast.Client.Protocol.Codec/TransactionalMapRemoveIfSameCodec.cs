using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionalMapRemoveIfSameCodec
	{
		public static readonly TransactionalMapMessageType RequestType = TransactionalMapMessageType.TransactionalmapRemoveifsame;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionalMapMessageType Type = RequestType;

			public string name;

			public string txnId;

			public long threadId;

			public IData key;

			public IData value;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string txnId, long threadId, IData key, IData value)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(txnId);
				dataSize += Bits.LongSizeInBytes;
				dataSize += ParameterUtil.CalculateDataSize(key);
				dataSize += ParameterUtil.CalculateDataSize(value);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string txnId, long threadId, IData key, IData value)
		{
			int requiredDataSize = TransactionalMapRemoveIfSameCodec.RequestParameters.CalculateDataSize(name, txnId, threadId, key, value);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(txnId);
			clientMessage.Set(threadId);
			clientMessage.Set(key);
			clientMessage.Set(value);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalMapRemoveIfSameCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionalMapRemoveIfSameCodec.RequestParameters parameters = new TransactionalMapRemoveIfSameCodec.RequestParameters();
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
			IData value;
			value = null;
			value = clientMessage.GetData();
			parameters.value = value;
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
			int requiredDataSize = TransactionalMapRemoveIfSameCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalMapRemoveIfSameCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionalMapRemoveIfSameCodec.ResponseParameters parameters = new TransactionalMapRemoveIfSameCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
