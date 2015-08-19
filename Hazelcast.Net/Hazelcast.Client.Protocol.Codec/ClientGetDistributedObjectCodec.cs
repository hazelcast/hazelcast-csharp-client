using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientGetDistributedObjectCodec
    {

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientGetDistributedObject;
        public const int ResponseType = 110;
        public const bool Retryable = false;

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
            public ISet<DistributedObjectInfo> infoCollection;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            ISet<DistributedObjectInfo> infoCollection = null;
            int infoCollection_size = clientMessage.GetInt();
            infoCollection = new HashSet<DistributedObjectInfo>();
            for (int infoCollection_index = 0; infoCollection_index<infoCollection_size; infoCollection_index++) {
                DistributedObjectInfo infoCollection_item;
            infoCollection_item = DistributedObjectInfoCodec.Decode(clientMessage);
                infoCollection.Add(infoCollection_item);
            }
            parameters.infoCollection = infoCollection;
            return parameters;
        }

    }
}
