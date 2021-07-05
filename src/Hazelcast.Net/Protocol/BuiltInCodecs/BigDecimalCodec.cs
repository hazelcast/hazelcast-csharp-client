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

using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class BigDecimalCodec
    {
        private static readonly NumberFormatInfo NoSignFormat = new NumberFormatInfo
        {
            NegativeSign = "",
            PositiveSign = ""
        };

        public static string Decode(IEnumerator<Frame> iterator)
        {
            var bytes = iterator.Take().Bytes;
            var contentSize = bytes.ReadIntL(0);
            var body = bytes.Slice(BytesExtensions.SizeOfInt, BytesExtensions.SizeOfInt + contentSize);

            // FIXME [Oleksii] check and cleanup redundant code
            var isNegative = (body[0] & 0x80) > 0;
            //if (isNegative)
            //{
            //    for (var i = 0; i < body.Length; i++)
            //        body[i] = (byte)~body[i];
            //}

            var unscaled = new BigInteger(body);
            //if (isNegative)
            //{
            //    unscaled += 1;
            //}

            var scale = bytes.ReadIntL(BytesExtensions.SizeOfInt + contentSize);

            var unscaledString = unscaled.ToString("N", NoSignFormat) ?? "0";
            var unsignedString = scale switch
            {
                var x when x < 0 => $"{unscaledString}{new string('0', -scale)}",
                var x when x > 0 && x < unscaledString.Length => $"{unscaledString[..^scale]}.{unscaledString[^scale..]}",
                var x when x >= unscaledString.Length => $"0.{new string('0', scale - unscaledString.Length)}{unscaledString}",
                _ => unscaledString
            };

            return (unscaled.Sign < 0 ? '-' : (char?)null) + unsignedString;
        }

        public static string DecodeNullable(IEnumerator<Frame> iterator)
        {
            return CodecUtil.DecodeNullable(iterator, Decode);
        }
    }
}