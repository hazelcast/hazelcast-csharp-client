using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapLoadGivenKeysCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapLoadgivenkeys;

		public const int ResponseType = 100;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public ICollection<IData> keys;

			public bool replaceExistingValues;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, ICollection<IData> keys, bool replaceExistingValues)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += Bits.IntSizeInBytes;
				foreach (IData keys_item in keys)
				{
					dataSize += ParameterUtil.CalculateDataSize(keys_item);
				}
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, ICollection<IData> keys, bool replaceExistingValues)
		{
			int requiredDataSize = MapLoadGivenKeysCodec.RequestParameters.CalculateDataSize(name, keys, replaceExistingValues);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(keys.Count);
			foreach (IData keys_item in keys)
			{
				clientMessage.Set(keys_item);
			}
			clientMessage.Set(replaceExistingValues);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapLoadGivenKeysCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapLoadGivenKeysCodec.RequestParameters parameters = new MapLoadGivenKeysCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IList<IData> keys;
			keys = null;
			int keys_size = clientMessage.GetInt();
			keys = new AList<IData>(keys_size);
			for (int keys_index = 0; keys_index < keys_size; keys_index++)
			{
				IData keys_item;
				keys_item = clientMessage.GetData();
				keys.AddItem(keys_item);
			}
			parameters.keys = keys;
			bool replaceExistingValues;
			replaceExistingValues = clientMessage.GetBoolean();
			parameters.replaceExistingValues = replaceExistingValues;
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
			int requiredDataSize = MapLoadGivenKeysCodec.ResponseParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapLoadGivenKeysCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapLoadGivenKeysCodec.ResponseParameters parameters = new MapLoadGivenKeysCodec.ResponseParameters();
			return parameters;
		}
	}
}
