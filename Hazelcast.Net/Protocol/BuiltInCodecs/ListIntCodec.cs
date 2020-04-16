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

using System.Collections.Generic;
using Hazelcast.Messaging;
using static Hazelcast.Protocol.Portability;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class ListIntegerCodec
    {
        public static void Encode(ClientMessage clientMessage, ICollection<int> collection)
        {
            var itemCount = collection.Count;
            var frame = new Frame(new byte[itemCount * IntSizeInBytes]);

            var i = 0;
            foreach (var value in collection)
            {
                EncodeInt(frame, i* IntSizeInBytes, value);
                i++;
            }

            clientMessage.Add(frame);
        }

        public static IList<int> Decode(FrameIterator iterator)
        {
            return Decode(iterator.Take());
        }

        public static IList<int> Decode(Frame frame)
        {
            // fixme can frame.Bytes even be null?
            var itemCount = frame.Bytes == null ? 0 : frame.Bytes.Length / IntSizeInBytes;
            var result = new List<int>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                result.Add(DecodeInt(frame, i * IntSizeInBytes));
            }
            return result;
        }
    }
}