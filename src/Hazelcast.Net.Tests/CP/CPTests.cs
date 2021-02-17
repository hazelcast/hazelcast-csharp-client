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

using System;
using NUnit.Framework;

namespace Hazelcast.Tests.CP
{
    using CP = Hazelcast.CP.CP;

    [TestFixture]
    public class CPTests
    {
        [Test]
        public void ParseName()
        {
            Assert.That(CP.ParseName("foo"), Is.EqualTo((CP.DefaultGroupName, "foo")));
            Assert.That(CP.ParseName("foo@bar"), Is.EqualTo(("bar", "foo")));
            Assert.That(CP.ParseName("   foo   @   bar   "), Is.EqualTo(("bar", "foo")));
            Assert.That(CP.ParseName("foo@" + CP.DefaultGroupName), Is.EqualTo((CP.DefaultGroupName, "foo")));
            Assert.That(CP.ParseName("foo@" + CP.DefaultGroupName.ToLower()), Is.EqualTo((CP.DefaultGroupName, "foo")));
            Assert.That(CP.ParseName("foo@" + CP.DefaultGroupName.ToUpper()), Is.EqualTo((CP.DefaultGroupName, "foo")));

            Assert.Throws<NotSupportedException>(() => CP.ParseName("foo@" + CP.MetaDataGroupName));

            Assert.Throws<ArgumentException>(() => CP.ParseName(null));
            Assert.Throws<ArgumentException>(() => CP.ParseName(""));
            Assert.Throws<ArgumentException>(() => CP.ParseName("     "));

            Assert.Throws<ArgumentException>(() => CP.ParseName("@bar"));
            Assert.Throws<ArgumentException>(() => CP.ParseName("foo@"));
            Assert.Throws<ArgumentException>(() => CP.ParseName("foo@@bar"));
            Assert.Throws<ArgumentException>(() => CP.ParseName("foo@bar@bar"));
        }
    }
}