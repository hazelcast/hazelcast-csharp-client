using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ListAddListenerCodec
    {

        public static readonly ListMessageType RequestType = ListMessageType.ListAddListener;
        public const int ResponseType = 104;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ListMessageType TYPE = RequestType;
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
        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleItem handleItem)
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
                    handleItem(item, uuid, eventType);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler)).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleItem(IData item, string uuid, int eventType);
       }

    }
}
