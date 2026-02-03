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
using System.Text;

namespace Hazelcast.Polyfills
{
    internal static class EncodingExtensions
    {
#if NETSTANDARD2_0
        public static unsafe int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            fixed (char* c = chars)
            fixed (byte* b = bytes)
            {
                return encoding.GetBytes(c, chars.Length, b, bytes.Length);
            }
        }

        public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
        {
            fixed (byte* b = bytes)
            {
                return encoding.GetString(b, bytes.Length);
            }
        }

        public static unsafe void Convert(
            this Encoder encoder,
            ReadOnlySpan<char> chars,
            Span<byte> bytes,
            bool flush,
            out int charsUsed,
            out int bytesUsed,
            out bool completed)
        {
            fixed (char* c = chars)
            fixed (byte* b = bytes)
            {
                encoder.Convert(c, chars.Length, b, bytes.Length, flush, out charsUsed, out bytesUsed, out completed);
            }
        }
#endif
    }
}
