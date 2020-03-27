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

using System.Buffers;

namespace AsyncTests1
{
    public static class Extensions
    {
        public static int ReadInt32(this ReadOnlySequence<byte> buffer)
        {
            var value = 0;
            var e = buffer.GetEnumerator();
            var j = 0;
            while (j < 4)
            {
                e.MoveNext();
                var m = e.Current;
                var k = 0;
                var l = m.Span.Length;
                while (k < l && j < 4)
                {
                    value <<= 8;
                    value |= m.Span[k++];
                    j++;
                }
            }

            return value;
        }
    }
}