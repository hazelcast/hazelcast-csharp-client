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
using System.Buffers;

namespace AsyncTests1.Networking
{
    /// <summary>
    /// Provides extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Reads an <see cref="Int32"/> from a buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>An <see cref="Int32"/>.</returns>
        public static int ReadInt32(this ReadOnlySequence<byte> buffer)
        {
            var value = 0;
            var bufferEnumerator = buffer.GetEnumerator();
            var byteCount = 0;
            while (byteCount < 4)
            {
                bufferEnumerator.MoveNext();
                var m = bufferEnumerator.Current;
                var spanIndex = 0;
                var spanLength = m.Span.Length;
                while (spanIndex < spanLength && byteCount < 4)
                {
                    value <<= 8;
                    value |= m.Span[spanIndex++];
                    byteCount++;
                }
            }

            return value;
        }
    }
}