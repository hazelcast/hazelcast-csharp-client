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
using System.Linq;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [Test]
        public void Shuffle()
        {
            var items = new[] { 1, 2, 3, 4, 5 };

            var j = 0;
            var difference = false;

            while (!difference && j++ < 5)
            {
                var shuffled = items.Shuffle().ToArray();

                Assert.That(shuffled.Length, Is.EqualTo(5));

                for (var i = 0; i < 5; i++)
                {
                    Assert.That(shuffled, Contains.Item(i + 1));
                    difference |= items[i] != shuffled[i];
                }
            }

            Assert.That(difference, Is.True);
        }

        [Test]
        public void Combine()
        {
            var items1 = new[] { 1, 2, 3 };
            var items2 = new[] { "a", "b", "c" };
            var items3 = new[] { 4, 5, 6 };
            var items4 = new[] { 'x', 'y', 'z', 't' };

            var items = EnumerableExtensions.Combine(items1, items2, items3, items4).ToArray();

            Assert.That(items.Length, Is.EqualTo(3));

            Assert.That(items[0], Is.EqualTo((1, "a", 4, 'x')));
            Assert.That(items[1], Is.EqualTo((2, "b", 5, 'y')));
            Assert.That(items[2], Is.EqualTo((3, "c", 6, 'z')));
        }

        [Test]
        public void ArgumentExceptions()
        {
            var items = Array.Empty<int>();

            Assert.Throws<ArgumentNullException>(() => _ = EnumerableExtensions.Combine(items, items, items, (int[]) null).ToArray());
            Assert.Throws<ArgumentNullException>(() => _ = EnumerableExtensions.Combine(items, items, (int[]) null, items).ToArray());
            Assert.Throws<ArgumentNullException>(() => _ = EnumerableExtensions.Combine(items, (int[]) null, items, items).ToArray());
            Assert.Throws<ArgumentNullException>(() => _ = EnumerableExtensions.Combine((int[]) null, items, items, items).ToArray());
        }
    }
}
