using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ExecutorServiceSubmitToAddressCodec
	{
		public static readonly ExecutorServiceMessageType RequestType = ExecutorServiceMessageType.ExecutorserviceSubmittoaddress;

		public const int ResponseType = 105;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ExecutorServiceMessageType Type = RequestType;

			public string name;

			public string uuid;

			public IData callable;

			public string hostname;

			public int port;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, string uuid, IData callable, string hostname, int port)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateStringDataSize(uuid);
				dataSize += ParameterUtil.CalculateDataSize(callable);
				dataSize += ParameterUtil.CalculateStringDataSize(hostname);
				dataSize += Bits.IntSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, string uuid, IData callable, string hostname, int port)
		{
			int requiredDataSize = ExecutorServiceSubmitToAddressCodec.RequestParameters.CalculateDataSize(name, uuid, callable, hostname, port);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(uuid);
			clientMessage.Set(callable);
			clientMessage.Set(hostname);
			clientMessage.Set(port);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ExecutorServiceSubmitToAddressCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ExecutorServiceSubmitToAddressCodec.RequestParameters parameters = new ExecutorServiceSubmitToAddressCodec.RequestParameters();
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
			string hostname;
			hostname = null;
			hostname = clientMessage.GetStringUtf8();
			parameters.hostname = hostname;
			int port;
			port = clientMessage.GetInt();
			parameters.port = port;
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
			int requiredDataSize = ExecutorServiceSubmitToAddressCodec.ResponseParameters.CalculateDataSize(response);
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

		public static ExecutorServiceSubmitToAddressCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ExecutorServiceSubmitToAddressCodec.ResponseParameters parameters = new ExecutorServiceSubmitToAddressCodec.ResponseParameters();
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
