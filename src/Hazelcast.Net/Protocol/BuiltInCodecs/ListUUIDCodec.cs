// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class ListUUIDCodec
    {
        public static void Encode(ClientMessage clientMessage, IEnumerable<Guid> collection)
        {
            var itemCount = collection.Count();
            var frame = new Frame(new byte[itemCount * BytesExtensions.SizeOfGuid]);

            var i = 0;
            foreach (var guid in collection)
            {
                frame.Bytes.WriteGuidL(i * BytesExtensions.SizeOfGuid, guid);
                i++;
            }

            clientMessage.Append(frame);
        }

        public static IList<Guid> Decode(IEnumerator<Frame> iterator)
        {
            return Decode(iterator.Take());
        }

        public static List<Guid> Decode(Frame frame)
        {
            var itemCount = frame.Bytes.Length / BytesExtensions.SizeOfGuid;
            var result = new List<Guid>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                result.Add(frame.Bytes.ReadGuidL(i * BytesExtensions.SizeOfGuid));
            }
            return result;
        }
    }
}
