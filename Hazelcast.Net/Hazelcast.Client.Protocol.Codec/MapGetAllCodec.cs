using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapGetAllCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapGetall;

		public const int ResponseType = 108;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public ICollection<IData> keys;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, ICollection<IData> keys)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				foreach (IData keys_item in keys)
				{
					dataSize += ParameterUtil.CalculateDataSize(keys_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, ICollection<IData> keys)
		{
			int requiredDataSize = MapGetAllCodec.RequestParameters.CalculateDataSize(name, keys);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(keys.Count);
			foreach (IData keys_item in keys)
			{
				clientMessage.Set(keys_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapGetAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapGetAllCodec.RequestParameters parameters = new MapGetAllCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			ICollection<IData> keys;
			keys = null;
			int keys_size = clientMessage.GetInt();
			keys = new HashSet<IData>(keys_size);
			for (int keys_index = 0; keys_index < keys_size; keys_index++)
			{
				IData keys_item;
				keys_item = clientMessage.GetData();
				keys.AddItem(keys_item);
			}
			parameters.keys = keys;
			return parameters;
		}

		public class ResponseParameters
		{
			public IDictionary<IData, IData> map;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(IDictionary<IData, IData> map)
			{
				int dataSize = ClientMessage.HeaderSize;
				ICollection<IData> map_keySet = (ICollection<IData>)map.Keys;
				dataSize += Bits.IntSizeInBytes;
				foreach (IData map_keySet_item in map_keySet)
				{
					dataSize += ParameterUtil.CalculateDataSize(map_keySet_item);
				}
				ICollection<IData> map_values = (ICollection<IData>)map.Values;
				dataSize += Bits.IntSizeInBytes;
				foreach (IData map_values_item in map_values)
				{
					dataSize += ParameterUtil.CalculateDataSize(map_values_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(IDictionary<IData, IData> map)
		{
			int requiredDataSize = MapGetAllCodec.ResponseParameters.CalculateDataSize(map);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			ICollection<IData> map_keySet = (ICollection<IData>)map.Keys;
			clientMessage.Set(map_keySet.Count);
			foreach (IData map_keySet_item in map_keySet)
			{
				clientMessage.Set(map_keySet_item);
			}
			ICollection<IData> map_values = (ICollection<IData>)map.Values;
			clientMessage.Set(map_values.Count);
			foreach (IData map_values_item in map_values)
			{
				clientMessage.Set(map_values_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapGetAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapGetAllCodec.ResponseParameters parameters = new MapGetAllCodec.ResponseParameters();
			IDictionary<IData, IData> map;
			map = null;
			IList<IData> map_keySet;
			int map_keySet_size = clientMessage.GetInt();
			map_keySet = new AList<IData>(map_keySet_size);
			for (int map_keySet_index = 0; map_keySet_index < map_keySet_size; map_keySet_index++)
			{
				IData map_keySet_item;
				map_keySet_item = clientMessage.GetData();
				map_keySet.AddItem(map_keySet_item);
			}
			IList<IData> map_values;
			int map_values_size = clientMessage.GetInt();
			map_values = new AList<IData>(map_values_size);
			for (int map_values_index = 0; map_values_index < map_values_size; map_values_index++)
			{
				IData map_values_item;
				map_values_item = clientMessage.GetData();
				map_values.AddItem(map_values_item);
			}
			map = new Dictionary<IData, IData>();
			for (int map_index = 0; map_index < map_keySet_size; map_index++)
			{
				map[map_keySet[map_index]] = map_values[map_index];
			}
			parameters.map = map;
			return parameters;
		}
	}
}
