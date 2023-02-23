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

#nullable enable

using System;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering;

[TestFixture]
public class InvocationTests
{
    [Test]
    public async Task Test()
    {
        var message = ClientPingCodec.EncodeRequest();
        var options = new MessagingOptions
        {
            MaxFastInvocationCount = 0,
            MinRetryDelayMilliseconds = 10,
            RetryTimeoutSeconds = 1
        };

        var clock = new TestClockSource { Now = DateTime.Now };
        using var clockOverride = Clock.Override(clock);

        // because RetryTimeoutSeconds = 1, cannot retry after 10s
        var invocation = new Invocation(message, options);
        var startTime = Clock.ToDateTime(invocation.StartTime);
        clock.Now = startTime.AddSeconds(10);
        await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await invocation.WaitRetryAsync(() => 0));

        // because RetryTimeoutSeconds = 1, can retry after 10ms
        // because MaxFastInvocationCount = 0, will immediately delay
        // delay is 1, 2, 4, 8, 16 ms but
        // - never less than MinRetryDelayMilliseconds
        // - never more than the total remaining time
        // so we're going to wait for 10ms here
        invocation = new Invocation(message, options);
        startTime = Clock.ToDateTime(invocation.StartTime);
        clock.Now = startTime.AddMilliseconds(10);
        await invocation.WaitRetryAsync(() => 0);

        // delay is constrained by remaining time, so here we do *not* wait for 10s
        options.MinRetryDelayMilliseconds = 10_000;
        invocation = new Invocation(message, options);
        startTime = Clock.ToDateTime(invocation.StartTime);
        clock.Now = startTime.AddMilliseconds(10);
        await invocation.WaitRetryAsync(() => 0);
    }

    private class TestClockSource : IClockSource
    {
        public DateTime Now { get; set; }
    }
}