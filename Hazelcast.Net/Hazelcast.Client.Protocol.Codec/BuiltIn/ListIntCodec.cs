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
using System.Linq;
using static Hazelcast.IO.Bits;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class ListIntCodec
    {
        public static void Encode(ClientMessage clientMessage, IEnumerable<int> collection)
        {
            var itemCount = collection.Count();
            var frame = new Frame(new byte[itemCount * IntSizeInBytes]);

            var i = 0;
            foreach (var value in collection)
            {
                EncodeInt(frame.Content, i* IntSizeInBytes, value);
                i++;
            }

            clientMessage.Add(frame);
        }

        public static IEnumerable<int> Decode(ref FrameIterator iterator)
        {
            return Decode(ref iterator.Next());
        }

        public static IEnumerable<int> Decode(ref Frame frame)
        {
            var itemCount = frame.Content == null ? 0 : frame.Content.Length / IntSizeInBytes;
            var result = new List<int>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                result.Add(DecodeInt(frame.Content, i * IntSizeInBytes));
            }
            return result;
        }
    }
}