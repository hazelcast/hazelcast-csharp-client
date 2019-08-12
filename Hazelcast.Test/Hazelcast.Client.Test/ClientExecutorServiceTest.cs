// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class ClientExecutorServiceTest : HazelcastTestSupport
    {
        ClientExecutionService _clientExecutionService;

        CancellationTokenSource _cts;

        [SetUp]
        public void Setup()
        {
            _cts = new CancellationTokenSource();
            _clientExecutionService = new ClientExecutionService("name", 10);
        }

        [TearDown]
        public void TearDown()
        {
            _cts.Dispose();
            _clientExecutionService.Shutdown();
        }

        [Test]
        public void TestScheduleWithCancellation_cancelled()
        {
            var executed = false;
            var continued = false;
            _clientExecutionService.ScheduleWithCancellation(() =>
                {
                    executed = true;
                }, 2, TimeUnit.Seconds, _cts.Token)
                .ContinueWith(t =>
                    {
                        if (!t.IsCanceled)
                        {
                            continued = true;
                        }
                    }
            );
            _cts.Cancel();
            TestSupport.AssertTrueEventually(() =>
            {
                Assert.False(executed);
                Assert.False(continued);
            });
        }

        [Test]
        public void TestScheduleWithCancellation_completed()
        {
            var executed = false;
            var continued = false;
            _clientExecutionService.ScheduleWithCancellation(() =>
                {
                    executed = true;
                }, 2, TimeUnit.Seconds, _cts.Token)
                .ContinueWith(t =>
                    {
                        if (!t.IsCanceled)
                        {
                            continued = true;
                        }
                    }
                );
            TestSupport.AssertTrueEventually(() =>
            {
                Assert.True(executed);
                Assert.True(continued);
            });
        }

        [Test]
        public void TestSchedule_completed()
        {
            var executed = false;
            var continued = false;
            _clientExecutionService.Schedule(() =>
                {
                    executed = true;
                }, 2, TimeUnit.Seconds)
                .ContinueWith(t =>
                    {
                        if (!t.IsCanceled)
                        {
                            continued = true;
                        }
                    }
                );
            TestSupport.AssertTrueEventually(() =>
            {
                Assert.True(executed);
                Assert.True(continued);
            });
        }
    }
}