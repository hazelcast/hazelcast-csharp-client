using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class TransactionCreateCodec
    {

        public static readonly TransactionMessageType RequestType = TransactionMessageType.TransactionCreate;
        public const int ResponseType = 104;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly TransactionMessageType TYPE = RequestType;
            public long timeout;
            public int durability;
            public int transactionType;
            public long threadId;

            public static int CalculateDataSize(long timeout, int durability, int transactionType, long threadId)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(long timeout, int durability, int transactionType, long threadId)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(timeout, durability, transactionType, threadId);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(timeout);
            clientMessage.Set(durability);
            clientMessage.Set(transactionType);
            clientMessage.Set(threadId);
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

    }
}
