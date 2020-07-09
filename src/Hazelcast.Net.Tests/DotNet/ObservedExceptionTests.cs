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
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class ObservedExceptionTests
    {
        private int _count;

        [Test]
        public async Task ObservedNonExceptionIsTransparent()
        {
            var i = await Task.Run(() => 42).ObserveException();
            Assert.That(i, Is.EqualTo(42));
        }

        [Test]
        public void UnobservedExceptionThrowsAtAwait()
        {
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await ThrowAsync();
            });
        }

        [Test]
        public async Task ObservedExceptionDoesNotThrowAtAwait()
        {
            var task = ThrowAsync().ObserveException();
            await task;

            Assert.That(task.IsFaulted, Is.False); // and it's not even faulted
        }

        [Test]
        public void UnobservedExceptionIsRaised()
        {
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            _count = 0;

            try
            {
                RunTask(false);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            finally
            {
                TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            }

            Assert.That(_count, Is.EqualTo(1));
        }

        [Test]
        public void ObservedExceptionIsNotRaised()
        {
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            _count = 0;

            try
            {
                RunTask(true);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            finally
            {
                TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            }

            Assert.That(_count, Is.EqualTo(0));
        }

        private void RunTask(bool observe)
        {
            // run the task - by running in a method we ensure that the task variable
            // will be really out of scope and finalize-able when the GC runs, even in
            // DEBUG mode

            var task = ThrowAsync();
            if (observe) task = task.ObserveException();

            // wait for the task to complete
            //while (!task.IsCompleted) Thread.Sleep(100);
            ((IAsyncResult)task).AsyncWaitHandle.WaitOne(); // does not await/throw
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            Console.WriteLine("Unobserved exception: ");// + args.Exception.Flatten().InnerException.Message);
            _count++;
            args.SetObserved();
        }

        public async Task ThrowAsync()
        {
            await Task.Yield();
            throw new Exception("bang!");
        }
    }
}
