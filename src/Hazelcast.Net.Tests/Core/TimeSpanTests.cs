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
    public class TimeSpanTests
    {
        [Test]
        public void LeaseTimeProperties()
        {
            Assert.That(LeaseTime.InfiniteTimeSpan, Is.EqualTo(Timeout.InfiniteTimeSpan));
            Assert.That(LeaseTime.Zero, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void TimeToLiveProperties()
        {
            Assert.That(TimeToLive.InfiniteTimeSpan, Is.EqualTo(Timeout.InfiniteTimeSpan));
            Assert.That(TimeToLive.Zero, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void TimeToWaitProperties()
        {
            Assert.That(TimeToWait.InfiniteTimeSpan, Is.EqualTo(Timeout.InfiniteTimeSpan));
            Assert.That(TimeToWait.Zero, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void Extensions()
        {
            Assert.That(TimeSpan.FromMilliseconds(100).CodecMilliseconds(200), Is.EqualTo(100));
            Assert.That(Timeout.InfiniteTimeSpan.CodecMilliseconds(200), Is.EqualTo(200));

            Assert.That(TimeSpan.FromMilliseconds(100).TimeoutMilliseconds(200, 300), Is.EqualTo(100));
            Assert.That(Timeout.InfiniteTimeSpan.TimeoutMilliseconds(200, 300), Is.EqualTo(300));
            Assert.That(TimeSpan.Zero.TimeoutMilliseconds(200, 300), Is.EqualTo(200));
        }
    }
}
