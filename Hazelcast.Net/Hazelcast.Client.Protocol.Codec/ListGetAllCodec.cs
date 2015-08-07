using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ListGetAllCodec
    {

        public static readonly ListMessageType RequestType = ListMessageType.ListGetAll;
        public const int ResponseType = 106;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ListMessageType TYPE = RequestType;
            public string name;

            public static int CalculateDataSize(string name)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
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
            list_item = clientMessage.GetData();
                list.Add(list_item);
            }
            parameters.list = list;
            return parameters;
        }

    }
}
