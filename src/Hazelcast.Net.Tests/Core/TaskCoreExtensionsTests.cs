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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class TaskCoreExtensionsTests : ObservingTestBase
    {
        // NOTE
        //
        // When an exception is added to a task via Task.AddException(object exceptionObject,
        // bool representsCancellation), a new TaskExceptionHolder is instantiated and associated
        // with the task. It is this holder which has a finalizer (~TaskExceptionHolder method)
        // which, when the holder is finalized, gathers the exceptions and raises them as
        // unobserved exceptions. Retrieving the exceptions from a task does mark the holder as
        // handled, so it will not raise its exceptions as unobserved.
        // And, the TaskExceptionHolder class treats cancellation exceptions differently. When
        // asked to gather the task's exception, it *does* add a TaskCancelledException to the
        // list, but when finalizing, it does *not* raise this exception as unobserved!

        // awaiting a task that has been canceled, throws
        //
        [Test]
        public async Task CancelAtAwait()
        {
            var cancellation = new CancellationTokenSource(1);
            var task = Task.Delay(TimeSpan.FromSeconds(1), cancellation.Token);

            await AssertEx.ThrowsAsync<OperationCanceledException>(async () => await task);
        }

        // not-awaiting a task that has been canceled, does *not* cause an unobserved exception
        //
        [Test]
        public async Task CancelUnobserved()
        {
            // make sure the canceled task is not referenced here,
            // and therefore will be finalized when getting exceptions
            CancelUnobservedAsync();

            // give time for the cancellation to happen
            await Task.Delay(1000, CancellationToken.None);

            // does *not* produce an unobserved exception
            var unobservedExceptions = GetUnobservedExceptions();
            Assert.That(unobservedExceptions.Count, Is.EqualTo(0));
        }

        // awaiting a task that throws, throws
        //
        [Test]
        public async Task ThrowAtAwait()
        {
            var task = ThrowAsync();

            await AssertEx.ThrowsAsync<Exception>(async () => await task.CfAwait());
        }

        // except if prevented from throwing - AwaitNoContextNoThrow replaces
        // the awaiter by an awaiter that does not throw but observes exceptions
        //
        [Test]
        public async Task ThrowAtAwaitNot()
        {
            var task = ThrowAsync();

            await task.CfAwaitNoThrow();
        }

        // same for value task
        //
        [Test]
        public async Task ThrowAtAwaitValueNot()
        {
            await ThrowValueAsync().CfAwaitNoThrow();
        }

        // not-awaiting a task that throws, causes an unobserved exception
        //
        [Test]
        public async Task ThrowAtObserve()
        {
            // make sure the throwing task is not referenced here,
            // and therefore will be finalized when getting exceptions
            ThrowUnobservedAsync();

            // give time for the exception to be thrown
            await Task.Delay(1000, CancellationToken.None);

            // produces an unobserved exception!
            var unobservedExceptions = GetUnobservedExceptions();
            Assert.That(unobservedExceptions.Count, Is.EqualTo(1));

            // don't fail the test
            ClearUnobservedExceptions();
        }

        // except if prevented by observing the exception
        // which is achieved by actually awaiting the task,
        // through AwaitNoContextNoThrow
        //
        [Test]
        public async Task ThrowAtObserveNot()
        {
            ThrowObservedAsync();

            // give time for the exception to be thrown
            await Task.Delay(1000, CancellationToken.None);
        }

        // ---- utilities ----

        // starts an unobserved task that cancels
        private static void CancelUnobservedAsync()
        {
            var cancellation = new CancellationTokenSource(1);
            var task = Task.Delay(TimeSpan.FromSeconds(1), cancellation.Token);
        }

        // starts an unobserved task that throws
        private static void ThrowUnobservedAsync()
        {
            var task = ThrowAsync();
        }

        // starts an observed task that throws
        private static void ThrowObservedAsync()
        {
            var task = ThrowAsync();

            task.ObserveException();
        }

        // a true async method that throws
        private static async Task ThrowAsync([CallerMemberName] string context = null)
        {
            await Task.Yield();
            throw new Exception($"{nameof(ThrowAsync)} ({context})!");
        }

        // same, but a value task
        private static async ValueTask ThrowValueAsync([CallerMemberName] string context = null)
        {
            await Task.Yield();
            throw new Exception($"{nameof(ThrowAsync)} ({context})!");
        }

        // a true async method that awaits a delay and then throws
        private static async Task DelayThrowAsync(TimeSpan delay, CancellationToken token, bool throwCanceled = true, [CallerMemberName] string context = null)
        {
            try
            {
                await Task.Delay(delay, token);
            }
            catch
            {
                if (throwCanceled) throw;
            }

            throw new Exception($"{nameof(DelayThrowAsync)} ({context})!");
        }

        // a true async method that awaits a delay
        private static async Task DelayAsync(TimeSpan delay, CancellationToken token)
        {
            await Task.Delay(delay, token);
        }

        // ---- timeouts ----

        [Test]
        public async Task TaskCompletesBeforeTimeout()
        {
            var stopwatch = Stopwatch.StartNew();

            // awaited task with timeout, which completes before timeout, is ok
            using var cancellation = new CancellationTokenSource();
            var task = DelayAsync(TimeSpan.FromSeconds(1), cancellation.Token);
            await task
                .CfAwait(TimeSpan.FromSeconds(10), cancellation);

            // completes before timeout
            var elapsed = stopwatch.Elapsed;
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));

            // cancellation has not been cancelled
            Assert.That(cancellation.IsCancellationRequested, Is.False);

            // the timeout delay task has been cancelled but does not leak
            // an unobserved exception, because its exceptions are NoThrow.
        }

        [Test]
        public async Task TaskThrowsBeforeTimeout()
        {
            var stopwatch = Stopwatch.StartNew();

            // awaited task with timeout, which throws before timeout, throws
            using var cancellation = new CancellationTokenSource();
            var task = DelayThrowAsync(TimeSpan.FromSeconds(1), cancellation.Token, false);
            var e = await AssertEx.ThrowsAsync<Exception>(async () =>
                await task.CfAwait(TimeSpan.FromSeconds(10), cancellation));

            // completes before timeout
            var elapsed = stopwatch.Elapsed;
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));

            // cancellation has not been cancelled
            Assert.That(cancellation.IsCancellationRequested, Is.False);

            // the timeout delay task has been cancelled but does not leak
            // an unobserved exception, because its exceptions are NoThrow.

            // the original task has thrown the 'e' exception
            // which has been caught ie observed ie not leaked either
        }

        [Test]
        public async Task TaskTimesOutBeforeCompletion()
        {
            var stopwatch = Stopwatch.StartNew();

            // awaited task with timeout, which times out before completion, throws
            using var cancellation = new CancellationTokenSource();
            var task = DelayAsync(TimeSpan.FromSeconds(10), cancellation.Token);
            var e = await AssertEx.ThrowsAsync<TaskTimeoutException>(async () =>
                await task.CfAwait(TimeSpan.FromSeconds(1), cancellation));

            Assert.That(e.Task, Is.SameAs(task));

            // times out before completion
            var elapsed = stopwatch.Elapsed;
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));

            // cancellation has been cancelled
            // if the original task supports cancellation, it will cancel
            Assert.That(cancellation.IsCancellationRequested, Is.True);

            // the original task has been cancelled but does not leak
            // an unobserved exception, because its exceptions are NoThrow.
            await AssertEx.SucceedsEventually(() => Assert.That(e.Task.IsCanceled), 10_000, 500);
        }

        [Test]
        public async Task TaskThrowsAfterTimeout()
        {
            var stopwatch = Stopwatch.StartNew();

            // awaited task with timeout, which times out before completion, throws
            using var cancellation = new CancellationTokenSource();
            var task = DelayThrowAsync(TimeSpan.FromSeconds(10), cancellation.Token, false);
            var e = await AssertEx.ThrowsAsync<TaskTimeoutException>(async () =>
                await task.CfAwait(TimeSpan.FromSeconds(1), cancellation));

            Assert.That(e.Task, Is.SameAs(task));

            // times out before completion
            var elapsed = stopwatch.Elapsed;
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));

            // cancellation has been cancelled
            // if the original task supports cancellation, it will cancel
            Assert.That(cancellation.IsCancellationRequested, Is.True);

            // the task is faulted, and has thrown an exception!
            await AssertEx.SucceedsEventually(() => Assert.That(e.Task.IsFaulted), 10_000, 500);

            // the exception has been observed, thus not leaking here.
        }

        [Test]
        public async Task TaskThrowsAfterTimeoutWithException()
        {
            var stopwatch = Stopwatch.StartNew();

            // awaited task with timeout, which times out before completion, throws
            using var cancellation = new CancellationTokenSource();
            var task = DelayThrowAsync(TimeSpan.FromSeconds(10), cancellation.Token, false);
            var e = await AssertEx.ThrowsAsync<TaskTimeoutException>(async () =>
                await task.CfAwait(TimeSpan.FromSeconds(1), cancellation));

            Assert.That(e.Task, Is.SameAs(task));

            // times out before completion
            var elapsed = stopwatch.Elapsed;
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));

            // cancellation has been cancelled
            // if the original task supports cancellation, it will cancel
            Assert.That(cancellation.IsCancellationRequested, Is.True);

            // the task is faulted, and has thrown an exception!
            await AssertEx.SucceedsEventually(() => Assert.That(e.Task.IsFaulted), 10000, 500);

            // the exception has been observed, and would not leak
            // now, assuming we want to handle it, we can await the
            // original task... or just get its exception... anything

            await AssertEx.ThrowsAsync<Exception>(async () => await e.Task);
        }

        [Test]
        public async Task TaskWithTokenTimesOutBeforeCompletion()
        {
            var cancellationToken = CancellationToken.None;

            var stopwatch = Stopwatch.StartNew();

            // awaited task with timeout, which times out before completion, throws
            // the cancellation is linked to the original cancellation token
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = DelayAsync(TimeSpan.FromSeconds(10), cancellation.Token);
            var e = await AssertEx.ThrowsAsync<TaskTimeoutException>(async () =>
                await task.CfAwait(TimeSpan.FromSeconds(1), cancellation));

            Assert.That(e.Task, Is.SameAs(task));

            // times out before completion
            var elapsed = stopwatch.Elapsed;
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));

            // cancellation has been cancelled
            // if the original task supports cancellation, it will cancel
            Assert.That(cancellation.IsCancellationRequested, Is.True);
        }

        [Test]
        public async Task TaskWithTokenCancels()
        {
            var tokenSource = new CancellationTokenSource();

            var stopwatch = Stopwatch.StartNew();

            // awaited task with timeout, which times out before completion, throws
            // the cancellation is linked to the original cancellation token
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token);
            var task = DelayAsync(TimeSpan.FromSeconds(10), cancellation.Token);
            var t = AssertEx.ThrowsAsync<OperationCanceledException>(async () =>
                await task.CfAwait(TimeSpan.FromSeconds(1), cancellation));

            await Task.Delay(100, CancellationToken.None);
            tokenSource.Cancel();

            var e = await t;

            // cancels before completion
            var elapsed = stopwatch.Elapsed;
            Assert.That(elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));

            // exception was the operation canceled exception we expected
            // it has been observed (since we caught it) so nothing leaks
        }

        // ----

        [Test]
        public async Task AwaitExceptionThrows()
        {
            var task = ThrowAsync();

            await AssertEx.ThrowsAsync<Exception>(async () =>
            {
                // awaiting the task throws
                await task.CfAwait();
            });
        }

        [Test]
        public async Task AwaitExceptionWithNoThrowDoesNotThrow()
        {
            var task = ThrowAsync();

            // awaiting the task with NoThrow does not throw
            await task.CfAwaitNoThrow();

            // still, the task is faulted
            Assert.That(task.IsFaulted, Is.True);

            //var x = task.Exception; // is this observing?
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ConfigureAwaitTaskOfTImplementation(bool configureAwait)
        {
            var task = Task.Run(() => 42);
            Assert.That(task, Is.InstanceOf<Task<int>>());

            var configured = task.ConfigureAwait(configureAwait);
            Assert.That(GetContinueOnCapturedContext(configured), Is.EqualTo(configureAwait));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ConfigureAwaitValueTaskOfTImplementation(bool configureAwait)
        {
            var task = new ValueTask<int>(42);
            Assert.That(task, Is.InstanceOf<ValueTask<int>>());

            var configured = task.ConfigureAwait(configureAwait);
            Assert.That(GetContinueOnCapturedContext(configured), Is.EqualTo(configureAwait));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ConfigureAwaitTaskImplementation(bool configureAwait)
        {
            var task = Task.Run(() => Task.CompletedTask);
            Assert.That(task, Is.InstanceOf<Task>());

            var configured = task.ConfigureAwait(configureAwait);
            Assert.That(GetContinueOnCapturedContext(configured), Is.EqualTo(configureAwait));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ConfigureAwaitValueTaskImplementation(bool configureAwait)
        {
            var task = new ValueTask();
            Assert.That(task, Is.InstanceOf<ValueTask>());

            var configured = task.ConfigureAwait(configureAwait);
            Assert.That(GetContinueOnCapturedContext(configured), Is.EqualTo(configureAwait));
        }

        [Test]
        public void ConfigureAwaitTaskOfT()
        {
            var task = Task.Run(() => 42);
            Assert.That(task, Is.InstanceOf<Task<int>>());

            var configured = task.CfAwait();
            Assert.That(GetContinueOnCapturedContext(configured), Is.False);
        }

        [Test]
        public void ConfigureAwaitValueTaskOfT()
        {
            var task = new ValueTask<int>(42);
            Assert.That(task, Is.InstanceOf<ValueTask<int>>());

            var configured = task.CfAwait();
            Assert.That(GetContinueOnCapturedContext(configured), Is.False);
        }

        [Test]
        public void ConfigureAwaitTask()
        {
            var task = Task.Run(() => Task.CompletedTask);
            Assert.That(task, Is.InstanceOf<Task>());

            var configured = task.CfAwait();
            Assert.That(GetContinueOnCapturedContext(configured), Is.False);
        }

        [Test]
        public void ConfigureAwaitValueTask()
        {
            var task = new ValueTask();
            Assert.That(task, Is.InstanceOf<ValueTask>());

            var configured = task.CfAwait();
            Assert.That(GetContinueOnCapturedContext(configured), Is.False);
        }

        private static bool GetContinueOnCapturedContext<TResult>(ConfiguredTaskAwaitable<TResult> task)
            => GetTaskContinueOnCapturedContext(task);

        private static bool GetContinueOnCapturedContext(ConfiguredTaskAwaitable task)
            => GetTaskContinueOnCapturedContext(task);

        private static bool GetContinueOnCapturedContext<TResult>(ConfiguredValueTaskAwaitable<TResult> task)
            => GetValueTaskContinueOnCapturedContext(task);

        private static bool GetContinueOnCapturedContext(ConfiguredValueTaskAwaitable task)
            => GetValueTaskContinueOnCapturedContext(task);

        private static bool GetTaskContinueOnCapturedContext(object awaitable)
        {
            var configuredTaskAwaiterField = awaitable.GetType().GetField("m_configuredTaskAwaiter", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(configuredTaskAwaiterField, Is.Not.Null);
            var configuredTaskAwaiter = configuredTaskAwaiterField.GetValue(awaitable);

            var continueOnCapturedContextField = configuredTaskAwaiter.GetType().GetField("m_continueOnCapturedContext", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(continueOnCapturedContextField, Is.Not.Null);
            var continueOnCapturedContext = (bool)continueOnCapturedContextField.GetValue(configuredTaskAwaiter);

            return continueOnCapturedContext;
        }

        private static bool GetValueTaskContinueOnCapturedContext(object awaitable)
        {
            var valueField = awaitable.GetType().GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(valueField, Is.Not.Null);
            var value = valueField.GetValue(awaitable);

            var continueOnCapturedContextField = value.GetType().GetField("_continueOnCapturedContext", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(continueOnCapturedContextField, Is.Not.Null);
            var continueOnCapturedContext = (bool) continueOnCapturedContextField.GetValue(value);

            return continueOnCapturedContext;
        }

        [Test]
        public void ArgumentExceptions()
        {
            Task task = null;
            Assert.Throws<NullReferenceException>(() => task.CfAwait());

            Task<int> taskOfT = null;
            Assert.Throws<NullReferenceException>(() => taskOfT.CfAwait());
        }
    }
}
