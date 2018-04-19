// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

// Client Protocol version, Since:1.0 - Update:1.6
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class RingbufferReadManyCodec
    {
        private static int CalculateRequestDataSize(string name, long startSequence, int minCount, int maxCount, IData filter)
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

        internal static ClientMessage EncodeRequest(string name, long startSequence, int minCount, int maxCount, IData filter)
        {
            var requiredDataSize = CalculateRequestDataSize(name, startSequence, minCount, maxCount, filter);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RingbufferMessageType.RingbufferReadMany);
            clientMessage.SetRetryable(true);
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

        internal class ResponseParameters
        {
            public int readCount;
            public IList<IData> items;
            public long[] itemSeqs;
            public bool itemSeqsExist;
            public long nextSeq;
            public bool nextSeqExist;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var readCount = clientMessage.GetInt();
            parameters.readCount = readCount;
            var itemsSize = clientMessage.GetInt();
            var items = new List<IData>(itemsSize);
            for (var itemsIndex = 0; itemsIndex < itemsSize; itemsIndex++)
            {
                var itemsItem = clientMessage.GetData();
                items.Add(itemsItem);
            }
            parameters.items = items;
            if (clientMessage.IsComplete())
            {
                return parameters;
            }
            var itemSeqsIsNull = clientMessage.GetBoolean();
            if (!itemSeqsIsNull)
            {
                var itemSeqsSize = clientMessage.GetInt();
                var itemSeqs = new long[itemSeqsSize];
                for (var itemSeqsIndex = 0; itemSeqsIndex < itemSeqsSize; itemSeqsIndex++)
                {
                    var itemSeqsItem = clientMessage.GetLong();
                    itemSeqs[itemSeqsIndex] = itemSeqsItem;
                }
                parameters.itemSeqs = itemSeqs;
            }
            parameters.itemSeqsExist = true;
            if (clientMessage.IsComplete())
            {
                return parameters;
            }
            var nextSeq = clientMessage.GetLong();
            parameters.nextSeq = nextSeq;
            parameters.nextSeqExist = true;
            return parameters;
        }
    }
}