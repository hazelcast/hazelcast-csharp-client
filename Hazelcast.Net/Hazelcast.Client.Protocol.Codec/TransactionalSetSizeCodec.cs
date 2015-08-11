using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class TransactionalSetSizeCodec
    {

        public static readonly TransactionalSetMessageType RequestType = TransactionalSetMessageType.TransactionalSetSize;
        public const int ResponseType = 102;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly TransactionalSetMessageType TYPE = RequestType;
            public string name;
            public string txnId;
            public long threadId;

            public static int CalculateDataSize(string name, string txnId, long threadId)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(txnId);
                dataSize += Bits.LongSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, string txnId, long threadId)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, txnId, threadId);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(txnId);
            clientMessage.Set(threadId);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public int response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            int response ;
            response = clientMessage.GetInt();
            parameters.response = response;
            return parameters;
        }

    }
}
