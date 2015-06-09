using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class SetCompareAndRemoveAllCodec
	{
		public static readonly SetMessageType RequestType = SetMessageType.SetCompareandremoveall;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly SetMessageType Type = RequestType;

			public string name;

			public ICollection<IData> valueSet;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, ICollection<IData> valueSet)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				foreach (IData valueSet_item in valueSet)
				{
					dataSize += ParameterUtil.CalculateDataSize(valueSet_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, ICollection<IData> valueSet)
		{
			int requiredDataSize = SetCompareAndRemoveAllCodec.RequestParameters.CalculateDataSize(name, valueSet);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(valueSet.Count);
			foreach (IData valueSet_item in valueSet)
			{
				clientMessage.Set(valueSet_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SetCompareAndRemoveAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			SetCompareAndRemoveAllCodec.RequestParameters parameters = new SetCompareAndRemoveAllCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			ICollection<IData> valueSet;
			valueSet = null;
			int valueSet_size = clientMessage.GetInt();
			valueSet = new HashSet<IData>(valueSet_size);
			for (int valueSet_index = 0; valueSet_index < valueSet_size; valueSet_index++)
			{
				IData valueSet_item;
				valueSet_item = clientMessage.GetData();
				valueSet.AddItem(valueSet_item);
			}
			parameters.valueSet = valueSet;
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
			int requiredDataSize = SetCompareAndRemoveAllCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static SetCompareAndRemoveAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			SetCompareAndRemoveAllCodec.ResponseParameters parameters = new SetCompareAndRemoveAllCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
