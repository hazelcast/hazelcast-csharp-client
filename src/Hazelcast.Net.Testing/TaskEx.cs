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
using System.Linq;
using System.Threading.Tasks;

namespace Hazelcast.Testing
{
    public static class TaskEx
    {
        /// <summary>
        /// Runs <paramref name="action"/> in <paramref name="count"/> concurrent tasks.
        /// </summary>
        public static async Task RunConcurrently(Func<int, Task> action, int count)
        {
            var starter = new TaskCompletionSource<object>();
            var tasks = Enumerable.Range(0, count).Select(async i =>
            {
                await starter.Task;
                await action(i);
            });

            starter.SetResult(null);
            await Task.WhenAll(tasks);
        }

        /// <inheritdoc cref="RunConcurrently(Func{int, Task},int)"/>
        public static Task RunConcurrently(Action<int> action, int count) =>
            RunConcurrently(i =>
            {
                action(i);
                return Task.CompletedTask;
            }, count);

        /// <summary>
        /// Runs <paramref name="func"/> in <paramref name="count"/> concurrent tasks and returns all results.
        /// </summary>
        public static async Task<T[]> RunConcurrently<T>(Func<int, Task<T>> func, int count)
        {
            var starter = new TaskCompletionSource<object>();
            var tasks = Enumerable.Range(0, count).Select(async i =>
            {
                await starter.Task;
                return await func(i);
            });

            starter.SetResult(null);
            return await Task.WhenAll(tasks);
        }

        /// <inheritdoc cref="RunConcurrently{T}(Func{int, Task{T}},int)"/>
        public static Task<T[]> RunConcurrently<T>(Func<int, T> func, int count) =>
            RunConcurrently(i => Task.FromResult(func(i)), count);
    }
}
