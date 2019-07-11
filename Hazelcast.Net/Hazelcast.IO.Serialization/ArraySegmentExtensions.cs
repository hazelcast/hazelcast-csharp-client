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

namespace Hazelcast.IO.Serialization
{
    public static class ArraySegmentExtensions
    {
        public static ArraySegment<byte> Slice(this ArraySegment<byte> segment, int offset)
        {
            return new ArraySegment<byte>(segment.Array, segment.Offset + offset, segment.Count - offset);
        }

        public static bool IsEqual(this ArraySegment<byte> segment, ArraySegment<byte> other)
        {
            if (segment.Count != other.Count)
                return false;

            var a1 = segment.Array;
            var offset1 = segment.Offset;

            var a2 = other.Array;
            var offset2 = other.Offset;

            for (var i = 0; i < segment.Count; i++)
            {
                if (a1[i + offset1] != a2[i + offset2])
                {
                    return false;
                }
            }

            return true;
        }
    }
}