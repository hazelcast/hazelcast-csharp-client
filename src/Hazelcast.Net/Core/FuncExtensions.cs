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
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="Func{TResult}"/> delegate.
    /// </summary>
    internal static class FuncExtensions
    {
        // Part 15.4 of the C# 4.0 language specification
        //
        // Invocation of a delegate instance whose invocation list contains multiple
        // entries proceeds by invoking each of the methods in the invocation list,
        // synchronously, in order. ... If the delegate invocation includes output
        // parameters or a return value, their final value will come from the invocation
        // of the last delegate in the list.
        //
        // ie only the last result of functions is returned
        // but each function or action runs

        // however, for tasks, what we really want is to run and await each entry in
        // sequence, and that cannot be done by simply invoking the delegate, we have
        // to get the invocation list and handle the calls

        /// <summary>
        /// Awaits each entries of an async function delegate.
        /// </summary>
        /// <param name="function">The function delegate.</param>
        /// <returns>A task that will complete when each entries in <paramref name="function"/>
        /// has been executed and awaited.</returns>
        public static async ValueTask AwaitEach(this Func<ValueTask> function)
        {
            if (function != null)
            {
                foreach (var d in function.GetInvocationList())
                {
                    await ((Func<ValueTask>) d)().CAF();
                }
            }
        }

        /// <summary>
        /// Awaits each entries of an async function delegate.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="function">The function delegate.</param>
        /// <param name="arg">An argument for the function.</param>
        /// <returns>A task that will complete when each entries in <paramref name="function"/>
        /// has been executed and awaited.</returns>
        public static async ValueTask AwaitEach<T>(this Func<T, ValueTask> function, T arg)
        {
            if (function != null)
            {
                foreach (var d in function.GetInvocationList())
                {
                    await ((Func<T, ValueTask>) d)(arg).CAF();
                }
            }
        }

        /// <summary>
        /// Awaits each entries of an async function delegate.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument.</typeparam>
        /// <typeparam name="T2">The type of the second argument.</typeparam>
        /// <param name="function">The function delegate.</param>
        /// <param name="arg1">The first argument for the function.</param>
        /// <param name="arg2">The second argument for the function.</param>
        /// <returns>A task that will complete when each entries in <paramref name="function"/>
        /// has been executed and awaited.</returns>
        public static async ValueTask AwaitEach<T1, T2>(this Func<T1, T2,ValueTask> function, T1 arg1, T2 arg2)
        {
            if (function != null)
            {
                foreach (var d in function.GetInvocationList())
                {
                    await ((Func<T1, T2, ValueTask>) d)(arg1, arg2).CAF();
                }
            }
        }

        /// <summary>
        /// Awaits each entries of an async function delegate.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument.</typeparam>
        /// <typeparam name="T2">The type of the second argument.</typeparam>
        /// <typeparam name="T3">The type of the third argument.</typeparam>
        /// <param name="function">The function delegate.</param>
        /// <param name="arg1">The first argument for the function.</param>
        /// <param name="arg2">The second argument for the function.</param>
        /// <param name="arg3">The third argument for the function.</param>
        /// <returns>A task that will complete when each entries in <paramref name="function"/>
        /// has been executed and awaited.</returns>
        public static async ValueTask AwaitEach<T1, T2, T3>(this Func<T1, T2, T3, ValueTask> function, T1 arg1, T2 arg2, T3 arg3)
        {
            if (function != null)
            {
                foreach (var d in function.GetInvocationList())
                {
                    await ((Func<T1, T2, T3, ValueTask>) d)(arg1, arg2, arg3).CAF();
                }
            }
        }
    }
}
