using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ExecutorServiceCancelOnAddressCodec
	{
		public static readonly ExecutorServiceMessageType RequestType = ExecutorServiceMessageType.ExecutorserviceCancelonaddress;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ExecutorServiceMessageType Type = RequestType;

			public string uuid;

			public string hostname;

			public int port;

			public bool interrupt;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string uuid, string hostname, int port, bool interrupt)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(uuid);
				dataSize += ParameterUtil.CalculateStringDataSize(hostname);
				dataSize += Bits.IntSizeInBytes;
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string uuid, string hostname, int port, bool interrupt)
		{
			int requiredDataSize = ExecutorServiceCancelOnAddressCodec.RequestParameters.CalculateDataSize(uuid, hostname, port, interrupt);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(uuid);
			clientMessage.Set(hostname);
			clientMessage.Set(port);
			clientMessage.Set(interrupt);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ExecutorServiceCancelOnAddressCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ExecutorServiceCancelOnAddressCodec.RequestParameters parameters = new ExecutorServiceCancelOnAddressCodec.RequestParameters();
			string uuid;
			uuid = null;
			uuid = clientMessage.GetStringUtf8();
			parameters.uuid = uuid;
			string hostname;
			hostname = null;
			hostname = clientMessage.GetStringUtf8();
			parameters.hostname = hostname;
			int port;
			port = clientMessage.GetInt();
			parameters.port = port;
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
			int requiredDataSize = ExecutorServiceCancelOnAddressCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ExecutorServiceCancelOnAddressCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ExecutorServiceCancelOnAddressCodec.ResponseParameters parameters = new ExecutorServiceCancelOnAddressCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
