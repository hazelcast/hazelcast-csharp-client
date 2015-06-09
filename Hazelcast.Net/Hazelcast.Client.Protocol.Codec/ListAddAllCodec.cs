using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ListAddAllCodec
	{
		public static readonly ListMessageType RequestType = ListMessageType.ListAddall;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ListMessageType Type = RequestType;

			public string name;

			public ICollection<IData> valueList;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, ICollection<IData> valueList)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				foreach (IData valueList_item in valueList)
				{
					dataSize += ParameterUtil.CalculateDataSize(valueList_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, ICollection<IData> valueList)
		{
			int requiredDataSize = ListAddAllCodec.RequestParameters.CalculateDataSize(name, valueList);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(valueList.Count);
			foreach (IData valueList_item in valueList)
			{
				clientMessage.Set(valueList_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListAddAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ListAddAllCodec.RequestParameters parameters = new ListAddAllCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IList<IData> valueList;
			valueList = null;
			int valueList_size = clientMessage.GetInt();
			valueList = new AList<IData>(valueList_size);
			for (int valueList_index = 0; valueList_index < valueList_size; valueList_index++)
			{
				IData valueList_item;
				valueList_item = clientMessage.GetData();
				valueList.AddItem(valueList_item);
			}
			parameters.valueList = valueList;
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
			int requiredDataSize = ListAddAllCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListAddAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ListAddAllCodec.ResponseParameters parameters = new ListAddAllCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
