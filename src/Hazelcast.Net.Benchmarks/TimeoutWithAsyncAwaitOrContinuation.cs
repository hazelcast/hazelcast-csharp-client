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
using BenchmarkDotNet.Attributes;
using Hazelcast.Core;

namespace Hazelcast.Benchmarks
{
    // the purpose of this benchmark is to compare ways to deal with task timeouts
    //
    // results (with 1000 delay and 100 timeout = times out):
    //
    // |           Method |     Mean |   Error |  StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    // |----------------- |---------:|--------:|--------:|------:|------:|------:|----------:|
    // |     WithAwaiting | 108.2 ms | 1.53 ms | 1.43 ms |     - |     - |     - |   2.92 KB |
    // | WithContinuation | 108.4 ms | 1.14 ms | 1.07 ms |     - |     - |     - |   4.02 KB |
    //
    // results (with 100 delay and 1000 timeout = does not time out):
    //
    // |           Method |     Mean |   Error |  StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    // |----------------- |---------:|--------:|--------:|------:|------:|------:|----------:|
    // |     WithAwaiting | 108.3 ms | 1.43 ms | 1.34 ms |     - |     - |     - |   1.08 KB |
    // | WithContinuation | 108.4 ms | 1.81 ms | 1.70 ms |     - |     - |     - |   1.09 KB |
    //
    // conclusion:
    //
    // although the difference is not massive, the async/await is generally better, plus the
    // implementation feels cleaner

    public class TimeoutWithAsyncAwaitOrContinuation
    {
        private const int TaskDelay = 1000; // ms
        private const int Timeout = 100; // ms

        [Benchmark]
        public async Task WithAwaiting()
        {
            try
            {
                await RunAwaiting();
            }
            catch
            { /* we know it throws */ }
        }

        [Benchmark]
        public async Task WithContinuation()
        {
            try
            {
                await RunContinuation();
            }
            catch
            { /* we know it throws */ }
        }

        public Task RunAwaiting()
        {
            return TaskEx.WithTimeout(Run, TimeSpan.FromMilliseconds(Timeout), 0);
        }

        public Task RunContinuation()
        {
            return WithTimeout(Run, TimeSpan.FromMilliseconds(Timeout), 0);
        }

        // with async/await, the timeout exception inner exception would rightfully point to Task.Delay,
        // otherwise it would of course origin the exception in WithTimeoutAlt and there is little we can do about it
        public async Task Run(CancellationToken cancellationToken)
            => await Task.Delay(TaskDelay, cancellationToken);

        //that was the original WithTimeout method that we wanted to benchmark
        public static Task WithTimeout(Func<CancellationToken, Task> function, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = AsCancellationTokenSource(timeout, defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task task;
            try
            {
                task = function(cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException(e);
            }

            return OrTimeout(task, cancellation);
        }

        private static readonly CancellationTokenSource NeverCanceledSource = new CancellationTokenSource();


        /// <summary>
        /// Converts a <see cref="TimeSpan"/> into a timeout <see cref="CancellationTokenSource"/>.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout if the time span is zero.</param>
        /// <returns>A <see cref="CancellationTokenSource"/> that will cancel once the timeout has elapsed.</returns>
        public static CancellationTokenSource AsCancellationTokenSource(TimeSpan timeSpan, int defaultTimeoutMilliseconds)
        {
            var timeout = (int)timeSpan.TotalMilliseconds;
            if (timeout < 0) return NeverCanceledSource;
            if (timeout == 0) timeout = defaultTimeoutMilliseconds;
            return new CancellationTokenSource(timeout);
        }

        /// <summary>
        /// Configures a task to handle timeouts.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cts">A cancellation token source controlling the timeout.</param>
        /// <returns>A task.</returns>
        public static Task OrTimeout(Task task, CancellationTokenSource cts)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (cts == NeverCanceledSource) return task;

            return task.ContinueWith(x =>
            {
                var notTimedOut = !x.IsCanceled;
                cts.Dispose();

                if (notTimedOut) return x;

                try
                {
                    // this is the way to get the original exception with correct stack trace
                    task.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    throw new TimeoutException("Operation timed out (see inner exception).", e);
                }

                throw new TimeoutException("Operation timed out");
            }, default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current).Unwrap();
        }

    }
}
