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

using System.Threading.Tasks;

namespace Hazelcast.Util
{
    /// <summary>
    /// Provides extension methods for the <see cref="Task"/> and <see cref="Task{T}"/> classes.
    /// </summary>
    internal static class TaskExtensions
    {
        /// <summary>
        /// Creates a continuation that observes exceptions.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="continuationOptions">Options for when the continuation is scheduled and how it behaves.</param>
        /// <returns>The continuation.</returns>
        /// <returns>
        /// <para>Observing exceptions prevents unobserved exceptions from reaching the task scheduler.</para>
        /// <para>By default, <paramref name="continuationOptions"/> is <see cref="TaskContinuationOptions.OnlyOnFaulted"/> which
        /// means that only true exceptions will be observed, but not <see cref="TaskCanceledException"/> resulting from the
        /// cancellation of the task. To observe these exceptions too, use <see cref="TaskContinuationOptions.NotOnRanToCompletion"/>.</para>
        /// </returns>
        public static Task IgnoreExceptions(this Task task, TaskContinuationOptions continuationOptions = TaskContinuationOptions.OnlyOnFaulted)
        {
            // make sure it executes synchronously (not point being asynchronous)
            continuationOptions |= TaskContinuationOptions.ExecuteSynchronously;

            // simply getting the exception observes it
            task.ContinueWith(c => { _ = c.Exception; }, continuationOptions);
            
            // return the task itself - the continuation is fire-and-forget
            return task;
        }
    }
}