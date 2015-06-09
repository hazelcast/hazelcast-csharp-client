using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapValuesWithPredicateCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapValueswithpredicate;

		public const int ResponseType = 106;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public IData predicate;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData predicate)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(predicate);
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData predicate)
		{
			int requiredDataSize = MapValuesWithPredicateCodec.RequestParameters.CalculateDataSize(name, predicate);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(predicate);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapValuesWithPredicateCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapValuesWithPredicateCodec.RequestParameters parameters = new MapValuesWithPredicateCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData predicate;
			predicate = null;
			predicate = clientMessage.GetData();
			parameters.predicate = predicate;
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
			int requiredDataSize = MapValuesWithPredicateCodec.ResponseParameters.CalculateDataSize(list);
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

		public static MapValuesWithPredicateCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapValuesWithPredicateCodec.ResponseParameters parameters = new MapValuesWithPredicateCodec.ResponseParameters();
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
