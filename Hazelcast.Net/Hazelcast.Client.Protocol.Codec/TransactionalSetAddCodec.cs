using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class TransactionalSetAddCodec
	{
		public static readonly TransactionalSetMessageType RequestType = TransactionalSetMessageType.TransactionalsetAdd;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly TransactionalSetMessageType Type = RequestType;

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
			int requiredDataSize = TransactionalSetAddCodec.RequestParameters.CalculateDataSize(name, txnId, threadId, item);
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

		public static TransactionalSetAddCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			TransactionalSetAddCodec.RequestParameters parameters = new TransactionalSetAddCodec.RequestParameters();
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
			int requiredDataSize = TransactionalSetAddCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static TransactionalSetAddCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			TransactionalSetAddCodec.ResponseParameters parameters = new TransactionalSetAddCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
