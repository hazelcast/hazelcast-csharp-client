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

using Hazelcast.Messaging;
using static Hazelcast.Protocol.Portability;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class LongArrayCodec
    {
        public static void Encode(ClientMessage clientMessage, long[] collection)
        {
            var itemCount = collection.Length;
            var frame = new Frame(new byte[itemCount * LongSizeInBytes]);

            for (var i = 0; i < collection.Length; i++)
            {
                EncodeLong(frame, i * LongSizeInBytes, collection[i]);
            }

            clientMessage.Add(frame);
        }

        public static long[] Decode(FrameIterator iterator)
        {
            return Decode(iterator.Take());
        }

        public static long[] Decode(Frame frame)
        {
            var itemCount = frame.Bytes.Length / LongSizeInBytes;
            var result = new long[itemCount];
            for (var i = 0; i < itemCount; i++)
            {
                result[i] = DecodeLong(frame, i * LongSizeInBytes);
            }
            return result;
        }
    }
}