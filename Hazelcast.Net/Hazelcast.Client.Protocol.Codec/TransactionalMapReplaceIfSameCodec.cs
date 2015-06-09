using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionalMapReplaceIfSameCodec
	{
		public static readonly TransactionalMapMessageType RequestType = TransactionalMapMessageType.TransactionalmapReplaceifsame;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionalMapMessageType Type = RequestType;

			public string name;

			public string txnId;

			public long threadId;

			public IData key;

			public IData oldValue;

			public IData newValue;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string txnId, long threadId, IData key, IData oldValue, IData newValue)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(txnId);
				dataSize += Bits.LongSizeInBytes;
				dataSize += ParameterUtil.CalculateDataSize(key);
				dataSize += ParameterUtil.CalculateDataSize(oldValue);
				dataSize += ParameterUtil.CalculateDataSize(newValue);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string txnId, long threadId, IData key, IData oldValue, IData newValue)
		{
			int requiredDataSize = TransactionalMapReplaceIfSameCodec.RequestParameters.CalculateDataSize(name, txnId, threadId, key, oldValue, newValue);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(txnId);
			clientMessage.Set(threadId);
			clientMessage.Set(key);
			clientMessage.Set(oldValue);
			clientMessage.Set(newValue);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalMapReplaceIfSameCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionalMapReplaceIfSameCodec.RequestParameters parameters = new TransactionalMapReplaceIfSameCodec.RequestParameters();
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
			IData oldValue;
			oldValue = null;
			oldValue = clientMessage.GetData();
			parameters.oldValue = oldValue;
			IData newValue;
			newValue = null;
			newValue = clientMessage.GetData();
			parameters.newValue = newValue;
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
			int requiredDataSize = TransactionalMapReplaceIfSameCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalMapReplaceIfSameCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionalMapReplaceIfSameCodec.ResponseParameters parameters = new TransactionalMapReplaceIfSameCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
