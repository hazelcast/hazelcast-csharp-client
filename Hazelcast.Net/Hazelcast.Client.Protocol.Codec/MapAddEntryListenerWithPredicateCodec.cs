using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class MapAddEntryListenerWithPredicateCodec
	{
		public static readonly MapMessageType RequestType = MapMessageType.MapAddentrylistenerwithpredicate;

		public const int ResponseType = 104;

		public const bool Retryable = true;

		public class RequestParameters
		{
			public static readonly MapMessageType Type = RequestType;

			public string name;

			public IData predicate;

			public bool includeValue;

			//************************ REQUEST *************************//
			public static int CalculateDataSize(string name, IData predicate, bool includeValue)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(name);
				dataSize += ParameterUtil.CalculateDataSize(predicate);
				dataSize += Bits.BooleanSizeInBytes;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest(string name, IData predicate, bool includeValue)
		{
			int requiredDataSize = MapAddEntryListenerWithPredicateCodec.RequestParameters.CalculateDataSize(name, predicate, includeValue);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.Set(name);
			clientMessage.Set(predicate);
			clientMessage.Set(includeValue);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapAddEntryListenerWithPredicateCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			MapAddEntryListenerWithPredicateCodec.RequestParameters parameters = new MapAddEntryListenerWithPredicateCodec.RequestParameters();
			string name;
			name = null;
			name = clientMessage.GetStringUtf8();
			parameters.name = name;
			IData predicate;
			predicate = null;
			predicate = clientMessage.GetData();
			parameters.predicate = predicate;
			bool includeValue;
			includeValue = clientMessage.GetBoolean();
			parameters.includeValue = includeValue;
			return parameters;
		}

		public class ResponseParameters
		{
			public string response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(string response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(response);
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(string response)
		{
			int requiredDataSize = MapAddEntryListenerWithPredicateCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static MapAddEntryListenerWithPredicateCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			MapAddEntryListenerWithPredicateCodec.ResponseParameters parameters = new MapAddEntryListenerWithPredicateCodec.ResponseParameters();
			string response;
			response = null;
			response = clientMessage.GetStringUtf8();
			parameters.response = response;
			return parameters;
		}

		//************************ EVENTS *************************//
		public static ClientMessage EncodeEntryEvent(IData key, IData value, IData oldValue, IData mergingValue, int eventType, string uuid, int numberOfAffectedEntries)
		{
			int dataSize = ClientMessage.HeaderSize;
			dataSize += Bits.BooleanSizeInBytes;
			if (key != null)
			{
				dataSize += ParameterUtil.CalculateDataSize(key);
			}
			dataSize += Bits.BooleanSizeInBytes;
			if (value != null)
			{
				dataSize += ParameterUtil.CalculateDataSize(value);
			}
			dataSize += Bits.BooleanSizeInBytes;
			if (oldValue != null)
			{
				dataSize += ParameterUtil.CalculateDataSize(oldValue);
			}
			dataSize += Bits.BooleanSizeInBytes;
			if (mergingValue != null)
			{
				dataSize += ParameterUtil.CalculateDataSize(mergingValue);
			}
			dataSize += Bits.IntSizeInBytes;
			dataSize += ParameterUtil.CalculateStringDataSize(uuid);
			dataSize += Bits.IntSizeInBytes;
			ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
			clientMessage.SetMessageType(EventMessageConst.EventEntry);
			clientMessage.AddFlag(ClientMessage.ListenerEventFlag);
			bool key_isNull;
			if (key == null)
			{
				key_isNull = true;
				clientMessage.Set(key_isNull);
			}
			else
			{
				key_isNull = false;
				clientMessage.Set(key_isNull);
				clientMessage.Set(key);
			}
			bool value_isNull;
			if (value == null)
			{
				value_isNull = true;
				clientMessage.Set(value_isNull);
			}
			else
			{
				value_isNull = false;
				clientMessage.Set(value_isNull);
				clientMessage.Set(value);
			}
			bool oldValue_isNull;
			if (oldValue == null)
			{
				oldValue_isNull = true;
				clientMessage.Set(oldValue_isNull);
			}
			else
			{
				oldValue_isNull = false;
				clientMessage.Set(oldValue_isNull);
				clientMessage.Set(oldValue);
			}
			bool mergingValue_isNull;
			if (mergingValue == null)
			{
				mergingValue_isNull = true;
				clientMessage.Set(mergingValue_isNull);
			}
			else
			{
				mergingValue_isNull = false;
				clientMessage.Set(mergingValue_isNull);
				clientMessage.Set(mergingValue);
			}
			clientMessage.Set(eventType);
			clientMessage.Set(uuid);
			clientMessage.Set(numberOfAffectedEntries);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public abstract class AbstractEventHandler
		{
			public static void Handle(ClientMessage clientMessage, HandleDelegate handle)
			{
				int messageType = clientMessage.GetMessageType();
				if (messageType == EventMessageConst.EventEntry)
				{
					IData key;
					key = null;
					bool key_isNull = clientMessage.GetBoolean();
					if (!key_isNull)
					{
						key = clientMessage.GetData();
					}
					IData value;
					value = null;
					bool value_isNull = clientMessage.GetBoolean();
					if (!value_isNull)
					{
						value = clientMessage.GetData();
					}
					IData oldValue;
					oldValue = null;
					bool oldValue_isNull = clientMessage.GetBoolean();
					if (!oldValue_isNull)
					{
						oldValue = clientMessage.GetData();
					}
					IData mergingValue;
					mergingValue = null;
					bool mergingValue_isNull = clientMessage.GetBoolean();
					if (!mergingValue_isNull)
					{
						mergingValue = clientMessage.GetData();
					}
					int eventType;
					eventType = clientMessage.GetInt();
					string uuid;
					uuid = null;
					uuid = clientMessage.GetStringUtf8();
					int numberOfAffectedEntries;
					numberOfAffectedEntries = clientMessage.GetInt();
					handle(key, value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries);
					return;
				}
                Logger.GetLogger(typeof(MapAddEntryListenerWithPredicateCodec)).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
			}

			public delegate void HandleDelegate(IData key, IData value, IData oldValue, IData mergingValue, int eventType, string uuid, int numberOfAffectedEntries);
		}
	}
}
