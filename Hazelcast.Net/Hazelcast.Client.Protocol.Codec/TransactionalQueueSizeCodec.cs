using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionalQueueSizeCodec
	{
		public static readonly TransactionalQueueMessageType RequestType = TransactionalQueueMessageType.TransactionalqueueSize;

		public const int ResponseType = 102;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionalQueueMessageType Type = RequestType;

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
			int requiredDataSize = TransactionalQueueSizeCodec.RequestParameters.CalculateDataSize(name, txnId, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(txnId);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalQueueSizeCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionalQueueSizeCodec.RequestParameters parameters = new TransactionalQueueSizeCodec.RequestParameters();
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
			public int response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(int response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.IntSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(int response)
		{
			int requiredDataSize = TransactionalQueueSizeCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalQueueSizeCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionalQueueSizeCodec.ResponseParameters parameters = new TransactionalQueueSizeCodec.ResponseParameters();
			int response;
			response = clientMessage.GetInt();
			parameters.response = response;
			return parameters;
		}
	}
}
