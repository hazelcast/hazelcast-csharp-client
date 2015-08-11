using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class TopicAddMessageListenerCodec
    {

        public static readonly TopicMessageType RequestType = TopicMessageType.TopicAddMessageListener;
        public const int ResponseType = 104;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly TopicMessageType TYPE = RequestType;
            public string name;

            public static int CalculateDataSize(string name)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
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

        public static ClientMessage EncodeTopicEvent(IData item, long publishTime, string uuid)
        {
            int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(item);
                dataSize += Bits.LongSizeInBytes;
                dataSize += ParameterUtil.CalculateDataSize(uuid);

            ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
            clientMessage.SetMessageType(EventMessageConst.EventTopic);
            clientMessage.AddFlag(ClientMessage.ListenerEventFlag);

            clientMessage.Set(item);
            clientMessage.Set(publishTime);
            clientMessage.Set(uuid);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleDelegate handle)
            {
                int messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventTopic) {
            IData item = null;
            item = clientMessage.GetData();
            long publishTime ;
            publishTime = clientMessage.GetLong();
            string uuid = null;
            uuid = clientMessage.GetStringUtf8();
                    handle(item, publishTime, uuid);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler)).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleDelegate(IData item, long publishTime, string uuid);

       }

    }
}
