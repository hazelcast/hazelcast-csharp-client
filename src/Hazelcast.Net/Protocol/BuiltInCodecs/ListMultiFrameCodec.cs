// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
    internal static class ListMultiFrameCodec
    {
        public static void Encode<T>(ClientMessage clientMessage, IEnumerable<T> collection, Action<ClientMessage, T> encodeFunction)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            foreach (var item in collection)
            {
                encodeFunction(clientMessage, item);
            }

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static void EncodeContainsNullable<T>(ClientMessage clientMessage, IEnumerable<T> collection, Action<ClientMessage, T> encodeFunction)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            foreach (var item in collection)
            {
#pragma warning disable CA1508 // Avoid dead conditional code - false positive, yes it can be null
                if (item == null)
#pragma warning restore CA1508
                {
                    clientMessage.Append(Frame.CreateNull());
                }
                else
                {
                    encodeFunction(clientMessage, item);
                }
            }

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static void EncodeNullable<T>(ClientMessage clientMessage, IEnumerable<T> collection, Action<ClientMessage, T> encodeFunction)
        {
            if (collection == null)
            {
                clientMessage.Append(Frame.CreateNull());
            }
            else
            {
                Encode(clientMessage, collection, encodeFunction);
            }
        }

        public static List<T> Decode<T>(IEnumerator<Frame> iterator, DecodeDelegate<T> decodeFunction)
        {
            var result = new List<T>();
            //begin frame, list
            iterator.Take();
            while (!iterator.AtStructEnd())
            {
                result.Add(decodeFunction(iterator));
            }

            //end frame, list
            iterator.Take();
            return result;
        }

        public static List<T> DecodeContainsNullable<T>(IEnumerator<Frame> iterator, DecodeDelegate<T> decodeFunction) where T: class
        {
            var result = new List<T>();
            //begin frame, list
            iterator.Take();
            while (!iterator.AtStructEnd())
            {
                result.Add(iterator.SkipNull() ? null : decodeFunction(iterator));
            }

            //end frame, list
            iterator.Take();
            return result;
        }

        public static List<T> DecodeNullable<T>(IEnumerator<Frame> iterator, DecodeDelegate<T> decodeFunction)
        {
            return iterator.SkipNull() ? null : Decode(iterator, decodeFunction);
        }
    }
}
