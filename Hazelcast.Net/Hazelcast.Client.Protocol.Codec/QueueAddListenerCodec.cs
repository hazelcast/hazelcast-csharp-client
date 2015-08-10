using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class QueueAddListenerCodec
    {

        public static readonly QueueMessageType RequestType = QueueMessageType.QueueAddListener;
        public const int ResponseType = 104;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly QueueMessageType TYPE = RequestType;
            public string name;
            public bool includeValue;

            public static int CalculateDataSize(string name, bool includeValue)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, bool includeValue)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, includeValue);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
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

        public static ClientMessage EncodeItemEvent(IData item, string uuid, int eventType)
        {
            int dataSize = ClientMessage.HeaderSize;
                dataSize += Bits.BooleanSizeInBytes;
                if (item != null)
                {
                dataSize += ParameterUtil.CalculateDataSize(item);
                }
                dataSize += ParameterUtil.CalculateDataSize(uuid);
                dataSize += Bits.IntSizeInBytes;

            ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
            clientMessage.SetMessageType(EventMessageConst.EventItem);
            clientMessage.AddFlag(ClientMessage.ListenerEventFlag);

            bool item_isNull;
            if (item == null)
            {
                item_isNull = true;
                clientMessage.Set(item_isNull);
            }
            else
            {
                item_isNull= false;
                clientMessage.Set(item_isNull);
            clientMessage.Set(item);
            }
            clientMessage.Set(uuid);
            clientMessage.Set(eventType);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleDelegate handle)
            {
                int messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventItem) {
            IData item = null;
            bool item_isNull = clientMessage.GetBoolean();
            if (!item_isNull)
            {
            item = clientMessage.GetData();
            }
            string uuid = null;
            uuid = clientMessage.GetStringUtf8();
            int eventType ;
            eventType = clientMessage.GetInt();
                    handle(item, uuid, eventType);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler)).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleDelegate(IData item, string uuid, int eventType);

       }

    }
}
