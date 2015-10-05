using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapAddEntryListenerToKeyCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapAddEntryListenerToKey;
        public const int ResponseType = 104;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public IData key;
            public bool includeValue;
            public int listenerFlags;

            public static int CalculateDataSize(string name, IData key, bool includeValue, int listenerFlags)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(key);
                dataSize += Bits.BooleanSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IData key, bool includeValue, int listenerFlags)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, key, includeValue, listenerFlags);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(key);
            clientMessage.Set(includeValue);
            clientMessage.Set(listenerFlags);
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
        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleEntry handleEntry)
            {
                int messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventEntry)
                {
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
                    int eventType;
                    eventType = clientMessage.GetInt();
                    string uuid = null;
                    uuid = clientMessage.GetStringUtf8();
                    int numberOfAffectedEntries;
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
