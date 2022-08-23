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

using System;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ActionExtensionsTests
    {
        [Test]
        public async Task Args1()
        {
            var c = 0;

            Action<int> synchronous = x => { c = x; };

            var asynchronous = synchronous.AsAsync();

            await asynchronous(42);

            Assert.That(c, Is.EqualTo(42));
        }

        [Test]
        public async Task Args2()
        {
            var c1 = 0;
            var c2 = 0;

            Action<int, int> synchronous = (x, y) => { c1 = x; c2 = y; };

            var asynchronous = synchronous.AsAsync();

            await asynchronous(42, 12);

            Assert.That(c1, Is.EqualTo(42));
            Assert.That(c2, Is.EqualTo(12));
        }
    }
}
