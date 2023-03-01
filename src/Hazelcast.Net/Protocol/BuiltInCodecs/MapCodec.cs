// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class MapCodec
    {
        public static void Encode<TKey, TValue>(ClientMessage clientMessage, IDictionary<TKey, TValue> map, Action<ClientMessage, TKey> encodeKey, Action<ClientMessage, TValue> encodeValue)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            foreach (var (key, value) in map)
            {
                encodeKey(clientMessage, key);
                encodeValue(clientMessage, value);
            }

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static void Encode<TKey, TValue>(ClientMessage clientMessage, IReadOnlyDictionary<TKey, TValue> map, Action<ClientMessage, TKey> encodeKey, Action<ClientMessage, TValue> encodeValue)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            foreach (var (key, value) in map)
            {
                encodeKey(clientMessage, key);
                encodeValue(clientMessage, value);
            }

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static void EncodeNullable<TKey, TValue>(ClientMessage clientMessage, IDictionary<TKey, TValue> map, Action<ClientMessage, TKey> encodeKey, Action<ClientMessage, TValue> encodeValue)
        {
            if (map == null)
            {
                clientMessage.Append(Frame.CreateNull());
            }
            else
            {
                Encode(clientMessage, map, encodeKey, encodeValue);
            }
        }

        public static Dictionary<TKey, TValue> Decode<TKey, TValue>(IEnumerator<Frame> iterator, DecodeDelegate<TKey> decodeKey, DecodeDelegate<TValue> decodeValue)
        {
            var result = new Dictionary<TKey, TValue>();

            //begin frame, map
            iterator.Take();

            while (!iterator.AtStructEnd())
            {
                var key = decodeKey(iterator);
                var value = decodeValue(iterator);
                result[key] = value;
            }

            //end frame, map
            iterator.Take();
            return result;
        }

        public static Dictionary<TKey, TValue> DecodeNullable<TKey, TValue>(IEnumerator<Frame> iterator, DecodeDelegate<TKey> decodeKey, DecodeDelegate<TValue> decodeValue)
        {
            return iterator.SkipNull() ? null : Decode(iterator, decodeKey, decodeValue);
        }
    }
}
