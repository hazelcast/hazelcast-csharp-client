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
using Hazelcast.CP;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    [TestFixture]
    [Category("enterprise")]
    public class RaftTests
    {
        [Test]
        public void Test()
        {
            var x = new CPGroupId("name", 123, 456);

            Console.WriteLine(x);

            Assert.That(x.GetHashCode() == new CPGroupId("name", 123, 456).GetHashCode());

            Assert.That(x.Name, Is.EqualTo("name"));
            Assert.That(x.Seed, Is.EqualTo(123));
            Assert.That(x.Id, Is.EqualTo(456));

            Assert.That(x.Equals(x));
            Assert.That(x.Equals((object) x));
            Assert.That(x.Equals(null), Is.False);

            Assert.That(CPGroupId.Equals(x, x));
            Assert.That(CPGroupId.Equals(x, new CPGroupId("name", 123, 456)));
            Assert.That(CPGroupId.Equals(x, null), Is.False);

            Assert.That(x.Equals(new CPGroupId("name", 123, 456)));
            Assert.That(x.Equals(new CPGroupId("namex", 123, 456)), Is.False);
            Assert.That(x.Equals(new CPGroupId("name", 1234, 456)), Is.False);
            Assert.That(x.Equals(new CPGroupId("name", 123, 4567)), Is.False);

            Assert.That(x == new CPGroupId("name", 123, 456));
            Assert.That(x != new CPGroupId("namex", 123, 456));
            Assert.That(x != new CPGroupId("name", 1234, 456));
            Assert.That(x != new CPGroupId("name", 123, 4567));

            Assert.That(Equals(x, new CPGroupId("name", 123, 456)));
            Assert.That(Equals(x, null), Is.False);
        }
    }
}
