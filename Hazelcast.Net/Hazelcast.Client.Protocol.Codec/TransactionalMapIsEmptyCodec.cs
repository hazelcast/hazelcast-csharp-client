using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionalMapIsEmptyCodec
	{
		public static readonly TransactionalMapMessageType RequestType = TransactionalMapMessageType.TransactionalmapIsempty;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionalMapMessageType Type = RequestType;

			public string name;

			public string txnId;

			public long threadId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string txnId, long threadId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(txnId);
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string txnId, long threadId)
		{
			int requiredDataSize = TransactionalMapIsEmptyCodec.RequestParameters.CalculateDataSize(name, txnId, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(txnId);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalMapIsEmptyCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionalMapIsEmptyCodec.RequestParameters parameters = new TransactionalMapIsEmptyCodec.RequestParameters();
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
			int requiredDataSize = TransactionalMapIsEmptyCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalMapIsEmptyCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionalMapIsEmptyCodec.ResponseParameters parameters = new TransactionalMapIsEmptyCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
