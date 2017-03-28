// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

// Client Protocol version, Since:1.0 - Update:1.0

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class RingbufferReadManyCodec
    {
        public static readonly RingbufferMessageType RequestType = RingbufferMessageType.RingbufferReadMany;
        public const int ResponseType = 115;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly RingbufferMessageType TYPE = RequestType;
            public string name;
            public long startSequence;
            public int minCount;
            public int maxCount;
            public IData filter;

            public static int CalculateDataSize(string name, long startSequence, int minCount, int maxCount,
                IData filter)
            {
                var dataSize = ClientMessage.HeaderSize;
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

        public static ClientMessage EncodeRequest(string name, long startSequence, int minCount, int maxCount,
            IData filter)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, startSequence, minCount, maxCount, filter);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(startSequence);
            clientMessage.Set(minCount);
            clientMessage.Set(maxCount);
            clientMessage.Set(filter == null);
            if (filter != null)
            {
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
            var parameters = new ResponseParameters();
            var readCount = clientMessage.GetInt();
            parameters.readCount = readCount;
            var items = new List<IData>();
            var itemsSize = clientMessage.GetInt();
            for (var itemsIndex = 0; itemsIndex < itemsSize; itemsIndex++)
            {
                var itemsItem = clientMessage.GetData();
                items.Add(itemsItem);
            }
            parameters.items = items;
            return parameters;
        }
    }
}