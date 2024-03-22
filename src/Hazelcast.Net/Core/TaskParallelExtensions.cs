// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    internal static class TaskParallelExtensions
    {
        /// <summary>
        /// Loops on a <see cref="IEnumerable{T}"/> and execute async tasks. The method does not observe exceptions.
        /// </summary>
        /// <typeparam name="T">The type of the enumerated values.</typeparam>
        /// <param name="enumerable">The enumerated values.</param>
        /// <param name="action">The action that will be run for each element.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <param name="parallelTask">Number of concurrent tasks.</param>
        /// <remarks>
        /// <para>By default, this method runs one task per processor, i.e. <paramref name="parallelTask"/>
        /// defaults to <c>Environment.ProcessorCount</c>.</para>
        /// </remarks>
        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, CancellationToken, Task> action, CancellationToken cancellationToken = default, int parallelTask = 0)
        {
            var tasks = new List<Task>();

            using var enumerator = enumerable.GetEnumerator();
            if (parallelTask <= 0) parallelTask = Environment.ProcessorCount;

            void StartCurrent()
            {
                var currentTask = action?.Invoke(enumerator.Current, cancellationToken);

                if (currentTask != default)
                    tasks.Add(currentTask);
            }

            // start tasks
            while (tasks.Count < parallelTask && enumerator.MoveNext() && !cancellationToken.IsCancellationRequested)
                StartCurrent();

            // when a task completes, try to add next one.
            while (tasks.Count > 0)
            {
                var completed = await Task.WhenAny(tasks).CfAwait();
                tasks.Remove(completed);

                if (enumerator.MoveNext() && !cancellationToken.IsCancellationRequested)
                    StartCurrent();
            }
        }
    }
}
