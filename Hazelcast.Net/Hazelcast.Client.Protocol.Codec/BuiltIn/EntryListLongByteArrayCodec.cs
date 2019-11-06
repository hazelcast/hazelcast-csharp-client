// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using static Hazelcast.Client.Protocol.ClientMessage;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class EntryListLongByteArrayCodec
    {
        public static void Encode(ClientMessage clientMessage, IEnumerable<KeyValuePair<long, byte[]>> collection)
        {
            var valueList = new List<long>();
            clientMessage.Add(BeginFrame);

            foreach (var kvp in collection)
            {
                valueList.Add(kvp.Key);
                ByteArrayCodec.Encode(clientMessage, kvp.Value);
            }

            clientMessage.Add(EndFrame);
            ListLongCodec.Encode(clientMessage, valueList);
        }

        public static IList<KeyValuePair<long, byte[]>> Decode(ref FrameIterator iterator)
        {
            var listV = ListMultiFrameCodec.Decode(ref iterator, ByteArrayCodec.Decode);
            var listK = ListLongCodec.Decode(ref iterator);

            var result = new List<KeyValuePair<long, byte[]>>(listV.Count);
            for (var i = 0; i < listK.Count; i++)
            {
                result.Add(new KeyValuePair<long, byte[]>(listK[i], listV[i]));
            }

            return result;
        }
    }
}