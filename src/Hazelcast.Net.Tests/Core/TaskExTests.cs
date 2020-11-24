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
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class TaskExTests : ObservingTestBase
    {
        [SetUp]
        [TearDown]
        public void Reset()
        {
            AsyncContext.ResetSequence();
        }

        [Test]
        public async Task WithTimeout()
        {
            // arg exception
            await AssertEx.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await TaskEx.RunWithTimeout(null, 0);
            });

            // completes before timeout = ok
            // (and, no unobserved exception)
            await TaskEx.RunWithTimeout(t => Task.CompletedTask, Timeout.InfiniteTimeSpan);
            await TaskEx.RunWithTimeout(t => Task.CompletedTask, TimeSpan.FromSeconds(60));

            // timeout before end of task = timeout exception
            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () =>
            {
                await TaskEx.RunWithTimeout(t => Delay(default), TimeSpan.FromMilliseconds(1));
            });

            // throws before timeout = get the thrown exception
            await AssertEx.ThrowsAsync<NotSupportedException>(async () =>
            {
                await TaskEx.RunWithTimeout(t => Throw(), Timeout.InfiniteTimeSpan);
            });

            // canceled before timeout = canceled exception
            await AssertEx.ThrowsAsync<TaskCanceledException>(async () =>
            {
                await TaskEx.RunWithTimeout(t => CancelTask(), Timeout.InfiniteTimeSpan);
            });

            // canceled before timeout = canceled exception
            await AssertEx.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await TaskEx.RunWithTimeout(t => CancelOperation(), Timeout.InfiniteTimeSpan);
            });
        }

        [Test]
        public async Task WithTimeoutAndObservedException()
        {
            static async Task Run(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                    await Task.Delay(100, default);
                throw new Exception("bang!");
            }

            // timeout before end of task = timeout exception
            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () =>
            {
                await TaskEx.RunWithTimeout(Run, TimeSpan.FromMilliseconds(1));
            });

            // and, the test should not leak the task exception
        }

        private static Task Delay(CancellationToken cancellationToken)
        {
            return Task.Delay(4_000, cancellationToken);
        }

        private static ValueTask DelayValue(CancellationToken cancellationToken)
        {
            return new ValueTask(Task.Delay(4_000, cancellationToken));
        }

        private static async Task<int> DelayResult(CancellationToken cancellationToken)
        {
            await Task.Delay(4_000, cancellationToken);
            return 0;
        }

        private static async ValueTask<int> DelayValueResult(CancellationToken cancellationToken)
        {
            await Task.Delay(4_000, cancellationToken);
            return 0;
        }

        private static Task Throw()
        {
            return Task.FromException(new NotSupportedException("bang"));
        }

        private static ValueTask ThrowValue()
        {
            return new ValueTask(Task.FromException(new NotSupportedException("bang")));
        }

        private static Task<int> ThrowResult()
        {
            return Task.FromException<int>(new NotSupportedException("bang"));
        }

        private static ValueTask<int> ThrowValueResult()
        {
            return new ValueTask<int>(Task.FromException<int>(new NotSupportedException("bang")));
        }

        private static Task CancelTask()
        {
            return Task.FromCanceled(new CancellationToken(true));
        }

        private static ValueTask CancelValueTask()
        {
            return new ValueTask(Task.FromCanceled(new CancellationToken(true)));
        }

        private static Task<int> CancelTaskResult()
        {
            return Task.FromCanceled<int>(new CancellationToken(true));
        }

        private static ValueTask<int> CancelValueTaskResult()
        {
            return new ValueTask<int>(Task.FromCanceled<int>(new CancellationToken(true)));
        }

        private static Task CancelOperation()
        {
            return Task.FromException(new OperationCanceledException("cancel"));
        }

        private static ValueTask CancelValueOperation()
        {
            return new ValueTask(Task.FromException(new OperationCanceledException("cancel")));
        }

        private static Task<int> CancelOperationResult()
        {
            return Task.FromException<int>(new OperationCanceledException("cancel"));
        }

        private static ValueTask<int> CancelValueOperationResult()
        {
            return new ValueTask<int>(Task.FromException<int>(new OperationCanceledException("cancel")));
        }
    }
}
