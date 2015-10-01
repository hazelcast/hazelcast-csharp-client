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
            public Address target;

            public static int CalculateDataSize(string name, string serviceName, Address target)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(serviceName);
                dataSize += AddressCodec.CalculateDataSize(target);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, string serviceName, Address target)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, serviceName, target);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(serviceName);
            AddressCodec.Encode(target, clientMessage);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            return parameters;
        }

    }
}
