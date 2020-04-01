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

namespace AsyncTests1.Networking
{
    public static class ByteArrayExtensions
    {
        public static int ReadInt32(this byte[] bytes, int position, bool bigEndian = false)
        {
            if (bytes.Length < position + sizeof(int))
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return bigEndian

                    ? bytes[position] << 24     | bytes[position + 1] << 16 |
                      bytes[position + 2] << 8  | bytes[position + 3]

                    : bytes[position]           | bytes[position + 1] << 8 |
                      bytes[position + 2] << 16 | bytes[position + 3] << 24;
            }
        }

        public static long ReadInt64(this byte[] bytes, int position, bool bigEndian = false)
        {
            if (bytes.Length < position + sizeof(long))
                throw new ArgumentOutOfRangeException(nameof(position));

            unchecked
            {
                return bigEndian

                    ? (long) bytes[position]     << 56 | (long) bytes[position + 1] << 48 |
                      (long) bytes[position + 2] << 40 | (long) bytes[position + 3] << 32 |
                      (long) bytes[position + 4] << 24 | (long) bytes[position + 5] << 16 |
                      (long) bytes[position + 6] << 8  | (long) bytes[position + 7]

                    : (long) bytes[position]           | (long) bytes[position + 1] << 8  |
                      (long) bytes[position + 2] << 16 | (long) bytes[position + 3] << 24 |
                      (long) bytes[position + 4] << 32 | (long) bytes[position + 5] << 40 |
                      (long) bytes[position + 6] << 48 | (long) bytes[position + 7] << 56;
            }
        }

        public static void WriteInt32(this byte[] bytes, int position, int value, bool bigEndian = false)
        {
            if (bytes.Length < position + sizeof(int))
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = (uint) value;

            unchecked
            {
                if (bigEndian)
                {
                    bytes[position]     = (byte) (unsigned >> 24);
                    bytes[position + 1] = (byte) (unsigned >> 16);
                    bytes[position + 2] = (byte) (unsigned >> 8);
                    bytes[position + 3] = (byte) unsigned;
                }
                else
                {
                    bytes[position]     = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                    bytes[position + 2] = (byte) (unsigned >> 16);
                    bytes[position + 3] = (byte) (unsigned >> 24);
                }
            }
        }

        public static void WriteInt64(this byte[] bytes, int position, long value, bool bigEndian = false)
        {
            if (bytes.Length < position + sizeof(long))
                throw new ArgumentOutOfRangeException(nameof(position));

            var unsigned = (ulong) value;

            unchecked
            {
                if (bigEndian)
                {
                    bytes[position]     = (byte) (unsigned >> 56);
                    bytes[position + 1] = (byte) (unsigned >> 48);
                    bytes[position + 2] = (byte) (unsigned >> 40);
                    bytes[position + 3] = (byte) (unsigned >> 32);
                    bytes[position + 4] = (byte) (unsigned >> 24);
                    bytes[position + 5] = (byte) (unsigned >> 16);
                    bytes[position + 6] = (byte) (unsigned >> 8);
                    bytes[position + 7] = (byte) unsigned;
                }
                else
                {
                    bytes[position]     = (byte) unsigned;
                    bytes[position + 1] = (byte) (unsigned >> 8);
                    bytes[position + 2] = (byte) (unsigned >> 16);
                    bytes[position + 3] = (byte) (unsigned >> 24);
                    bytes[position + 4] = (byte) (unsigned >> 32);
                    bytes[position + 5] = (byte) (unsigned >> 40);
                    bytes[position + 6] = (byte) (unsigned >> 48);
                    bytes[position + 7] = (byte) (unsigned >> 56);
                }
            }
        }
    }
}