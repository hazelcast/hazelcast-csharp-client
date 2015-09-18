using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MultiMapForceUnlockCodec
    {

        public static readonly MultiMapMessageType RequestType = MultiMapMessageType.MultiMapForceUnlock;
        public const int ResponseType = 100;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MultiMapMessageType TYPE = RequestType;
            public string name;
            public IData key;

            public static int CalculateDataSize(string name, IData key)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(key);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IData key)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, key);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(key);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }
    }
}
