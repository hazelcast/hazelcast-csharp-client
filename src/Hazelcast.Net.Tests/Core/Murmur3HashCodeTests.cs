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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class Murmur3HashCodeTests
    {
        [Test]
        public void Test()
        {
            _ = Murmur3HashCode.Hash(new byte[100], 0, 100);
            _ = Murmur3HashCode.Hash(new byte[100], 0, 99);
            _ = Murmur3HashCode.Hash(new byte[100], 0, 98);
        }

        [Test]
        public void ArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => Murmur3HashCode.Hash(null, 0, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3HashCode.Hash(new byte[10], -1, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3HashCode.Hash(new byte[10], 11, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3HashCode.Hash(new byte[10], 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Murmur3HashCode.Hash(new byte[10], 5, 6));
        }
    }
}
