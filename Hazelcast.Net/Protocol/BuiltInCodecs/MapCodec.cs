// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Messaging;
using Hazelcast.Protocol.Portability;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class MapCodec
    {
        public static void Encode<TKey, TValue>(ClientMessage clientMessage, IDictionary<TKey, TValue> map, Action<ClientMessage, TKey> encodeKey, Action<ClientMessage, TValue> encodeValue)
        {
            clientMessage.Add(Frame.CreateBeginStruct());

            foreach (var kvp in map)
            {
                encodeKey(clientMessage, kvp.Key);
                encodeValue(clientMessage, kvp.Value);
            }

            clientMessage.Add(Frame.CreateEndStruct());
        }

        public static void EncodeNullable<TKey, TValue>(ClientMessage clientMessage, IDictionary<TKey, TValue> map, Action<ClientMessage, TKey> encodeKey, Action<ClientMessage, TValue> encodeValue)
        {
            if (map == null)
            {
                clientMessage.Add(Frame.CreateNull());
            }
            else
            {
                Encode(clientMessage, map, encodeKey, encodeValue);
            }
        }

        public static IDictionary<TKey, TValue> Decode<TKey, TValue>(FrameIterator iterator, DecodeDelegate<TKey> decodeKey, DecodeDelegate<TValue> decodeValue)
        {
            var result = new Dictionary<TKey, TValue>();

            //begin frame, map
            iterator.Next();

            while (!iterator.NextFrameIsEndStruct)
            {
                var key = decodeKey(iterator);
                var value = decodeValue(iterator);
                result[key] = value;
            }

            //end frame, map
            iterator.Next();
            return result;
        }

        public static IDictionary<TKey, TValue> DecodeNullable<TKey, TValue>(FrameIterator iterator, DecodeDelegate<TKey> decodeKey, DecodeDelegate<TValue> decodeValue)
        {
            return iterator.NextFrameIsNullMoveNext() ? null : Decode(iterator, decodeKey, decodeValue);
        }
    }
}