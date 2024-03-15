// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Messaging;
using JetBrains.Annotations;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class ListCNFixedSizeCodec
    {
        private const int TypeNullOnly = 1;
        private const int TypeNotNullOnly = 2;
        private const int TypeMixed = 3;
        private const int HeaderSize = BytesExtensions.SizeOfByte + BytesExtensions.SizeOfInt;
        private const int ItemsPerBitmask = 8;

        public static IList<T?> Decode<T>(Frame frame, int itemSizeInBytes, DecodeBytesDelegate<T> decodeFunc) where T : struct
        {
            var type = frame.Bytes.ReadByte(0);
            var count = frame.Bytes.ReadInt(1, Endianness.LittleEndian);

            return type switch
            {
                TypeNullOnly => DecodeNullOnly<T>(count),
                TypeNotNullOnly => DecodeNotNullOnly(frame.Bytes, itemSizeInBytes, decodeFunc, count),
                TypeMixed => DecodeMixed(frame.Bytes, itemSizeInBytes, decodeFunc, count),
                _ => throw new NotSupportedException($"Type #{type} is not supported")
            };
        }

        private static IList<T?> DecodeNullOnly<T>(int count) where T : struct
        {
            var result = new List<T?>(count);
            for (var i = 0; i < count; i++)
                result.Add(null);

            return result;
        }

        [ItemCanBeNull]
        private static IList<T?> DecodeNotNullOnly<T>(byte[] bytes, int itemSizeInBytes, DecodeBytesDelegate<T> decodeFunc, int count) where T : struct
        {
            var res = new List<T?>(count);
            for (var i = 0; i < count; i++)
                res.Add(decodeFunc(bytes, HeaderSize + i * itemSizeInBytes));

            return res;
        }

        [ItemCanBeNull]
        private static IList<T?> DecodeMixed<T>(byte[] bytes, int itemSizeInBytes, DecodeBytesDelegate<T> decodeFunc, int count) where T : struct
        {
            var position = HeaderSize;
            var readCount = 0;

            var res = new List<T?>(count);
            while (readCount < count)
            {
                var bitmask = bytes.ReadByte(position++);
                for (var i = 0; i < ItemsPerBitmask && readCount < count; i++)
                {
                    var mask = 1 << i;
                    if ((bitmask & mask) == mask)
                    {
                        res.Add(decodeFunc(bytes, position));
                        position += itemSizeInBytes;
                    }
                    else
                    {
                        res.Add(null);
                    }

                    readCount++;
                }
            }

            return res;
        }
    }
}
