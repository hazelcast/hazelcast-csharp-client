using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapAddIndexCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapAddIndex;
        public const int ResponseType = 100;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public string attribute;
            public bool ordered;

            public static int CalculateDataSize(string name, string attribute, bool ordered)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(attribute);
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, string attribute, bool ordered)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, attribute, ordered);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(attribute);
            clientMessage.Set(ordered);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }
    }
}
