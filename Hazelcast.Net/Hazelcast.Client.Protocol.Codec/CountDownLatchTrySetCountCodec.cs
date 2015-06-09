using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class CountDownLatchTrySetCountCodec
	{
		public static readonly CountDownLatchMessageType RequestType = CountDownLatchMessageType.CountdownlatchTrysetcount;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly CountDownLatchMessageType Type = RequestType;

			public string name;

			public int count;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, int count)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, int count)
		{
			int requiredDataSize = CountDownLatchTrySetCountCodec.RequestParameters.CalculateDataSize(name, count);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(count);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static CountDownLatchTrySetCountCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			CountDownLatchTrySetCountCodec.RequestParameters parameters = new CountDownLatchTrySetCountCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			int count;
			count = clientMessage.GetInt();
			parameters.count = count;
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
			int requiredDataSize = CountDownLatchTrySetCountCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static CountDownLatchTrySetCountCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			CountDownLatchTrySetCountCodec.ResponseParameters parameters = new CountDownLatchTrySetCountCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
