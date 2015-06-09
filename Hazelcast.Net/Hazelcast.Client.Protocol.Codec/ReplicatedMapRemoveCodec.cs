using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ReplicatedMapRemoveCodec
	{
		public static readonly ReplicatedMapMessageType RequestType = ReplicatedMapMessageType.ReplicatedmapRemove;

		public const int ResponseType = 105;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ReplicatedMapMessageType Type = RequestType;

			public string name;

			public IData key;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData key)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(key);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData key)
		{
			int requiredDataSize = ReplicatedMapRemoveCodec.RequestParameters.CalculateDataSize(name, key);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(key);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ReplicatedMapRemoveCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ReplicatedMapRemoveCodec.RequestParameters parameters = new ReplicatedMapRemoveCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData key;
			key = null;
			key = clientMessage.GetData();
			parameters.key = key;
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
			int requiredDataSize = ReplicatedMapRemoveCodec.ResponseParameters.CalculateDataSize(response);
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

		public static ReplicatedMapRemoveCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ReplicatedMapRemoveCodec.ResponseParameters parameters = new ReplicatedMapRemoveCodec.ResponseParameters();
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
