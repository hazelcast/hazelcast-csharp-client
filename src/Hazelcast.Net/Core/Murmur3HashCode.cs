// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    /// <summary>
    /// Computes Murmur3 hash codes.
    /// </summary>
    /// <returns>
    /// <para>This is an x86 32bits implementation.</para>
    /// </returns>
    internal static class Murmur3HashCode
    {
        // ReSharper disable once InconsistentNaming
        private const int DefaultMurmurSeed = 0x01000193;

        /// <summary>
        /// Reserved for internal usage.
        /// </summary>
        /// <param name="k">Input value.</param>
        /// <returns>Output value.</returns>
        private static int MurmurHash3_fmix(int k)
        {
            k ^= (int)(((uint)k) >> 16);
            k *= unchecked((int)(0x85ebca6b));
            k ^= (int)(((uint)k) >> 13);
            k *= unchecked((int)(0xc2b2ae35));
            k ^= (int)(((uint)k) >> 16);
            return k;
        }

        /// <summary>
        /// Computes the hash code of an array of bytes.
        /// </summary>
        /// <param name="data">An array of bytes.</param>
        /// <param name="offset">The offset at which hashing begins.</param>
        /// <param name="count">The number of bytes to hash.</param>
        /// <param name="seed">An optional hash seed.</param>
        /// <returns>The hash code of the array of bytes.</returns>
        public static int Hash(byte[] data, int offset, int count, int seed = DefaultMurmurSeed)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset >= data.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(count));

            var c1 = unchecked((int)(0xcc9e2d51));
            var c2 = unchecked(0x1b873593);
            var h1 = seed;
            var roundedEnd = offset + (count & unchecked((int)(0xfffffffc)));
            // round down to 4 byte block
            for (var i = offset; i < roundedEnd; i += 4)
            {
                // little endian load order
                var k1 = (data[i] & unchecked(0xff)) |
                         ((data[i + 1] & unchecked(0xff)) << 8) |
                         ((data[i + 2] & unchecked(0xff)) << 16) |
                         (data[i + 3] << 24);
                k1 *= c1;
                k1 = (k1 << 15) | ((int)(((uint)k1) >> 17));
                // ROTL32(k1,15);
                k1 *= c2;
                h1 ^= k1;
                h1 = (h1 << 13) | ((int)(((uint)h1) >> 19));
                // ROTL32(h1,13);
                h1 = h1 * 5 + unchecked((int)(0xe6546b64));
            }
            // tail
            var k11 = 0;
            switch (count & unchecked(0x03))
            {
                case 3:
                    {
                        k11 = (data[roundedEnd + 2] & unchecked(0xff)) << 16;
                        goto case 2;
                    }

                case 2:
                    {
                        // fallthrough
                        k11 |= (data[roundedEnd + 1] & unchecked(0xff)) << 8;
                        goto case 1;
                    }

                case 1:
                    {
                        // fallthrough
                        k11 |= data[roundedEnd] & unchecked(0xff);
                        k11 *= c1;
                        k11 = (k11 << 15) | ((int)(((uint)k11) >> 17));
                        // ROTL32(k1,15);
                        k11 *= c2;
                        h1 ^= k11;
                        break;
                    }
            }
            // finalization
            h1 ^= count;
            h1 = MurmurHash3_fmix(h1);
            return h1;
        }
    }
}
