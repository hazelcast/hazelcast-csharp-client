// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Linq;
using System.Net;
using Hazelcast.Networking;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class HDnsTests
    {
        [Test]
        public void Test()
        {
            Assert.That(HDns.GetHostName(), Is.EqualTo(Dns.GetHostName()));

            Assert.That(AreEqual(HDns.GetHostEntry("127.0.0.1"), Dns.GetHostEntry("127.0.0.1")), Is.True);
            Assert.That(AreEqual(HDns.GetHostEntry("www.hazelcast.com"), Dns.GetHostEntry("www.hazelcast.com")), Is.True);

            Assert.That(AreEqual(HDns.GetHostEntry(IPAddress.Parse("127.0.0.1")), Dns.GetHostEntry(IPAddress.Parse("127.0.0.1"))), Is.True);

            Assert.That(AreEqual(HDns.GetHostAddresses("127.0.0.1"), Dns.GetHostAddresses("127.0.0.1")), Is.True);
            Assert.That(AreEqual(HDns.GetHostAddresses("www.hazelcast.com"), Dns.GetHostAddresses("www.hazelcast.com")), Is.True);
        }

        private static bool AreEqual(IPAddress[] left, IPAddress[] right)
        {
            if (left.Length != right.Length) return false;
            foreach (var x in left)
                if (!right.Contains(x))
                    return false;

            return true;
        }

        private static bool AreEqual(IPHostEntry left, IPHostEntry right)
        {
            if (left.HostName != right.HostName) return false;

            if (left.Aliases.Length != right.Aliases.Length) return false;
            foreach (var x in left.Aliases)
                if (!right.Aliases.Contains(x))
                    return false;

            if (left.AddressList.Length != right.AddressList.Length) return false;
            foreach (var x in left.AddressList)
                if (!right.AddressList.Contains(x))
                    return false;

            return true;
        }
    }
}
