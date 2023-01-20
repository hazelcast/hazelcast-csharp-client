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
using System.Net;
using NUnit.Framework;

namespace Hazelcast.Tests.NetStandard
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class IPEndPointExTests
    {
        [Test]
        public void Parse()
        {
            Assert.That(IPAddress.TryParse("127.0.0.1", out var address), Is.True);
            var endpoint = new IPEndPoint(address, 80);

            Assert.That(IPEndPointEx.Parse("127.0.0.1:80"), Is.EqualTo(endpoint));
            Assert.That(IPEndPointEx.Parse(new ReadOnlySpan<char>("127.0.0.1:80".ToCharArray())), Is.EqualTo(endpoint));

            Assert.Throws<ArgumentNullException>(() => _ = IPEndPointEx.Parse((string) null));

            Assert.Throws<FormatException>(() => _ = IPEndPointEx.Parse(new ReadOnlySpan<char>("!**!".ToCharArray())));

            Assert.That(IPEndPointEx.TryParse("127.0.0.1:80", out var parsed), Is.True);
            Assert.That(parsed, Is.EqualTo(endpoint));

            Assert.That(IPEndPointEx.TryParse(new ReadOnlySpan<char>("127.0.0.1:80".ToCharArray()), out parsed), Is.True);
            Assert.That(parsed, Is.EqualTo(endpoint));

            Assert.That(IPEndPointEx.TryParse("[0:0:0:0:0:0:0:1]:80", out parsed), Is.True);
            Assert.That(parsed.ToString(), Is.EqualTo("[::1]:80"));

            Assert.That(IPEndPointEx.TryParse("[::1]:80", out parsed), Is.True);
            Assert.That(parsed.ToString(), Is.EqualTo("[::1]:80"));

            Assert.That(IPEndPointEx.TryParse(new ReadOnlySpan<char>("!**!:80".ToCharArray()), out _), Is.False);
            Assert.That(IPEndPointEx.TryParse(new ReadOnlySpan<char>("127.0.0.1:xx".ToCharArray()), out _), Is.False);
        }
    }
}
