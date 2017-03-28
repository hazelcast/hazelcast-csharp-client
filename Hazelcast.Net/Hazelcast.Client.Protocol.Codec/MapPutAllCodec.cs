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
    internal sealed class MapPutAllCodec
    {
        public static readonly MapMessageType RequestType = MapMessageType.MapPutAll;
        public const int ResponseType = 100;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public IList<KeyValuePair<IData, IData>> entries;

            public static int CalculateDataSize(string name, IList<KeyValuePair<IData, IData>> entries)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                foreach (var entriesItem in entries)
                {
                    var entriesItemKey = entriesItem.Key;
                    var entriesItemVal = entriesItem.Value;
                    dataSize += ParameterUtil.CalculateDataSize(entriesItemKey);
                    dataSize += ParameterUtil.CalculateDataSize(entriesItemVal);
                }
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, IList<KeyValuePair<IData, IData>> entries)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, entries);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(entries.Count);
            foreach (var entriesItem in entries)
            {
                var entriesItemKey = entriesItem.Key;
                var entriesItemVal = entriesItem.Value;
                clientMessage.Set(entriesItemKey);
                clientMessage.Set(entriesItemVal);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE IS EMPTY *****************//
    }
}