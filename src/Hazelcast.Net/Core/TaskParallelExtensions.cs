// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    internal static class TaskParallelExtensions
    {
        /// <summary>
        /// Loop on <see cref="IEnumerable<T>"/> at parallel async. The method does not observe exceptions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action">The action will be run for each element</param>
        /// <param name="cancellationToken"></param>
        /// <param name="parallelTask">Number of tasks at parallel</param>
        /// <returns></returns>
        public static async Task ParallelForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, CancellationToken, Task> action, CancellationToken cancellationToken, int parallelTask = 4)
        {
            var tasks = new List<Task>();

            var enumerator = enumerable.GetEnumerator();

            void StartCurrent()
            {
                var currentTask = action?.Invoke(enumerator.Current, cancellationToken);

                if (currentTask != default)
                    tasks.Add(currentTask);
            }

            //Start tasks as much as possible.
            while (tasks.Count < parallelTask && enumerator.MoveNext() && !cancellationToken.IsCancellationRequested)
                StartCurrent();

            // when a tasks completes, try to add next one.
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
