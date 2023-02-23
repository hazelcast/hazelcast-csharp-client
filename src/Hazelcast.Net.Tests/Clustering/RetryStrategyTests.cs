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

using Hazelcast.Clustering;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering
{
    [TestFixture]
    public class RetryStrategyTests
    {
        [Test]
        public void TestDelayWithTimeout()
        {
            var retry = new RetryStrategy("test", 1_000, 30_000, 1, 60_000, 0, new NullLoggerFactory());

            Assert.That(retry.GetDelay(1_000), Is.EqualTo(1_000));
            Assert.That(retry.GetDelay(2_000), Is.EqualTo(1_000));

            Assert.That(retry.GetDelay(59_000), Is.EqualTo(1_000));
            Assert.That(retry.GetDelay(59_500), Is.EqualTo(500)); // almost timeout
            Assert.That(retry.GetDelay(60_000), Is.EqualTo(0)); // timeout

            // elapsed cannot be > timeout - but just in case
            Assert.That(retry.GetDelay(80_000), Is.EqualTo(0));
        }

        [Test]
        public void TestDelayInfiniteTimeout()
        {
            var retry = new RetryStrategy("test", 1_000, 30_000, 1, -1, 0, new NullLoggerFactory());

            Assert.That(retry.GetDelay(1_000), Is.EqualTo(1_000));
            Assert.That(retry.GetDelay(2_000), Is.EqualTo(1_000));

            Assert.That(retry.GetDelay(int.MaxValue), Is.EqualTo(1_000)); // no timeout
        }

        [Test]
        public void TestDelayJitter()
        {
            var retry0 = new RetryStrategy("test", 1_000, 30_000, 1, 60_000, 0, new NullLoggerFactory());
            Assert.That(retry0.GetDelay(1_000), Is.EqualTo(1_000));

            var retry1 = new RetryStrategy("test", 1_000, 30_000, 1, 60_000, 1, new NullLoggerFactory());
            const int count = 100;
            var min = int.MaxValue;
            var max = int.MinValue;
            for (var i = 0; i < count; i++)
            {
                var delay = retry1.GetDelay(1_000);
                if (min > delay) min = delay;
                if (max < delay) max = delay;
            }
            Assert.That(min, Is.LessThan(1_000)); // some values were lower than initial
            Assert.That(max, Is.GreaterThan(1_000)); // some values were greater than initial
        }

        [TestCase(1_000, 1, 1_000)]
        [TestCase(1_000, 2, 2_000)]
        [TestCase(8_000, 2, 16_000)]
        [TestCase(16_000, 2, 30_000)] // max!
        public void TestBackoff(int initial, int multiplier, int expected)
        {
            var retry = new RetryStrategy("test", initial, 30_000, multiplier, 60_000, 0, new NullLoggerFactory());

            Assert.That(retry.GetNewBackoff(), Is.EqualTo(expected));
        }
    }
}
