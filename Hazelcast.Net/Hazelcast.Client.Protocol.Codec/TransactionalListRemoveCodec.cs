using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionalListRemoveCodec
	{
		public static readonly TransactionalListMessageType RequestType = TransactionalListMessageType.TransactionallistRemove;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionalListMessageType Type = RequestType;

			public string name;

			public string txnId;

			public long threadId;

			public IData item;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string txnId, long threadId, IData item)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(txnId);
				dataSize += Bits.LongSizeInBytes;
				dataSize += ParameterUtil.CalculateDataSize(item);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string txnId, long threadId, IData item)
		{
			int requiredDataSize = TransactionalListRemoveCodec.RequestParameters.CalculateDataSize(name, txnId, threadId, item);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(txnId);
			clientMessage.Set(threadId);
			clientMessage.Set(item);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalListRemoveCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionalListRemoveCodec.RequestParameters parameters = new TransactionalListRemoveCodec.RequestParameters();
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
			IData item;
			item = null;
			item = clientMessage.GetData();
			parameters.item = item;
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
			int requiredDataSize = TransactionalListRemoveCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalListRemoveCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionalListRemoveCodec.ResponseParameters parameters = new TransactionalListRemoveCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
