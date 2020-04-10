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
    internal static class ListMultiFrameCodec
    {
        public static void Encode<T>(ClientMessage clientMessage, IEnumerable<T> collection, Action<ClientMessage, T> encodeFunction)
        {
            clientMessage.Add(Frame.CreateBeginStruct());

            foreach (var item in collection)
            {
                encodeFunction(clientMessage, item);
            }

            clientMessage.Add(Frame.CreateEndStruct());
        }

        public static void EncodeContainsNullable<T>(ClientMessage clientMessage, IEnumerable<T> collection, Action<ClientMessage, T> encodeFunction)
        {
            clientMessage.Add(Frame.CreateBeginStruct());

            foreach (var item in collection)
            {
                if (item == null)
                {
                    clientMessage.Add(Frame.CreateNull());
                }
                else
                {
                    encodeFunction(clientMessage, item);
                }
            }

            clientMessage.Add(Frame.CreateEndStruct());
        }

        public static void EncodeNullable<T>(ClientMessage clientMessage, IEnumerable<T> collection, Action<ClientMessage, T> encodeFunction)
        {
            if (collection == null)
            {
                clientMessage.Add(Frame.CreateNull());
            }
            else
            {
                Encode(clientMessage, collection, encodeFunction);
            }
        }

        public static List<T> Decode<T>(FrameIterator iterator, DecodeDelegate<T> decodeFunction)
        {
            var result = new List<T>();
            //begin frame, list
            iterator.Next();
            while (!iterator.NextFrameIsNullMoveNext())
            {
                result.Add(decodeFunction(iterator));
            }

            //end frame, list
            iterator.Next();
            return result;
        }

        public static List<T> DecodeContainsNullable<T>(FrameIterator iterator, DecodeDelegate<T> decodeFunction) where T : class
        {
            var result = new List<T>();
            //begin frame, list
            iterator.Next();
            while (!iterator.NextFrameIsEndStruct)
            {
                result.Add(iterator.NextFrameIsNullMoveNext() ? null : decodeFunction(iterator));
            }

            //end frame, list
            iterator.Next();
            return result;
        }

        public static List<T> DecodeNullable<T>(ref FrameIterator iterator, DecodeDelegate<T> decodeFunction)
        {
            return iterator.NextFrameIsNullMoveNext() ? null : Decode(iterator, decodeFunction);
        }
    }
}