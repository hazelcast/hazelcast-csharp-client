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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core;

/// <summary>
/// Runs tasks in parallel.
/// </summary>
internal static class ParallelRunner 
{
    // notes
    //
    // https://stackoverflow.com/questions/72459483/run-multiple-tasks-in-parallel-and-cancel-rest-if-any-of-them-returns-false-net
    // "The Task.WhenAny-in-a-loop is generally considered an anti-pattern, because of its O(n²) complexity."
    //
    // https://stackoverflow.com/questions/43763982/how-to-use-task-whenany-and-implement-retry/43764680#43764680
    // https://stackoverflow.com/questions/72271006/task-whenany-alternative-to-list-avoiding-on%C2%B2-issues
    // https://devblogs.microsoft.com/pfxteam/processing-tasks-as-they-complete/
    //
    //
    // https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask?view=net-7.0#remarks
    // lists value tasks restrictions
    // - awaiting the instance multiple times
    // - calling AsTask multiple times
    // - using more than 1 of these techniques to consume the instance
    //
    // these restrictions are for ValueTask backed by an IValueTaskSource and don't apply to those backed by
    // an actual Task, or TResult value. In those two latter cases everything is safe. see also: Preserve().

    /// <summary>
    /// Represents options for running tasks in parallel.
    /// </summary>
    public struct Options
    {
        /// <summary>
        /// Gets or sets the number of parallel tasks.
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Runs tasks in parallel.
    /// </summary>
    /// <param name="source">A <see cref="Task"/> producer.</param>
    /// <param name="options">Options.</param>
    public static async Task Run(IEnumerable<Task> source, Options options)
    {
        using var enumerator = source.GetEnumerator();
        using var mutex = new SemaphoreSlim(1, 1);
        var completed = false;
        var count = Math.Max(1, options.Count);
        List<Exception> exceptions = null;

        HConsole.Configure(x => x.Configure(typeof(ParallelRunner)).SetPrefix("PRUNNER"));
        HConsole.WriteLine(typeof(ParallelRunner), $"Running with count={count}");

        async Task Consume(int i)
        {
            HConsole.WriteLine(typeof(ParallelRunner), $"Runner task {i} begin");

            // ReSharper disable AccessToDisposedClosure
            while (!completed)
            {
                Task task = null;

                // mutex protects the enumerator and the exceptions
                await mutex.WaitAsync().CfAwait();

                try
                {
                    if (enumerator.MoveNext()) task = enumerator.Current;
                    else completed = true;
                }
                catch (Exception e)
                {
                    // producing a the task should *not* throw but, never know
                    (exceptions ??= new List<Exception>()).Add(e);
                }
                finally
                {
                    mutex.Release();
                }

                if (task != null)
                {
                    HConsole.WriteLine(typeof(ParallelRunner), $"Runner task {i} run one");

                    try
                    {
                        await task.CfAwait();
                        HConsole.WriteLine(typeof(ParallelRunner), $"Runner task {i} completed one");
                    }
                    catch (Exception e)
                    {
                        HConsole.WriteLine(typeof(ParallelRunner), $"Runner task {i} failed one");

                        // the task *may* throw
                        await mutex.WaitAsync().CfAwait();
                        try
                        {
                            (exceptions ??= new List<Exception>()).Add(e);
                        }
                        finally
                        {
                            mutex.Release();
                        }
                    }
                }
            }
            // ReSharper restore AccessToDisposedClosure

            HConsole.WriteLine(typeof(ParallelRunner), $"Runner task {i} end");
        }

        var tasks = new List<Task>();
        for (var i = 0; i < count && !completed; i++) tasks.Add(Consume(i));
        await Task.WhenAll(tasks).CfAwait(); // consume does *not* throw

        HConsole.WriteLine(typeof(ParallelRunner), "All tasks completed" + (exceptions == null ? "" : " and exceptions were thrown"));

        if (exceptions != null) throw new AggregateException("Exceptions in parallel tasks.", exceptions);
    }
}
