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

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides extension methods for the <see cref="Task"/> class.
    /// </summary>
    internal static class TaskExtensions
    {
        /// <summary>
        /// Determines whether the task ran to successful completion.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>true if the task ran to successful completion, otherwise false.</returns>
        /// <remarks>
        /// <para>Built-in task.IsCompletedSuccessfully property is introduced in netstandard2.1.</para>
        /// </remarks>
        public static bool IsCompletedSuccessfully(this Task task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
#if NET462 || NETSTANDARD2_0
            return task.IsCompleted && !(task.IsFaulted || task.IsCanceled);
#endif
#if NETSTANDARD2_1
            return task.IsCompletedSuccessfully;
#endif
        }

        /// <inheritdoc cref="IsCompletedSuccessfully(Task)"/>>
        public static bool IsCompletedSuccessfully(this ValueTask task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
#if NET462 || NETSTANDARD2_0
            return task.IsCompleted && !(task.IsFaulted || task.IsCanceled);
#endif
#if NETSTANDARD2_1
            return task.IsCompletedSuccessfully;
#endif
        }

        /// <inheritdoc cref="IsCompletedSuccessfully(Task)"/>>
        public static bool IsCompletedSuccessfully<T>(this ValueTask<T> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
#if NET462 || NETSTANDARD2_0
            return task.IsCompleted && !(task.IsFaulted || task.IsCanceled);
#endif
#if NETSTANDARD2_1
            return task.IsCompletedSuccessfully;
#endif
        }
    }
}
