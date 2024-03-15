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
using System.Threading.Tasks;
using Hazelcast.CP;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    [TestFixture]
    public class CPTests
    {
        [Test]
        public void ParseName()
        {
            Assert.That(CPSubsystem.ParseName("foo"), Is.EqualTo((CPSubsystem.DefaultGroupName, "foo", "foo@" + CPSubsystem.DefaultGroupName)));
            Assert.That(CPSubsystem.ParseName("foo@bar"), Is.EqualTo(("bar", "foo", "foo@bar")));
            Assert.That(CPSubsystem.ParseName("   foo   @   bar   "), Is.EqualTo(("bar", "foo", "foo@bar")));
            Assert.That(CPSubsystem.ParseName("foo@" + CPSubsystem.DefaultGroupName), Is.EqualTo((CPSubsystem.DefaultGroupName, "foo", "foo@" + CPSubsystem.DefaultGroupName)));
            Assert.That(CPSubsystem.ParseName("foo@" + CPSubsystem.DefaultGroupName.ToLower()), Is.EqualTo((CPSubsystem.DefaultGroupName, "foo", "foo@" + CPSubsystem.DefaultGroupName)));
            Assert.That(CPSubsystem.ParseName("foo@" + CPSubsystem.DefaultGroupName.ToUpper()), Is.EqualTo((CPSubsystem.DefaultGroupName, "foo", "foo@" + CPSubsystem.DefaultGroupName)));

            Assert.Throws<NotSupportedException>(() => CPSubsystem.ParseName("foo@" + CPSubsystem.MetaDataGroupName));

            Assert.Throws<ArgumentException>(() => CPSubsystem.ParseName(null));
            Assert.Throws<ArgumentException>(() => CPSubsystem.ParseName(""));
            Assert.Throws<ArgumentException>(() => CPSubsystem.ParseName("     "));

            Assert.Throws<ArgumentException>(() => CPSubsystem.ParseName("@bar"));
            Assert.Throws<ArgumentException>(() => CPSubsystem.ParseName("foo@"));
            Assert.Throws<ArgumentException>(() => CPSubsystem.ParseName("foo@@bar"));
            Assert.Throws<ArgumentException>(() => CPSubsystem.ParseName("foo@bar@bar"));
        }

        [Test]
        public async Task TestCPSession()
        {
            var session = new CPSession(1, 1_000);
            
            Assert.False(session.IsExpired);
            Assert.That(session.AcquireCount, Is.Zero);
            Assert.That(session.Acquire(1), Is.EqualTo(1));
            Assert.True(session.IsValid);
            Assert.True(session.IsInUse);
            session.Release(1);
            Assert.That(session.AcquireCount, Is.Zero);
            Assert.That(session.GetHashCode(), Is.EqualTo(1));
            Assert.That(session.Id, Is.EqualTo(1));
            Assert.True(session.Equals(session));
            var s2 = new CPSession(2, 10);
            Assert.False(session.Equals(s2));
            await Task.Delay(50);
            Assert.True(s2.IsExpired);
            
        }
    }
}
