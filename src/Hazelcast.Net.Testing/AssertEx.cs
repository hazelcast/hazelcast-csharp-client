// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Hazelcast.Testing
{
    public static class AssertEx
    {
        /// <summary>
        /// Verifies that an action succeeds, eventually.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="delayMilliseconds">How long to wait.</param>
        /// <param name="pollingMilliseconds">How often to try to run the action.</param>
        /// <returns>A task that will complete when the action succeeds.</returns>
        public static async ValueTask SucceedsEventually(Action action, int delayMilliseconds, int pollingMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                Exception caught;
                using (new TestExecutionContext.IsolatedContext())
                {
                    try
                    {
                        action();
                        break;
                    }
                    catch (Exception e)
                    {
                        // catch both AssertionException thrown by NUnit assertions,
                        // and exceptions thrown by the executing code
                        caught = e;
                    }
                }

                if (stopwatch.ElapsedMilliseconds > delayMilliseconds - pollingMilliseconds)
                    throw new Exception($"Action is still failing after {delayMilliseconds}ms.", caught);

                await Task.Delay(pollingMilliseconds);
            }
        }

        /// <summary>
        /// Verifies that an action succeeds, eventually.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="delayMilliseconds">How long to wait.</param>
        /// <param name="pollingMilliseconds">How often to try to run the action.</param>
        /// <returns>A task that will complete when the action succeeds.</returns>
        public static async ValueTask SucceedsEventually(Func<ValueTask> action, int delayMilliseconds, int pollingMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                Exception caught;
                using (new TestExecutionContext.IsolatedContext())
                {
                    try
                    {
                        await action().CfAwait();
                        break;
                    }
                    catch (Exception e)
                    {
                        // catch both AssertionException thrown by NUnit assertions,
                        // and exceptions thrown by the executing code
                        caught = e;
                    }
                }

                if (stopwatch.ElapsedMilliseconds > delayMilliseconds - pollingMilliseconds)
                    throw new Exception($"Action is still failing after {delayMilliseconds}ms.", caught);

                await Task.Delay(pollingMilliseconds);
            }
        }

        /// <summary>
        /// Verifies that an async action throws a particular exception when called.
        /// </summary>
        /// <typeparam name="TException">The type of the expected exception.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>The caught exception.</returns>
        /// <remarks>
        /// </remarks>
        /// <para>The original NUnit Assert.ThrowsAsync method does sync-over-async and is prone
        /// to deadlocks, as detailed in this issue https://github.com/nunit/nunit/issues/2843
        /// which is still open as of Sept. 2020. Our own implementation works around this.</para>
        public static async ValueTask<TException> ThrowsAsync<TException>(Func<ValueTask> action)
            where TException : Exception
        {
            Exception caughtException = null;

            try
            {
                await action();
            }
            catch (TException e)
            {
                caughtException = e;
            }

            Assert.That(caughtException, Is.Not.Null, $"Expected a {typeof(TException)}, but code did not throw.");
            Assert.That(caughtException, Is.InstanceOf<TException>(), $"Expected a {typeof(TException)}, not a {caughtException.GetType()}");

            return (TException) caughtException;
        }
    }
}
