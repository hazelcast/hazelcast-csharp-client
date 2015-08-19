using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapAddEntryListenerToKeyWithPredicateCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapAddEntryListenerToKeyWithPredicate;
        public const int ResponseType = 104;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public IData key;
            public IData predicate;
            public bool includeValue;

            public static int CalculateDataSize(string name, IData key, IData predicate, bool includeValue)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(key);
                dataSize += ParameterUtil.CalculateDataSize(predicate);
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IData key, IData predicate, bool includeValue)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, key, predicate, includeValue);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(key);
            clientMessage.Set(predicate);
            clientMessage.Set(includeValue);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public string response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            string response = null;
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
                dataSize += ParameterUtil.CalculateDataSize(uuid);
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
                key_isNull= false;
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
                value_isNull= false;
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
                oldValue_isNull= false;
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
                mergingValue_isNull= false;
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
            public static void Handle(IClientMessage clientMessage, HandleEntry handleEntry)
            {
                int messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventEntry) {
            IData key = null;
            bool key_isNull = clientMessage.GetBoolean();
            if (!key_isNull)
            {
            key = clientMessage.GetData();
            }
            IData value = null;
            bool value_isNull = clientMessage.GetBoolean();
            if (!value_isNull)
            {
            value = clientMessage.GetData();
            }
            IData oldValue = null;
            bool oldValue_isNull = clientMessage.GetBoolean();
            if (!oldValue_isNull)
            {
            oldValue = clientMessage.GetData();
            }
            IData mergingValue = null;
            bool mergingValue_isNull = clientMessage.GetBoolean();
            if (!mergingValue_isNull)
            {
            mergingValue = clientMessage.GetData();
            }
            int eventType ;
            eventType = clientMessage.GetInt();
            string uuid = null;
            uuid = clientMessage.GetStringUtf8();
            int numberOfAffectedEntries ;
            numberOfAffectedEntries = clientMessage.GetInt();
                    handleEntry(key, value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler)).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleEntry(IData key, IData value, IData oldValue, IData mergingValue, int eventType, string uuid, int numberOfAffectedEntries);
       }

    }
}
