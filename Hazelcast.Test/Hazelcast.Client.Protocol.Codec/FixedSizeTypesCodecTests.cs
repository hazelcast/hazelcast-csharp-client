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
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using NUnit.Framework;

namespace Hazelcast.Client.Protocol.Codec
{
    public class FixedSizeTypesCodecTests
    {
        [Test]
        public void TestGuid()
        {
            var bytes = new byte[17];
            FixedSizeTypesCodec.EncodeGuid(bytes, 0, Guid.Parse("08070605-0403-0201-100f-0e0d0c0b0a09"));
            var expectedBytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            Assert.AreEqual(expectedBytes, bytes);
        }

        [Test]
        public void TestGuidDecode()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            var decodeGuid = FixedSizeTypesCodec.DecodeGuid(bytes, 0);
            var guid = Guid.Parse("08070605-0403-0201-100f-0e0d0c0b0a09");
            Assert.AreEqual(decodeGuid, guid);
        }
    }
}