using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapRemoveInterceptorCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapRemoveInterceptor;
        public const int ResponseType = 101;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public string id;

            public static int CalculateDataSize(string name, string id)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(id);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, string id)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, id);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(id);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }
    }
}
