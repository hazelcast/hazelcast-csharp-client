using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MultiMapEntrySetCodec
	{
		public static readonly MultiMapMessageType RequestType = MultiMapMessageType.MultimapEntryset;

		public const int ResponseType = 116;

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
			int requiredDataSize = MultiMapEntrySetCodec.RequestParameters.CalculateDataSize(name);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapEntrySetCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MultiMapEntrySetCodec.RequestParameters parameters = new MultiMapEntrySetCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			return parameters;
		}

		public class ResponseParameters
		{
			public ICollection<IData> keys;

			public ICollection<IData> values;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(ICollection<IData> keys, ICollection<IData> values)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.IntSizeInBytes;
				foreach (IData keys_item in keys)
				{
					dataSize += ParameterUtil.CalculateDataSize(keys_item);
				}
				dataSize += Bits.IntSizeInBytes;
				foreach (IData values_item in values)
				{
					dataSize += ParameterUtil.CalculateDataSize(values_item);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(ICollection<IData> keys, ICollection<IData> values)
		{
			int requiredDataSize = MultiMapEntrySetCodec.ResponseParameters.CalculateDataSize(keys, values);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(keys.Count);
			foreach (IData keys_item in keys)
			{
				clientMessage.Set(keys_item);
			}
			clientMessage.Set(values.Count);
			foreach (IData values_item in values)
			{
				clientMessage.Set(values_item);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MultiMapEntrySetCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MultiMapEntrySetCodec.ResponseParameters parameters = new MultiMapEntrySetCodec.ResponseParameters();
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
			IList<IData> values;
			values = null;
			int values_size = clientMessage.GetInt();
			values = new AList<IData>(values_size);
			for (int values_index = 0; values_index < values_size; values_index++)
			{
				IData values_item;
				values_item = clientMessage.GetData();
				values.AddItem(values_item);
			}
			parameters.values = values;
			return parameters;
		}
	}
}
