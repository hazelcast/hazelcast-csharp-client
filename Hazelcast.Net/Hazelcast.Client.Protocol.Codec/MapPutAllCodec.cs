using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapPutAllCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapPutall;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public IDictionary<IData, IData> entries;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IDictionary<IData, IData> entries)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				ICollection<IData> entries_keySet = (ICollection<IData>)entries.Keys;
				dataSize += Bits.IntSizeInBytes;
				foreach (IData entries_keySet_item in entries_keySet)
				{
					dataSize += ParameterUtil.CalculateDataSize(entries_keySet_item);
				}
				ICollection<IData> entries_values = (ICollection<IData>)entries.Values;
				dataSize += Bits.IntSizeInBytes;
				foreach (IData entries_values_item in entries_values)
				{
					dataSize += ParameterUtil.CalculateDataSize(entries_values_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IDictionary<IData, IData> entries)
		{
			int requiredDataSize = MapPutAllCodec.RequestParameters.CalculateDataSize(name, entries);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			ICollection<IData> entries_keySet = (ICollection<IData>)entries.Keys;
			clientMessage.Set(entries_keySet.Count);
			foreach (IData entries_keySet_item in entries_keySet)
			{
				clientMessage.Set(entries_keySet_item);
			}
			ICollection<IData> entries_values = (ICollection<IData>)entries.Values;
			clientMessage.Set(entries_values.Count);
			foreach (IData entries_values_item in entries_values)
			{
				clientMessage.Set(entries_values_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapPutAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapPutAllCodec.RequestParameters parameters = new MapPutAllCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IDictionary<IData, IData> entries;
			entries = null;
			IList<IData> entries_keySet;
			int entries_keySet_size = clientMessage.GetInt();
			entries_keySet = new AList<IData>(entries_keySet_size);
			for (int entries_keySet_index = 0; entries_keySet_index < entries_keySet_size; entries_keySet_index++)
			{
				IData entries_keySet_item;
				entries_keySet_item = clientMessage.GetData();
				entries_keySet.AddItem(entries_keySet_item);
			}
			IList<IData> entries_values;
			int entries_values_size = clientMessage.GetInt();
			entries_values = new AList<IData>(entries_values_size);
			for (int entries_values_index = 0; entries_values_index < entries_values_size; entries_values_index++)
			{
				IData entries_values_item;
				entries_values_item = clientMessage.GetData();
				entries_values.AddItem(entries_values_item);
			}
			entries = new Dictionary<IData, IData>();
			for (int entries_index = 0; entries_index < entries_keySet_size; entries_index++)
			{
				entries[entries_keySet[entries_index]] = entries_values[entries_index];
			}
			parameters.entries = entries;
			return parameters;
		}

		public class ResponseParameters
		{
			//************************ RESPONSE *************************//
			public static int CalculateDataSize()
			{
				int dataSize = ClientMessage.HeaderSize;
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse()
		{
			int requiredDataSize = MapPutAllCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapPutAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapPutAllCodec.ResponseParameters parameters = new MapPutAllCodec.ResponseParameters();
			return parameters;
		}
	}
}
