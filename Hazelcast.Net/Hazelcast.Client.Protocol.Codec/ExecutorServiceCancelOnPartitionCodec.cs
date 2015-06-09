using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ExecutorServiceCancelOnPartitionCodec
	{
		public static readonly ExecutorServiceMessageType RequestType = ExecutorServiceMessageType.ExecutorserviceCancelonpartition;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ExecutorServiceMessageType Type = RequestType;

			public string uuid;

			public int partitionId;

			public bool interrupt;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string uuid, int partitionId, bool interrupt)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(uuid);
				dataSize += Bits.IntSizeInBytes;
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string uuid, int partitionId, bool interrupt)
		{
			int requiredDataSize = ExecutorServiceCancelOnPartitionCodec.RequestParameters.CalculateDataSize(uuid, partitionId, interrupt);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(uuid);
			clientMessage.Set(partitionId);
			clientMessage.Set(interrupt);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ExecutorServiceCancelOnPartitionCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ExecutorServiceCancelOnPartitionCodec.RequestParameters parameters = new ExecutorServiceCancelOnPartitionCodec.RequestParameters();
			string uuid;
			uuid = null;
			uuid = clientMessage.GetStringUtf8();
			parameters.uuid = uuid;
			int partitionId;
			partitionId = clientMessage.GetInt();
			parameters.partitionId = partitionId;
			bool interrupt;
			interrupt = clientMessage.GetBoolean();
			parameters.interrupt = interrupt;
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
			int requiredDataSize = ExecutorServiceCancelOnPartitionCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ExecutorServiceCancelOnPartitionCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ExecutorServiceCancelOnPartitionCodec.ResponseParameters parameters = new ExecutorServiceCancelOnPartitionCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
