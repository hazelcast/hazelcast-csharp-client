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

using System;
using System.Collections.Generic;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.CodecUtil;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class MapCodec
    {
        public static void Encode<TKey, TValue>(ClientMessage clientMessage, IDictionary<TKey, TValue> map, Action<ClientMessage, TKey> encodeKey, Action<ClientMessage, TValue> encodeValue)
        {
            clientMessage.Add(BeginFrame);

            foreach (var kvp in map)
            {
                encodeKey(clientMessage, kvp.Key);
                encodeValue(clientMessage, kvp.Value);
            }

            clientMessage.Add(EndFrame);
        }

        public static void EncodeNullable<TKey, TValue>(ClientMessage clientMessage, IReadOnlyDictionary<TKey, TValue> map, Action<ClientMessage, TKey> encodeKey, Action<ClientMessage, TValue> encodeValue)
        {
            if (map == null)
            {
                clientMessage.Add(NullFrame);
            }
            else
            {
                Encode(clientMessage, map, encodeKey, encodeValue);
            }
        }

        public static IDictionary<TKey, TValue> Decode<TKey, TValue>(ref FrameIterator iterator, DecodeDelegate<TKey> decodeKey, DecodeDelegate<TValue> decodeValue)
        {
            var result = new Dictionary<TKey, TValue>();

            //begin frame, map
            iterator.Next();

            while (!IsNextFrameIsDataStructureEndFrame(ref iterator))
            {
                var key = decodeKey(ref iterator);
                var value = decodeValue(ref iterator);
                result[key] = value;
            }

            //end frame, map
            iterator.Next();
            return result;
        }

        public static IDictionary<TKey, TValue> DecodeNullable<TKey, TValue>(ref FrameIterator iterator, DecodeDelegate<TKey> decodeKey, DecodeDelegate<TValue> decodeValue)
        {
            return IsNextFrameIsNullEndFrame(ref iterator) ? null : Decode(ref iterator, decodeKey, decodeValue);
        }
    }
}