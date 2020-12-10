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

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class TaskSystemExtensions
    {
        /// <summary>
        /// Observes the exception of a faulted task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A task with an observed exception.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task ObserveException(this Task task)
        {
            if (task == null) return;

            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                // observe the exception
            }
        }

        /// <summary>
        /// Observes the exception of a faulted task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A task with an observed exception.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async ValueTask ObserveException(this ValueTask task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                // observe the exception
            }
        }

        /// <summary>
        /// Observes the exception of a faulted task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A task with an observed exception.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<T> ObserveException<T>(this Task<T> task)
        {
            if (task == null) return default;

            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                // observe the exception
                return default;
            }
        }

        /// <summary>
        /// Observes a canceled task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A task that will complete when the task completes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task ObserveCanceled(this Task task)
        {
            if (task == null) return;

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // observe the exception
            }
        }
    }
}
