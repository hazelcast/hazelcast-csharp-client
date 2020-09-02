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
                    catch (AssertionException e)
                    {
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
                        await action().CAF();
                        break;
                    }
                    catch (AssertionException e)
                    {
                        caught = e;
                    }
                }

                if (stopwatch.ElapsedMilliseconds > delayMilliseconds - pollingMilliseconds)
                    throw new Exception($"Action is still failing after {delayMilliseconds}ms.", caught);

                await Task.Delay(pollingMilliseconds);
            }
        }
    }
}
