using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class SetCompareAndRetainAllCodec
    {

        public static readonly SetMessageType RequestType = SetMessageType.SetCompareAndRetainAll;
        public const int ResponseType = 101;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly SetMessageType TYPE = RequestType;
            public string name;
            public ISet<IData> valueSet;

            public static int CalculateDataSize(string name, ISet<IData> valueSet)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                foreach (var valueSet_item in valueSet )
                {
                dataSize += ParameterUtil.CalculateDataSize(valueSet_item);
                }
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, ISet<IData> valueSet)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, valueSet);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(valueSet.Count);
            foreach (var valueSet_item in valueSet) {
            clientMessage.Set(valueSet_item);
            }
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
