/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class RingbufferReadManyAsyncCodec
    {

        public static readonly RingbufferMessageType RequestType = RingbufferMessageType.RingbufferReadManyAsync;
        public const int ResponseType = 115;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly RingbufferMessageType TYPE = RequestType;
            public string name;
            public long startSequence;
            public int minCount;
            public int maxCount;
            public IData filter;

            public static int CalculateDataSize(string name, long startSequence, int minCount, int maxCount, IData filter)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.LongSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.BooleanSizeInBytes;
                if (filter != null)
                {
                dataSize += ParameterUtil.CalculateDataSize(filter);
                }
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, long startSequence, int minCount, int maxCount, IData filter)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(name, startSequence, minCount, maxCount, filter);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(startSequence);
            clientMessage.Set(minCount);
            clientMessage.Set(maxCount);
            bool filter_isNull;
            if (filter == null)
            {
                filter_isNull = true;
                clientMessage.Set(filter_isNull);
            }
            else
            {
                filter_isNull= false;
                clientMessage.Set(filter_isNull);
            clientMessage.Set(filter);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public int readCount;
            public IList<IData> items;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            int readCount ;
            readCount = clientMessage.GetInt();
            parameters.readCount = readCount;
            IList<IData> items = null;
            int items_size = clientMessage.GetInt();
            items = new List<IData>();
            for (int items_index = 0; items_index<items_size; items_index++) {
                IData items_item;
            items_item = clientMessage.GetData();
                items.Add(items_item);
            }
            parameters.items = items;
            return parameters;
        }

    }
}
