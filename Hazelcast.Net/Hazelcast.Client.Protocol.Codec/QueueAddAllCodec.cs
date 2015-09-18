using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class QueueAddAllCodec
    {

        public static readonly QueueMessageType RequestType = QueueMessageType.QueueAddAll;
        public const int ResponseType = 101;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly QueueMessageType TYPE = RequestType;
            public string name;
            public ISet<IData> dataList;

            public static int CalculateDataSize(string name, ISet<IData> dataList)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                foreach (var dataList_item in dataList )
                {
                dataSize += ParameterUtil.CalculateDataSize(dataList_item);
                }
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, ISet<IData> dataList)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, dataList);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(dataList.Count);
            foreach (var dataList_item in dataList) {
            clientMessage.Set(dataList_item);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public bool response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            bool response ;
            response = clientMessage.GetBoolean();
            parameters.response = response;
            return parameters;
        }

    }
}
