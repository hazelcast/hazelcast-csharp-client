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
using System.Threading;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ClockTests
    {
        [Test]
        public void Never()
        {
            Assert.That(Clock.Never, Is.EqualTo(-1));
        }

        [Test]
        public void Milliseconds()
        {
            var m1 = Clock.Milliseconds;
            Thread.Sleep(100);
            var m2 = Clock.Milliseconds;
            Assert.That(m1, Is.GreaterThan(0));
            Assert.That(m2, Is.GreaterThan(m1 + 50));
        }

        [Test]
        public void Conversions()
        {
            var now = DateTime.Now;

            // clock rounds to milliseconds
            var ms = now.Millisecond;
            now = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMillisecond));
            Assert.That(now.Millisecond, Is.EqualTo(ms));

            var epoch = Clock.ToEpoch(now);
            Assert.That(Clock.ToDateTime(epoch), Is.EqualTo(now));
        }

        [Test]
        public void Initialize()
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.That(Clock.Origin, Is.EqualTo(origin));

            var options = new ClockOptions
            {
                OffsetMilliseconds = 1000
            };

            Assert.That(Clock.ToEpoch(origin), Is.EqualTo(0));
            Assert.That(Clock.ToDateTime(0), Is.EqualTo(origin));

            Clock.Reset();

            Clock.Initialize(options);
            Clock.Initialize(options); // ok to re-initialize with same options

            options.OffsetMilliseconds = 2000;
            Assert.Throws<InvalidOperationException>(() => Clock.Initialize(options)); // not ok with different options

            Assert.That(Clock.ToEpoch(origin), Is.EqualTo(1000));
            Assert.That(Clock.ToDateTime(1000), Is.EqualTo(origin));

            Clock.Reset();

            Assert.That(Clock.ToEpoch(origin), Is.EqualTo(0));

            Clock.Initialize(new ClockOptions());

            Clock.Reset();
        }

        [Test]
        public void Options()
        {
            var options = new ClockOptions { OffsetMilliseconds = 100 };
            Assert.That(options.OffsetMilliseconds, Is.EqualTo(100));

            var clone = options.Clone();
            Assert.That(clone.OffsetMilliseconds, Is.EqualTo(100));
        }
    }
}
