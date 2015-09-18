using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ListSetCodec
    {

        public static readonly ListMessageType RequestType = ListMessageType.ListSet;
        public const int ResponseType = 105;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ListMessageType TYPE = RequestType;
            public string name;
            public int index;
            public IData value;

            public static int CalculateDataSize(string name, int index, IData value)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                dataSize += ParameterUtil.CalculateDataSize(value);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, int index, IData value)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, index, value);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(index);
            clientMessage.Set(value);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public IData response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            IData response = null;
            bool response_isNull = clientMessage.GetBoolean();
            if (!response_isNull)
            {
            response = clientMessage.GetData();
            parameters.response = response;
            }
            return parameters;
        }

    }
}
