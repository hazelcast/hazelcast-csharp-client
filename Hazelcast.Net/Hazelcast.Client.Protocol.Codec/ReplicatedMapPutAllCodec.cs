using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ReplicatedMapPutAllCodec
	{
		public static readonly ReplicatedMapMessageType RequestType = ReplicatedMapMessageType.ReplicatedmapPutall;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ReplicatedMapMessageType Type = RequestType;

			public string name;

			public IDictionary<IData, IData> map;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IDictionary<IData, IData> map)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
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

		public static ClientMessage EncodeRequest(string name, IDictionary<IData, IData> map)
		{
			int requiredDataSize = ReplicatedMapPutAllCodec.RequestParameters.CalculateDataSize(name, map);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
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

		public static ReplicatedMapPutAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ReplicatedMapPutAllCodec.RequestParameters parameters = new ReplicatedMapPutAllCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
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
			int requiredDataSize = ReplicatedMapPutAllCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ReplicatedMapPutAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ReplicatedMapPutAllCodec.ResponseParameters parameters = new ReplicatedMapPutAllCodec.ResponseParameters();
			return parameters;
		}
	}
}
