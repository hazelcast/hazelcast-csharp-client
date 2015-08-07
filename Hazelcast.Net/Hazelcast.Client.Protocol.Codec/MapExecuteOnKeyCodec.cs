using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapExecuteOnKeyCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapExecuteOnKey;
        public const int ResponseType = 105;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public IData entryProcessor;
            public IData key;

            public static int CalculateDataSize(string name, IData entryProcessor, IData key)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(entryProcessor);
                dataSize += ParameterUtil.CalculateDataSize(key);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IData entryProcessor, IData key)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, entryProcessor, key);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(entryProcessor);
            clientMessage.Set(key);
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
