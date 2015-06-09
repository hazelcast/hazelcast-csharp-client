using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionCommitCodec
	{
		public static readonly TransactionMessageType RequestType = TransactionMessageType.TransactionCommit;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionMessageType Type = RequestType;

			public string transactionId;

			public long threadId;

			public bool prepareAndCommit;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string transactionId, long threadId, bool prepareAndCommit)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(transactionId);
				dataSize += Bits.LongSizeInBytes;
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string transactionId, long threadId, bool prepareAndCommit)
		{
			int requiredDataSize = TransactionCommitCodec.RequestParameters.CalculateDataSize(transactionId, threadId, prepareAndCommit);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(transactionId);
			clientMessage.Set(threadId);
			clientMessage.Set(prepareAndCommit);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionCommitCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionCommitCodec.RequestParameters parameters = new TransactionCommitCodec.RequestParameters();
			string transactionId;
			transactionId = null;
			transactionId = clientMessage.GetStringUtf8();
			parameters.transactionId = transactionId;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
			bool prepareAndCommit;
			prepareAndCommit = clientMessage.GetBoolean();
			parameters.prepareAndCommit = prepareAndCommit;
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
			int requiredDataSize = TransactionCommitCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionCommitCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionCommitCodec.ResponseParameters parameters = new TransactionCommitCodec.ResponseParameters();
			return parameters;
		}
	}
}
