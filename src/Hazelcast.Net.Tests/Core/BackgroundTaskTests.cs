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

using System;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class BackgroundTaskTests : HazelcastTestBase
    {
        // a BackgroundTask is like running a task with Task.Run, and supports being
        // canceled, by creating and handling and disposing a cancellation source.
        //
        // cancellation assumes that the task monitors the token and will cancel
        // nicely, we cannot abort it if it does not collaborate.

        // all in all, this simplifies our code, we don't have to manage the cancellation
        // sources everywhere.

        [Test]
        public async Task Completes()
        {
            var task = BackgroundTask.Run(token => Task.CompletedTask);

            await task.Task;
        }

        [Test]
        public async Task Cancels()
        {
            var task = BackgroundTask.Run(async token => await Task.Delay(1000, token));

            await task.CompletedOrCancelAsync();

            Assert.That(task.Task.IsCanceled);
        }

        [Test]
        public async Task TaskRunImmediateExceptions()
        {
            // Task.Run(Func<Task>) is OK with the task factory throwing immediately
            var task = Task.Run(/*Func<Task>*/ () => throw new Exception("bang"));

            await task.CfAwaitNoThrow();
        }

        [Test]
        public async Task BackgroundTaskImmediateExceptions()
        {
            // BackgroundTask.Run works the same
            var task = BackgroundTask.Run(token => throw new Exception("bang"));

            await task.Task.CfAwaitNoThrow();
        }

        [Test]
        public async Task CancelBubblesException()
        {
            var task = BackgroundTask.Run(async token =>
            {
                await Task.Yield();
                throw new Exception("bang");
            });

            var e = await AssertEx.ThrowsAsync<Exception>(async () => await task.CompletedOrCancelAsync());

            Assert.That(e.Message, Is.EqualTo("bang"));
        }

        [Test]
        public async Task ObserveCancelDoesNotThrow()
        {
            var task = BackgroundTask.Run(token => throw new Exception("bang"));

            await task.CompletedOrCancelAsync(true);
        }
    }
}
