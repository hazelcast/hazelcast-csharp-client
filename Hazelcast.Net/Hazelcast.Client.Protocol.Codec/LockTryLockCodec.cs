using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class LockTryLockCodec
    {

        public static readonly LockMessageType RequestType = LockMessageType.LockTryLock;
        public const int ResponseType = 101;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly LockMessageType TYPE = RequestType;
            public string name;
            public long threadId;
            public long lease;
            public long timeout;

            public static int CalculateDataSize(string name, long threadId, long lease, long timeout)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.LongSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, long threadId, long lease, long timeout)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, threadId, lease, timeout);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(threadId);
            clientMessage.Set(lease);
            clientMessage.Set(timeout);
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
