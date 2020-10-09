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
    public class TaskCoreExtensionsTests
    {
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

            var configured = task.CAF();
            Assert.That(GetContinueOnCapturedContext(configured), Is.False);
        }

        [Test]
        public void ConfigureAwaitValueTaskOfT()
        {
            var task = new ValueTask<int>(42);
            Assert.That(task, Is.InstanceOf<ValueTask<int>>());

            var configured = task.CAF();
            Assert.That(GetContinueOnCapturedContext(configured), Is.False);
        }

        [Test]
        public void ConfigureAwaitTask()
        {
            var task = Task.Run(() => Task.CompletedTask);
            Assert.That(task, Is.InstanceOf<Task>());

            var configured = task.CAF();
            Assert.That(GetContinueOnCapturedContext(configured), Is.False);
        }

        [Test]
        public void ConfigureAwaitValueTask()
        {
            var task = new ValueTask();
            Assert.That(task, Is.InstanceOf<ValueTask>());

            var configured = task.CAF();
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
            Assert.Throws<ArgumentNullException>(() => task.CAF());

            Task<int> taskOfT = null;
            Assert.Throws<ArgumentNullException>(() => taskOfT.CAF());
        }

        [Test]
        public async Task ObserveTaskExceptions()
        {
            // does not alter the result
            await Task.Run(() => Task.CompletedTask).ObserveException();

            // would throw
            Assert.ThrowsAsync<Exception>(async () => await Task.Run(ThrowAsync));

            // does not throw when observed
            await Task.Run(ThrowAsync).ObserveException();
        }

        [Test]
        public async Task ObserveTaskOfTExceptions()
        {
            // does not alter the result
            var i = await Task.Run(() => 42).ObserveException();
            Assert.That(i, Is.EqualTo(42));

            // would throw
            Assert.ThrowsAsync<Exception>(async () => await Task.Run(ThrowOfTAsync));

            // does not throw when observed
            var j = await Task.Run(ThrowOfTAsync).ObserveException();
            Assert.That(j, Is.EqualTo(default(int)));
        }

        [Test]
        public void TaskTimeoutConstructor()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TaskTimeout(-1));
        }

        [Test]
        public async Task TaskTimeouts()
        {
            var timeout = new TaskTimeout(100);

            static async Task GetTask()
            {
                await Task.Delay(2000);
            }

            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await timeout.Run(GetTask()));

            var cancellation = new CancellationTokenSource();
            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await timeout.Run(GetTask(), cancellation, true));

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await ((Task) null).TimeoutAfter(100));

            await Task.Delay(100, default).TimeoutAfter(-1);
        }

        [Test]
        public async Task TaskOfTTimeouts()
        {
            var timeout = new TaskTimeout(100);

            static async Task<int> GetTask()
            {
                await Task.Delay(2000);
                return 33;
            }

            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await timeout.Run(GetTask()));

            var cancellation = new CancellationTokenSource();
            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await timeout.Run(GetTask(), cancellation, true));

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await ((Task<int>)null).TimeoutAfter(100));

            await Task.Delay(100, default).TimeoutAfter(2000);

            cancellation = new CancellationTokenSource();
            await Task.Delay(100, cancellation.Token).TimeoutAfter(2000, cancellation);

            static async Task<int> GetTask2()
            {
                await Task.Delay(100, default);
                return 33;
            }

            await GetTask2().TimeoutAfter(-1);
        }

        [Test]
        public async Task ValueTaskTimeouts()
        {
            static async ValueTask GetValueTask()
            {
                await Task.Delay(2000);
            }

            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await GetValueTask().TimeoutAfter(100));

            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await GetValueTask().TimeoutAfter(TimeSpan.FromMilliseconds(100)));
        }

        [Test]
        public async Task ValueTaskOfTTimeouts()
        {
            static async ValueTask<int> GetValueTask()
            {
                await Task.Delay(2000);
                return 33;
            }

            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await GetValueTask().TimeoutAfter(100));

            await AssertEx.ThrowsAsync<TaskTimeoutException>(async () => await GetValueTask().TimeoutAfter(TimeSpan.FromMilliseconds(100)));
        }

        private static async Task ThrowAsync()
        {
            await Task.Yield();
            throw new Exception("bang");
        }

        private static async Task<int> ThrowOfTAsync()
        {
            await Task.Yield();
            throw new Exception("bang");
        }
    }
}
