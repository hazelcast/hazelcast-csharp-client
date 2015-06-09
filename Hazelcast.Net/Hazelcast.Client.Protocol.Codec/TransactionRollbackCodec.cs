using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionRollbackCodec
	{
		public static readonly TransactionMessageType RequestType = TransactionMessageType.TransactionRollback;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionMessageType Type = RequestType;

			public string transactionId;

			public long threadId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string transactionId, long threadId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(transactionId);
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string transactionId, long threadId)
		{
			int requiredDataSize = TransactionRollbackCodec.RequestParameters.CalculateDataSize(transactionId, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(transactionId);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionRollbackCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionRollbackCodec.RequestParameters parameters = new TransactionRollbackCodec.RequestParameters();
			string transactionId;
			transactionId = null;
			transactionId = clientMessage.GetStringUtf8();
			parameters.transactionId = transactionId;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
			return parameters;
		}

		public class ResponseParameters
		{
			//************************ RESPONSE *************************//
			public static int CalculateDataSize()
			{
				int dataSize = ClientMessage.HeaderSize;
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse()
		{
			int requiredDataSize = TransactionRollbackCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionRollbackCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionRollbackCodec.ResponseParameters parameters = new TransactionRollbackCodec.ResponseParameters();
			return parameters;
		}
	}
}
