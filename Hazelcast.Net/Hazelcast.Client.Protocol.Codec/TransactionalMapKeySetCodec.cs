using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class TransactionalMapKeySetCodec
    {

        public static readonly TransactionalMapMessageType RequestType = TransactionalMapMessageType.TransactionalMapKeySet;
        public const int ResponseType = 113;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly TransactionalMapMessageType TYPE = RequestType;
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
            public ISet<IData> set;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            ISet<IData> set = null;
            int set_size = clientMessage.GetInt();
            set = new HashSet<IData>();
            for (int set_index = 0; set_index<set_size; set_index++) {
                IData set_item;
            set_item = clientMessage.GetData();
                set.Add(set_item);
            }
            parameters.set = set;
            return parameters;
        }

    }
}
