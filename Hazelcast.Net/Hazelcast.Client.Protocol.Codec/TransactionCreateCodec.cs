using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionCreateCodec
	{
		public static readonly TransactionMessageType RequestType = TransactionMessageType.TransactionCreate;

		public const int ResponseType = 104;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionMessageType Type = RequestType;

			public long timeout;

			public int durability;

			public int transactionType;

			public long threadId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(long timeout, int durability, int transactionType, long threadId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.LongSizeInBytes;
				dataSize += Bits.IntSizeInBytes;
				dataSize += Bits.IntSizeInBytes;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(long timeout, int durability, int transactionType, long threadId)
		{
			int requiredDataSize = TransactionCreateCodec.RequestParameters.CalculateDataSize(timeout, durability, transactionType, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(timeout);
			clientMessage.Set(durability);
			clientMessage.Set(transactionType);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionCreateCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionCreateCodec.RequestParameters parameters = new TransactionCreateCodec.RequestParameters();
			long timeout;
			timeout = clientMessage.GetLong();
			parameters.timeout = timeout;
			int durability;
			durability = clientMessage.GetInt();
			parameters.durability = durability;
			int transactionType;
			transactionType = clientMessage.GetInt();
			parameters.transactionType = transactionType;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
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
			int requiredDataSize = TransactionCreateCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionCreateCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionCreateCodec.ResponseParameters parameters = new TransactionCreateCodec.ResponseParameters();
			string response;
			response = null;
			response = clientMessage.GetStringUtf8();
			parameters.response = response;
			return parameters;
		}
	}
}
