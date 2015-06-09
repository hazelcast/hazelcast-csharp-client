using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapGetEntryViewCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapGetentryview;

		public const int ResponseType = 113;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public IData key;

			public long threadId;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData key, long threadId)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(key);
				dataSize += Bits.LongSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData key, long threadId)
		{
			int requiredDataSize = MapGetEntryViewCodec.RequestParameters.CalculateDataSize(name, key, threadId);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(key);
			clientMessage.Set(threadId);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapGetEntryViewCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapGetEntryViewCodec.RequestParameters parameters = new MapGetEntryViewCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData key;
			key = null;
			key = clientMessage.GetData();
			parameters.key = key;
			long threadId;
			threadId = clientMessage.GetLong();
			parameters.threadId = threadId;
			return parameters;
		}

		public class ResponseParameters
		{
			public SimpleEntryView<IData, IData> dataEntryView;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(SimpleEntryView<IData, IData> dataEntryView)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += Bits.BooleanSizeInBytes;
				if (dataEntryView != null)
				{
					dataSize += EntryViewCodec.CalculateDataSize(dataEntryView);
				}
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(SimpleEntryView<IData, IData> dataEntryView)
		{
			int requiredDataSize = MapGetEntryViewCodec.ResponseParameters.CalculateDataSize(dataEntryView);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			bool dataEntryView_isNull;
			if (dataEntryView == null)
			{
				dataEntryView_isNull = true;
				clientMessage.Set(dataEntryView_isNull);
			}
			else
			{
				dataEntryView_isNull = false;
				clientMessage.Set(dataEntryView_isNull);
				EntryViewCodec.Encode(dataEntryView, clientMessage);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapGetEntryViewCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapGetEntryViewCodec.ResponseParameters parameters = new MapGetEntryViewCodec.ResponseParameters();
			SimpleEntryView<IData, IData> dataEntryView;
			dataEntryView = null;
			bool dataEntryView_isNull = clientMessage.GetBoolean();
			if (!dataEntryView_isNull)
			{
				dataEntryView = EntryViewCodec.Decode(clientMessage);
				parameters.dataEntryView = dataEntryView;
			}
			return parameters;
		}
	}
}
