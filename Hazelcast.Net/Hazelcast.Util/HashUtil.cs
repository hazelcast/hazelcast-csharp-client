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

namespace Hazelcast.Util
{
    internal sealed class HashUtil
    {
        private const int DefaultMurmurSeed = 0x01000193;

        public static int MurmurHash3_fmix(int k)
        {
            k ^= (int) (((uint) k) >> 16);
            k *= unchecked((int) (0x85ebca6b));
            k ^= (int) (((uint) k) >> 13);
            k *= unchecked((int) (0xc2b2ae35));
            k ^= (int) (((uint) k) >> 16);
            return k;
        }

        public static int MurmurHash3_x86_32(byte[] data, int offset, int len)
        {
            return MurmurHash3_x86_32(data, offset, len, DefaultMurmurSeed);
        }

        /// <summary>Returns the MurmurHash3_x86_32 hash.</summary>
        public static int MurmurHash3_x86_32(byte[] data, int offset, int len, int seed)
        {
            var c1 = unchecked((int) (0xcc9e2d51));
            var c2 = unchecked(0x1b873593);
            var h1 = seed;
            var roundedEnd = offset + (len & unchecked((int) (0xfffffffc)));
            // round down to 4 byte block
            for (var i = offset; i < roundedEnd; i += 4)
            {
                // little endian load order
                var k1 = (data[i] & unchecked(0xff)) |
                         ((data[i + 1] & unchecked(0xff)) << 8) |
                         ((data[i + 2] & unchecked(0xff)) << 16) |
                         (data[i + 3] << 24);
                k1 *= c1;
                k1 = (k1 << 15) | ((int) (((uint) k1) >> 17));
                // ROTL32(k1,15);
                k1 *= c2;
                h1 ^= k1;
                h1 = (h1 << 13) | ((int) (((uint) h1) >> 19));
                // ROTL32(h1,13);
                h1 = h1*5 + unchecked((int) (0xe6546b64));
            }
            // tail
            var k11 = 0;
            switch (len & unchecked(0x03))
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
                    k11 = (k11 << 15) | ((int) (((uint) k11) >> 17));
                    // ROTL32(k1,15);
                    k11 *= c2;
                    h1 ^= k11;
                    break;
                }
            }
            // finalization
            h1 ^= len;
            h1 = MurmurHash3_fmix(h1);
            return h1;
        }
        
        public static int HashToIndex(int hash, int length) {
            if (hash == int.MinValue) {
                return 0;
            }
            return Math.Abs(hash) % length;
        }

    }
}