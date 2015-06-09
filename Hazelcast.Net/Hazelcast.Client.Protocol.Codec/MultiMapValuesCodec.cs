using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MultiMapValuesCodec
	{
		public static readonly MultiMapMessageType RequestType = MultiMapMessageType.MultimapValues;

		public const int ResponseType = 106;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly MultiMapMessageType Type = RequestType;

			public string name;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name)
		{
			int requiredDataSize = MultiMapValuesCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapValuesCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MultiMapValuesCodec.RequestParameters parameters = new MultiMapValuesCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			return parameters;
		}

		public class ResponseParameters
		{
			public ICollection<IData> list;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(ICollection<IData> list)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.IntSizeInBytes;
				foreach (IData list_item in list)
				{
					dataSize += ParameterUtil.CalculateDataSize(list_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(ICollection<IData> list)
		{
			int requiredDataSize = MultiMapValuesCodec.ResponseParameters.CalculateDataSize(list);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(list.Count);
			foreach (IData list_item in list)
			{
				clientMessage.Set(list_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapValuesCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MultiMapValuesCodec.ResponseParameters parameters = new MultiMapValuesCodec.ResponseParameters();
			IList<IData> list;
			list = null;
			int list_size = clientMessage.GetInt();
			list = new AList<IData>(list_size);
			for (int list_index = 0; list_index < list_size; list_index++)
			{
				IData list_item;
				list_item = clientMessage.GetData();
				list.AddItem(list_item);
			}
			parameters.list = list;
			return parameters;
		}
	}
}
