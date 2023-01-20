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
using System.Collections.Generic;
using System.Numerics;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Models;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class BigDecimalCodec
    {
        public static HBigDecimal Decode(IEnumerator<Frame> iterator)
        {
            var bytes = iterator.Take().Bytes;
            var contentSize = bytes.ReadIntL(0);
            var body = new ReadOnlySpan<byte>(bytes, BytesExtensions.SizeOfInt, contentSize);

#if NETSTANDARD2_0
            var bodyLE = body.ToArray();
            Array.Reverse(bodyLE);
            var unscaled = new BigInteger(bodyLE);
#else
            var unscaled = new BigInteger(body, isUnsigned: false, isBigEndian: true);
#endif

            var scale = bytes.ReadIntL(BytesExtensions.SizeOfInt + contentSize);

            return new HBigDecimal(unscaled, scale);
        }

        public static HBigDecimal DecodeNullable(IEnumerator<Frame> iterator)
        {
            return iterator.SkipNull() ? default : Decode(iterator);
        }
    }
}