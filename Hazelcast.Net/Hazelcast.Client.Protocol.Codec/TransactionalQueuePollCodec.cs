using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionalQueuePollCodec
	{
		public static readonly TransactionalQueueMessageType RequestType = TransactionalQueueMessageType.TransactionalqueuePoll;

		public const int ResponseType = 105;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionalQueueMessageType Type = RequestType;

			public string name;

			public string txnId;

			public long threadId;

			public long timeout;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string txnId, long threadId, long timeout)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(txnId);
				dataSize += Bits.LongSizeInBytes;
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string txnId, long threadId, long timeout)
		{
			int requiredDataSize = TransactionalQueuePollCodec.RequestParameters.CalculateDataSize(name, txnId, threadId, timeout);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(txnId);
			clientMessage.Set(threadId);
			clientMessage.Set(timeout);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalQueuePollCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionalQueuePollCodec.RequestParameters parameters = new TransactionalQueuePollCodec.RequestParameters();
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
			long timeout;
			timeout = clientMessage.GetLong();
			parameters.timeout = timeout;
			return parameters;
		}

		public class ResponseParameters
		{
			public IData response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(IData response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.BooleanSizeInBytes;
				if (response != null)
				{
					dataSize += ParameterUtil.CalculateDataSize(response);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(IData response)
		{
			int requiredDataSize = TransactionalQueuePollCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			bool response_isNull;
			if (response == null)
			{
				response_isNull = true;
				clientMessage.Set(response_isNull);
			}
			else
			{
				response_isNull = false;
				clientMessage.Set(response_isNull);
				clientMessage.Set(response);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalQueuePollCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionalQueuePollCodec.ResponseParameters parameters = new TransactionalQueuePollCodec.ResponseParameters();
			IData response;
			response = null;
			bool response_isNull = clientMessage.GetBoolean();
			if (!response_isNull)
			{
				response = clientMessage.GetData();
				parameters.response = response;
			}
			return parameters;
		}
	}
}
