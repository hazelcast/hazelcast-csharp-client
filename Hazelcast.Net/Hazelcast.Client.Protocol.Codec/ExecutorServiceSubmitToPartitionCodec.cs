using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ExecutorServiceSubmitToPartitionCodec
	{
		public static readonly ExecutorServiceMessageType RequestType = ExecutorServiceMessageType.ExecutorserviceSubmittopartition;

		public const int ResponseType = 105;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ExecutorServiceMessageType Type = RequestType;

			public string name;

			public string uuid;

			public IData callable;

			public int partitionId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string uuid, IData callable, int partitionId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(uuid);
				dataSize += ParameterUtil.CalculateDataSize(callable);
				dataSize += Bits.IntSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string uuid, IData callable, int partitionId)
		{
			int requiredDataSize = ExecutorServiceSubmitToPartitionCodec.RequestParameters.CalculateDataSize(name, uuid, callable, partitionId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(uuid);
			clientMessage.Set(callable);
			clientMessage.Set(partitionId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ExecutorServiceSubmitToPartitionCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ExecutorServiceSubmitToPartitionCodec.RequestParameters parameters = new ExecutorServiceSubmitToPartitionCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			string uuid;
			uuid = null;
			uuid = clientMessage.GetStringUtf8();
			parameters.uuid = uuid;
			IData callable;
			callable = null;
			callable = clientMessage.GetData();
			parameters.callable = callable;
			int partitionId;
			partitionId = clientMessage.GetInt();
			parameters.partitionId = partitionId;
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
			int requiredDataSize = ExecutorServiceSubmitToPartitionCodec.ResponseParameters.CalculateDataSize(response);
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

		public static ExecutorServiceSubmitToPartitionCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ExecutorServiceSubmitToPartitionCodec.ResponseParameters parameters = new ExecutorServiceSubmitToPartitionCodec.ResponseParameters();
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
