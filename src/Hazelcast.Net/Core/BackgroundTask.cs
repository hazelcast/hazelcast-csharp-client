// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a background task.
    /// </summary>
    // we do not dispose tasks, generally, and we *do* dispose the cancellation
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    internal class BackgroundTask
#pragma warning restore CA1001
    {
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly object _mutex = new object();
        private bool _completed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTask"/> class.
        /// </summary>
        /// <param name="function">The work to execute.</param>
        /// <remarks>
        /// <para>The <paramref name="function"/> to execute should not throw. If it
        /// throws, then the background task ends and does not restart.</para>
        /// </remarks>
        private BackgroundTask(Func<CancellationToken, Task> function)
        {
            Task = Task.Run(async () =>
            {
                try
                {
                    await function(_cancellation.Token).CfAwait();
                }
                finally
                {
                    lock (_mutex)
                    {
                        _completed = true;
                        _cancellation.Dispose();
                    }
                }
            });
        }

        /// <summary>
        /// (internal for tests only) Gets the executing task.
        /// </summary>
        internal Task Task { get; }

        /// <summary>
        /// Completes the background task.
        /// </summary>
        /// <param name="observeException">Whether to observe exceptions (or throw).</param>
        /// <returns>A task that will complete when the background task has completed.</returns>
        public async Task CompleteAsync(bool observeException)
        {
            if (observeException)
                await Task.CfAwaitNoThrow();
            else
                await Task.CfAwait();
        }

        /// <summary>
        /// Cancels the background task if not completed, and waits for completion.
        /// </summary>
        /// <param name="observeException">Whether to observe exceptions (or throw).</param>
        /// <returns>A task that will complete when the background task has completed.</returns>
        public async Task CompletedOrCancelAsync(bool observeException = false)
        {
            lock (_mutex)
            {
#pragma warning disable CA1849 // Call async methods when in an async method, TODO: Revisit Cancel usage in lock
                if (!_completed) _cancellation.Cancel();
#pragma warning restore CA1849 // Call async methods when in an async method
            }

            if (observeException)
                await Task.CfAwaitNoThrow();
            else
                await Task.CfAwaitCanceled();
        }

        /// <summary>
        /// Runs the specified work as a background task.
        /// </summary>
        /// <param name="function">The work to execute in a background task.</param>
        /// <returns>A background task executing the work.</returns>
        /// <remarks>
        /// <para>The <paramref name="function"/> to execute should not throw. If it
        /// throws, then the background task ends and does not restart.</para>
        /// </remarks>
        public static BackgroundTask Run(Func<CancellationToken, Task> function)
            => new BackgroundTask(function);
    }
}
