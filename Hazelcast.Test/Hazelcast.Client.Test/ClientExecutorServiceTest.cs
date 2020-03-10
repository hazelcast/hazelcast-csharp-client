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
using System.Threading.Tasks;
using Hazelcast.Client.Spi;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class ClientExecutorServiceTest : HazelcastTestSupport
    {
        private ExecutionService _executionService;

        [SetUp]
        public void Setup()
        {
            _executionService = new ExecutionService("", 10);
        }

        [TearDown]
        public void TearDown()
        {
            _executionService.Shutdown();
        }

        [Test]
        public async Task Schedule_Cancelled()
        {
            using (var cts = new CancellationTokenSource())
            {
                var task = _executionService.Schedule(
                        () => throw new Exception("Should not execute"), TimeSpan.FromSeconds(2), cts.Token);

                cts.Cancel();

                await task.ShouldThrow<TaskCanceledException>();
            }
        }

        [Test]
        public async Task Schedule_Completed()
        {
            var executed = new TaskCompletionSource<object>();

            var task = _executionService
                .Schedule(() => { executed.SetResult(executed); }, TimeSpan.FromSeconds(2), CancellationToken.None);

            await Task.WhenAll(executed.Task, task);
        }
    }
}