using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class TransactionalMultiMapRemoveCodec
    {

        public static readonly TransactionalMultiMapMessageType RequestType = TransactionalMultiMapMessageType.TransactionalMultiMapRemove;
        public const int ResponseType = 106;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly TransactionalMultiMapMessageType TYPE = RequestType;
            public string name;
            public string txnId;
            public long threadId;
            public IData key;

            public static int CalculateDataSize(string name, string txnId, long threadId, IData key)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(txnId);
                dataSize += Bits.LongSizeInBytes;
                dataSize += ParameterUtil.CalculateDataSize(key);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, string txnId, long threadId, IData key)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, txnId, threadId, key);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(txnId);
            clientMessage.Set(threadId);
            clientMessage.Set(key);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public IList<IData> list;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            IList<IData> list = null;
            int list_size = clientMessage.GetInt();
            list = new List<IData>();
            for (int list_index = 0; list_index<list_size; list_index++) {
                IData list_item;
            list_item = clientMessage.GetData();
                list.Add(list_item);
            }
            parameters.list = list;
            return parameters;
        }

    }
}
