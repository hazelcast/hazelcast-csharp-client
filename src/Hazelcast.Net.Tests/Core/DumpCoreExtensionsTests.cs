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
using System.Text;
using Hazelcast.Core;
using Hazelcast.Tests.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class DumpCoreExtensionsTests
    {
        private const string Source = "HELLO THIS IS A TEST HOPEFULLY IT WORKS";
        private const string Expected = @"48 45 4c 4c 4f 20 54 48
49 53 20 49 53 20 41 20
54 45 53 54 20 48 4f 50
45 46 55 4c 4c 59 20 49
54 20 57 4f 52 4b 53 ";

        [Test]
        public void DumpBytes()
        {
            var bytes = Encoding.UTF8.GetBytes(Source);
            var dump = bytes.Dump();
            Assert.That(dump.ToLf(), Is.EqualTo(Expected.ToLf()));
        }

        [Test]
        public void DumpReadOnlySequence()
        {
            var bytes = Encoding.UTF8.GetBytes(Source);
            var sequence = new ReadOnlySequence<byte>(bytes);
            var dump = sequence.Dump();
            Assert.That(dump.ToLf(), Is.EqualTo(Expected.ToLf()));
        }

        [Test]
        public void ArgumentExceptions()
        {
            byte[] bytes = null;
            Assert.Throws<ArgumentNullException>(() => bytes.Dump());

            Assert.Throws<InvalidOperationException>(() => new byte[10].Dump(100));
        }
    }
}
