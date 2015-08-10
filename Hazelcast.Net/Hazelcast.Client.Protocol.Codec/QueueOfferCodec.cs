using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class QueueOfferCodec
    {

        public static readonly QueueMessageType RequestType = QueueMessageType.QueueOffer;
        public const int ResponseType = 101;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly QueueMessageType TYPE = RequestType;
            public string name;
            public IData value;
            public long timeoutMillis;

            public static int CalculateDataSize(string name, IData value, long timeoutMillis)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(value);
                dataSize += Bits.LongSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IData value, long timeoutMillis)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, value, timeoutMillis);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(value);
            clientMessage.Set(timeoutMillis);
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
