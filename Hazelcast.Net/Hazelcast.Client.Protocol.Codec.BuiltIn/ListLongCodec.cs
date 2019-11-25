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

using System.Collections.Generic;
using System.Linq;
using Hazelcast.IO;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class ListLongCodec
    {
        public static void Encode(ClientMessage clientMessage, IEnumerable<long> collection)
        {
            var itemCount = collection.Count();
            var frame = new ClientMessage.Frame(new byte[itemCount * Bits.LongSizeInBytes]);

            var i = 0;
            foreach (var value in collection)
            {
                FixedSizeTypesCodec.EncodeLong(frame.Content, i * Bits.LongSizeInBytes, value);
                i++;
            }

            clientMessage.Add(frame);
        }

        public static List<long> Decode(ClientMessage.FrameIterator iterator)
        {
            return Decode(iterator.Next());
        }

        public static List<long> Decode(ClientMessage.Frame frame)
        {
            var itemCount = frame.Content == null ? 0 : frame.Content.Length / Bits.LongSizeInBytes;
            var result = new List<long>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                result.Add(FixedSizeTypesCodec.DecodeLong(frame.Content, i * Bits.LongSizeInBytes));
            }
            return result;
        }
    }
}