using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class RingbufferAddAllAsyncCodec
    {

        public static readonly RingbufferMessageType RequestType = RingbufferMessageType.RingbufferAddAllAsync;
        public const int ResponseType = 103;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly RingbufferMessageType TYPE = RequestType;
            public string name;
            public IList<IData> valueList;
            public int overflowPolicy;

            public static int CalculateDataSize(string name, IList<IData> valueList, int overflowPolicy)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                foreach (var valueList_item in valueList )
                {
                dataSize += ParameterUtil.CalculateDataSize(valueList_item);
                }
                dataSize += Bits.IntSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IList<IData> valueList, int overflowPolicy)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, valueList, overflowPolicy);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(valueList.Count);
            foreach (var valueList_item in valueList) {
            clientMessage.Set(valueList_item);
            }
            clientMessage.Set(overflowPolicy);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public long response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            long response ;
            response = clientMessage.GetLong();
            parameters.response = response;
            return parameters;
        }

    }
}
