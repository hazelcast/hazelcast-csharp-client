﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
        public async Task WithNewContext()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            long idx = -1;
            await TaskEx.RunWithNewContext(() =>
            {
                idx = AsyncContext.CurrentContext.Id;
                return Task.CompletedTask;
            });

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithNewContextResult()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            var idx = await TaskEx.RunWithNewContext(() =>
                Task.FromResult(AsyncContext.CurrentContext.Id));

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithNewContextToken()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            long idx = -1;
            await TaskEx.RunWithNewContext(token =>
            {
                idx = AsyncContext.CurrentContext.Id;
                return Task.CompletedTask;
            }, CancellationToken.None);

            Assert.That(idx, Is.Not.EqualTo(id));
        }

        [Test]
        public async Task WithNewContextResultToken()
        {
            AsyncContext.Ensure();
            var id = AsyncContext.CurrentContext.Id;

            var idx = await TaskEx.RunWithNewContext(token =>
                Task.FromResult(AsyncContext.CurrentContext.Id), CancellationToken.None);

            Assert.That(idx, Is.Not.EqualTo(id));
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
        public async Task WithTimeoutThenCancel()
        {
            static async Task Run(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                    await Task.Delay(100, default);
                throw new Exception("bang!");
            }

            var cancellation = new CancellationTokenSource();
            try
            {
                await TaskEx.RunWithTimeout(Run, TimeSpan.FromMilliseconds(1), cancellation.Token);
            }
            catch (TaskTimeoutException)
            {
                // when TimeoutAfter throws an exception, the original task keeps
                // running... if it supports cancellation, it may be a good idea
                // to cancel it, or deal with the situation one way or another.

                cancellation.Cancel();
                cancellation.Dispose();
            }

            // however, the task ends up being unobserved!
            for (var i = 0; i < 40; i++)
            {
                if (GetUnobservedExceptions().Count > 0)
                    break;
                await Task.Delay(100, default);
            }
            Assert.That(GetUnobservedExceptions().Count, Is.GreaterThanOrEqualTo(1));

            ClearUnobservedExceptions();

            // so this is the proper way to do it:

            cancellation = new CancellationTokenSource();
            try
            {
                await TaskEx.RunWithTimeout(Run, TimeSpan.FromMilliseconds(1), cancellation.Token);
            }
            catch (TaskTimeoutException e)
            {
                // when TimeoutAfter throws an exception, the original task keeps
                // running... if it supports cancellation, it may be a good idea
                // to cancel it, or deal with the situation one way or another.

                cancellation.Cancel();
                cancellation.Dispose();

                try
                {
                    await e.Task.CAF();
                }
                catch
                {
                    // deal with the exception, if any
                }
            }

            // now if the task does not support being cancelled, at least the exception should be observed,
            // either by a custom continuation that will do something about the situation, or just with
            // the ObserveException extension method.

            try
            {
                await TaskEx.RunWithTimeout(Run, TimeSpan.FromMilliseconds(1), default);
            }
            catch (TaskTimeoutException e)
            {
                // when TimeoutAfter throws an exception, the original task keeps
                // running... if it does not support cancellation, one still needs
                // to deal with the situation one way or another. either by awaiting
                // the task (but then why the timeout?) or by making sure its
                // exception is observed.

                e.ObserveException();
            }
        }

        [Test]
        public async Task AwaitCanceled()
        {
            // ok to pass null
            await TaskEx.AwaitCanceled(null);

            // normal task = normal
            await TaskEx.AwaitCanceled(Task.CompletedTask);

            // canceled task = ok too
            var completionSource = new TaskCompletionSource<int>();
            completionSource.TrySetCanceled();
            await TaskEx.AwaitCanceled(completionSource.Task);
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
