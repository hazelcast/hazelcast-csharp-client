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
    internal static class EntryListCodec
    {
        public static void Encode<TKey, TValue>(ClientMessage clientMessage, IEnumerable<KeyValuePair<TKey, TValue>> collection,
                                         Action<ClientMessage, TKey> encodeKeyFunc,
                                         Action<ClientMessage, TValue> encodeValueFunc)
        {
            clientMessage.Add(BeginFrame);

            foreach (var kvp in collection)
            {
                encodeKeyFunc(clientMessage, kvp.Key);
                encodeValueFunc(clientMessage, kvp.Value);
            }

            clientMessage.Add(EndFrame);
        }

        public static void EncodeNullable<TKey, TValue>(ClientMessage clientMessage, IEnumerable<KeyValuePair<TKey, TValue>> collection,
                Action<ClientMessage, TKey> encodeKeyFunc,
                Action<ClientMessage, TValue> encodeValueFunc)
        { 
            if (collection == null)
            {
                clientMessage.Add(NullFrame);
            }
            else
            {
                Encode(clientMessage, collection, encodeKeyFunc, encodeValueFunc);
            }
        }

        public static  IEnumerable<KeyValuePair<TKey, TValue>> Decode<TKey, TValue>(ref FrameIterator iterator,
                                                          DecodeDelegate<TKey> decodeKeyFunc,
                                                          DecodeDelegate<TValue> decodeValueFunc)
        {
            var result = new List<KeyValuePair<TKey,TValue>>();
            
            //begin frame, map
            iterator.Next();
            while (!IsNextFrameIsDataStructureEndFrame(ref iterator))
            {
                var key = decodeKeyFunc(ref iterator);
                var value = decodeValueFunc(ref iterator);
                result.Add(new KeyValuePair<TKey, TValue>(key, value));
            }
            //end frame, map
            iterator.Next();
            return result;
        }

        public static IEnumerable<KeyValuePair<TKey,TValue>> DecodeNullable<TKey,TValue>(ref FrameIterator iterator, 
            DecodeDelegate<TKey> decodeKeyFunc, DecodeDelegate<TValue> decodeValueFunc)
        {
            return IsNextFrameIsNullEndFrame(ref iterator) ? null : Decode(ref iterator, decodeKeyFunc, decodeValueFunc);
        }
    }
}