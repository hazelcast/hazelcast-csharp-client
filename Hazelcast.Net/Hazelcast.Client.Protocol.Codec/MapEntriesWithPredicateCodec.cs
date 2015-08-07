using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapEntriesWithPredicateCodec
    {

        public static readonly MapMessageType RequestType = MapMessageType.MapEntriesWithPredicate;
        public const int ResponseType = 114;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public IData predicate;

            public static int CalculateDataSize(string name, IData predicate)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += ParameterUtil.CalculateDataSize(predicate);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IData predicate)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, predicate);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(predicate);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public ISet<KeyValuePair<IData,IData>> entrySet;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            ISet<KeyValuePair<IData,IData>> entrySet = null;
            int entrySet_size = clientMessage.GetInt();
            entrySet = new HashSet<KeyValuePair<IData,IData>>();
            for (int entrySet_index = 0; entrySet_index<entrySet_size; entrySet_index++) {
                KeyValuePair<IData,IData> entrySet_item;
            entrySet_item = clientMessage.GetMapEntry();
                entrySet.Add(entrySet_item);
            }
            parameters.entrySet = entrySet;
            return parameters;
        }

    }
}
