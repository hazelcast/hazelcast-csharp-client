using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ListAddAllWithIndexCodec
	{
		public static readonly ListMessageType RequestType = ListMessageType.ListAddallwithindex;

		public const int ResponseType = 101;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ListMessageType Type = RequestType;

			public string name;

			public int index;

			public ICollection<IData> valueList;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, int index, ICollection<IData> valueList)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				dataSize += Bits.IntSizeInBytes;
				foreach (IData valueList_item in valueList)
				{
					dataSize += ParameterUtil.CalculateDataSize(valueList_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, int index, ICollection<IData> valueList)
		{
			int requiredDataSize = ListAddAllWithIndexCodec.RequestParameters.CalculateDataSize(name, index, valueList);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(index);
			clientMessage.Set(valueList.Count);
			foreach (IData valueList_item in valueList)
			{
				clientMessage.Set(valueList_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListAddAllWithIndexCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ListAddAllWithIndexCodec.RequestParameters parameters = new ListAddAllWithIndexCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			int index;
			index = clientMessage.GetInt();
			parameters.index = index;
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
			int requiredDataSize = ListAddAllWithIndexCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ListAddAllWithIndexCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ListAddAllWithIndexCodec.ResponseParameters parameters = new ListAddAllWithIndexCodec.ResponseParameters();
			bool response;
			response = clientMessage.GetBoolean();
			parameters.response = response;
			return parameters;
		}
	}
}
