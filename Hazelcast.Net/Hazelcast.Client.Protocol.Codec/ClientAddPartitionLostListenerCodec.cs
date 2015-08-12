using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientAddPartitionLostListenerCodec
    {

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientAddPartitionLostListener;
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

        public static ClientMessage EncodePartitionLostEvent(int partitionId, int lostBackupCount, Address source)
        {
            int dataSize = ClientMessage.HeaderSize;
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.BooleanSizeInBytes;
                if (source != null)
                {
                dataSize += AddressCodec.CalculateDataSize(source);
                }

            ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
            clientMessage.SetMessageType(EventMessageConst.EventPartitionLost);
            clientMessage.AddFlag(ClientMessage.ListenerEventFlag);

            clientMessage.Set(partitionId);
            clientMessage.Set(lostBackupCount);
            bool source_isNull;
            if (source == null)
            {
                source_isNull = true;
                clientMessage.Set(source_isNull);
            }
            else
            {
                source_isNull= false;
                clientMessage.Set(source_isNull);
            AddressCodec.Encode(source, clientMessage);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleDelegate handle)
            {
                int messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventPartitionLost) {
            int partitionId ;
            partitionId = clientMessage.GetInt();
            int lostBackupCount ;
            lostBackupCount = clientMessage.GetInt();
            Address source = null;
            bool source_isNull = clientMessage.GetBoolean();
            if (!source_isNull)
            {
            source = AddressCodec.Decode(clientMessage);
            }
                    handle(partitionId, lostBackupCount, source);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler)).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleDelegate(int partitionId, int lostBackupCount, Address source);

       }

    }
}
