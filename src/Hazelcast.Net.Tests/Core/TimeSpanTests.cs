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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class TimeSpanTests
    {
        [Test]
        public void TimeSpanValues()
        {
            Assert.That(TimeSpanExtensions.MinusOneMillisecond.TotalMilliseconds, Is.EqualTo(-1));
            Assert.That(TimeSpan.Zero.TotalMilliseconds, Is.EqualTo(0));
        }

        [Test]
        public void Extensions()
        {
            Assert.That(TimeSpan.FromMilliseconds(123).RoundedMilliseconds(), Is.EqualTo(123));
            Assert.That(TimeSpan.FromMilliseconds(0).RoundedMilliseconds(), Is.EqualTo(0));
            Assert.That(TimeSpan.FromMilliseconds(-123).RoundedMilliseconds(), Is.EqualTo(-1));

            // a tick is 100ns ie 0.0001 ms, ie 1ms = 10000 ticks

            // in .NET 462, and Core 2.1, FromMilliseconds is this, with scale being 1:
            //double num = value * (double) scale + (value >= 0.0 ? 0.5 : -0.5);
            //return num <= 922337203685477.0 && num >= -922337203685477.0 ? new TimeSpan((long)num * 10000L) : throw new OverflowException(Environment.GetResourceString("Overflow_TimeSpanTooLong"));

            // in NET 3.1, FromMilliseconds is this, with scale being TicksPerMilliseconds ie 10000
            //double ticks = value * scale;
            //if ((ticks > long.MaxValue) || (ticks < long.MinValue)) throw new OverflowException(SR.Overflow_TimeSpanTooLong);
            //return new TimeSpan((long) ticks);

            // therefore, .01 ms rounds to zero in some cases, and to non-zero in others
            // and the value must be >= .5 to round to a non-zero value!
            Assert.That(TimeSpan.FromMilliseconds(.5), Is.Not.EqualTo(TimeSpan.Zero));

            Assert.That(TimeSpan.FromTicks(1), Is.Not.EqualTo(TimeSpan.Zero));         // 1 tick = 100 ns = .0001 ms
            Assert.That(TimeSpan.FromMilliseconds(.9), Is.Not.EqualTo(TimeSpan.Zero)); // .9 ms = 9000 ticks
            Assert.That(TimeSpan.FromTicks(10), Is.Not.EqualTo(TimeSpan.Zero));        // 10 ticks = 1 ms

            // 100 ticks is .01 ms and would round to zero
            Assert.That(TimeSpan.FromTicks(100).RoundedMilliseconds(), Is.EqualTo(0));
            Assert.That(TimeSpan.FromTicks(100).RoundedMilliseconds(false), Is.EqualTo(1));

            Assert.That(TimeSpan.FromMilliseconds(42).RoundedMilliseconds().ClampToInt32(), Is.EqualTo(42));

            Assert.That(TimeSpan.FromMilliseconds((long) int.MaxValue + 123).RoundedMilliseconds().ClampToInt32(), Is.EqualTo(int.MaxValue));
            Assert.That(TimeSpan.FromMilliseconds((long) int.MinValue - 123).RoundedMilliseconds().ClampToInt32(), Is.EqualTo(-1));
        }
    }
}
