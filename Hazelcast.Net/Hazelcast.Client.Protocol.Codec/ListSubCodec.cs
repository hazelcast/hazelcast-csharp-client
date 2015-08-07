using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ListSubCodec
    {

        public static readonly ListMessageType RequestType = ListMessageType.ListSub;
        public const int ResponseType = 106;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ListMessageType TYPE = RequestType;
            public string name;
            public int from;
            public int to;

            public static int CalculateDataSize(string name, int from, int to)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, int from, int to)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, from, to);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(from);
            clientMessage.Set(to);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public IList<IData> list;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            IList<IData> list = null;
            int list_size = clientMessage.GetInt();
            list = new List<IData>();
            for (int list_index = 0; list_index<list_size; list_index++) {
                IData list_item;
            list_item = clientMessage.GetIData();
                list.Add(list_item);
            }
            parameters.list = list;
            return parameters;
        }

    }
}
