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
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Hazelcast.Testing
{
    public static class AsserterHazelcastExtensions
    {
        // TODO: document

        public static void Eventually(this Asserter asserter, Action assertAction, int timeoutSeconds = 30)
        {
            ExceptionDispatchInfo dispatch = null;
            var startTimeMillis = Clock.Milliseconds;
            var timeoutMillis = timeoutSeconds * 1000;

            while (Clock.Milliseconds - startTimeMillis < timeoutMillis)
            {
                using (new TestExecutionContext.IsolatedContext())
                {
                    try
                    {
                        assertAction();
                        return;
                    }
                    catch (AssertionException e)
                    {
                        if (dispatch == null)
                            dispatch = ExceptionDispatchInfo.Capture(e);
                        Thread.Sleep(250);
                    }
                }
            }

            // rethrow the fist exception
            dispatch?.Throw();
        }

        public static async Task Eventually(this Asserter asserter, Func<Task> assertAction, int timeoutSeconds = 30)
        {
            ExceptionDispatchInfo dispatch = null;
            var startTimeMillis = Clock.Milliseconds;
            var timeoutMillis = timeoutSeconds * 1000;

            while (Clock.Milliseconds - startTimeMillis < timeoutMillis)
            {
                using (new TestExecutionContext.IsolatedContext())
                {
                    try
                    {
                        await assertAction();
                        return;
                    }
                    catch (AssertionException e)
                    {
                        if (dispatch == null)
                            dispatch = ExceptionDispatchInfo.Capture(e);
                        await Task.Delay(250);
                    }
                }
            }

            // rethrow the fist exception
            dispatch?.Throw();
        }
    }
}