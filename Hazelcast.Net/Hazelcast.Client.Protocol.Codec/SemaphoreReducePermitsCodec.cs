using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class SemaphoreReducePermitsCodec
    {

        public static readonly SemaphoreMessageType RequestType = SemaphoreMessageType.SemaphoreReducePermits;
        public const int ResponseType = 100;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly SemaphoreMessageType TYPE = RequestType;
            public string name;
            public int reduction;

            public static int CalculateDataSize(string name, int reduction)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, int reduction)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, reduction);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(reduction);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }
    }
}
