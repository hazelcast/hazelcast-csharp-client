using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapLoadGivenKeysCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapLoadGivenKeys;
        public const int ResponseType = 100;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public ISet<IData> keys;
            public bool replaceExistingValues;

            public static int CalculateDataSize(string name, ISet<IData> keys, bool replaceExistingValues)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                foreach (var keys_item in keys )
                {
                dataSize += ParameterUtil.CalculateDataSize(keys_item);
                }
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, ISet<IData> keys, bool replaceExistingValues)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, keys, replaceExistingValues);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(keys.Count);
            foreach (var keys_item in keys) {
            clientMessage.Set(keys_item);
            }
            clientMessage.Set(replaceExistingValues);
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
