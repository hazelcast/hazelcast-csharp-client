using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientAddDistributedObjectListenerCodec
    {

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientAddDistributedObjectListener;
        public const int ResponseType = 104;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;

            public static int CalculateDataSize()
            {
                int dataSize = ClientMessage.HeaderSize;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest()
        {
            int requiredDataSize = RequestParameters.CalculateDataSize();
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
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

        public static ClientMessage EncodeDistributedObjectEvent(string name, string serviceName, string eventType)
        {
            int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(serviceName);
                dataSize += ParameterUtil.CalculateDataSize(eventType);

            ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
            clientMessage.SetMessageType(EventMessageConst.EventDistributedObject);
            clientMessage.AddFlag(ClientMessage.ListenerEventFlag);

            clientMessage.Set(name);
            clientMessage.Set(serviceName);
            clientMessage.Set(eventType);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleDistributedObject handleDistributedObject)
            {
                int messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventDistributedObject) {
            string name = null;
            name = clientMessage.GetStringUtf8();
            string serviceName = null;
            serviceName = clientMessage.GetStringUtf8();
            string eventType = null;
            eventType = clientMessage.GetStringUtf8();
                    handleDistributedObject(name, serviceName, eventType);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler)).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleDistributedObject(string name, string serviceName, string eventType);
       }

    }
}
