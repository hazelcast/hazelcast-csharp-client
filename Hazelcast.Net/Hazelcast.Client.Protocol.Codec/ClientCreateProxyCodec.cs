using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientCreateProxyCodec
    {

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientCreateProxy;
        public const int ResponseType = 100;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;
            public string name;
            public string serviceName;

            public static int CalculateDataSize(string name, string serviceName)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(serviceName);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, string serviceName)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, serviceName);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(serviceName);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }
    }
}
