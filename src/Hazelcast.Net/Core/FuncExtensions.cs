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
        /// <remarks>
        /// <para>If one entry of the delegate throws, execution stops and the remaining entries are ignored.</para>
        /// </remarks>
        public static ValueTask AwaitEach(this Func<ValueTask> function)
        {
            if (function == null) return default;
            
            // optimize for the (most common) case where there is only 1 handler
            // and then this method can synchronously return the unique ValueTask,
            // only enter an async state machine if there are more than 1 handler
            
            // TODO: implement this optimization for all AwaitEach overloads

            var functions = function.GetInvocationList();
            return functions.Length == 1
                ? ((Func<ValueTask>) functions[0])()
                : AwaitEachAsync(functions);

            static async ValueTask AwaitEachAsync(Delegate[] delegates)
            {
                foreach (var d in delegates)
                {
                    await ((Func<ValueTask>) d)().CfAwait();
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
        /// <remarks>
        /// <para>If one entry of the delegate throws, execution stops and the remaining entries are ignored.</para>
        /// </remarks>
        public static async ValueTask AwaitEach<T>(this Func<T, ValueTask> function, T arg)
        {
            if (function != null)
            {
                foreach (var d in function.GetInvocationList())
                {
                    await ((Func<T, ValueTask>) d)(arg).CfAwait();
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
        /// <remarks>
        /// <para>If one entry of the delegate throws, execution stops and the remaining entries are ignored.</para>
        /// </remarks>
        public static async ValueTask AwaitEach<T1, T2>(this Func<T1, T2,ValueTask> function, T1 arg1, T2 arg2)
        {
            if (function != null)
            {
                foreach (var d in function.GetInvocationList())
                {
                    await ((Func<T1, T2, ValueTask>) d)(arg1, arg2).CfAwait();
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
        /// <remarks>
        /// <para>If one entry of the delegate throws, execution stops and the remaining entries are ignored.</para>
        /// </remarks>
        public static async ValueTask AwaitEach<T1, T2, T3>(this Func<T1, T2, T3, ValueTask> function, T1 arg1, T2 arg2, T3 arg3)
        {
            if (function != null)
            {
                foreach (var d in function.GetInvocationList())
                {
                    await ((Func<T1, T2, T3, ValueTask>) d)(arg1, arg2, arg3).CfAwait();
                }
            }
        }

        /// <summary>
        /// Awaits each entries of an async function delegate.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument.</typeparam>
        /// <typeparam name="T2">The type of the second argument.</typeparam>
        /// <typeparam name="T3">The type of the third argument.</typeparam>
        /// <typeparam name="T4">The type of the fourth argument.</typeparam>
        /// <param name="function">The function delegate.</param>
        /// <param name="arg1">The first argument for the function.</param>
        /// <param name="arg2">The second argument for the function.</param>
        /// <param name="arg3">The third argument for the function.</param>
        /// <param name="arg4">The fourth argument for the function.</param>
        /// <returns>A task that will complete when each entries in <paramref name="function"/>
        /// has been executed and awaited.</returns>
        /// <remarks>
        /// <para>If one entry of the delegate throws, execution stops and the remaining entries are ignored.</para>
        /// </remarks>
        public static async ValueTask AwaitEach<T1, T2, T3, T4>(this Func<T1, T2, T3, T4, ValueTask> function, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (function != null)
            {
                foreach (var d in function.GetInvocationList())
                {
                    await ((Func<T1, T2, T3, T4, ValueTask>)d)(arg1, arg2, arg3, arg4).CfAwait();
                }
            }
        }

        /// <summary>
        /// Awaits each entries of an async function delegate.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument.</typeparam>
        /// <typeparam name="T2">The type of the second argument.</typeparam>
        /// <typeparam name="T3">The type of the third argument.</typeparam>
        /// <typeparam name="T4">The type of the fourth argument.</typeparam>
        /// <typeparam name="T5">The type of the fifth argument.</typeparam>
        /// <param name="function">The function delegate.</param>
        /// <param name="arg1">The first argument for the function.</param>
        /// <param name="arg2">The second argument for the function.</param>
        /// <param name="arg3">The third argument for the function.</param>
        /// <param name="arg4">The fourth argument for the function.</param>
        /// <param name="arg5">The fifth argument for the function.</param>
        /// <returns>A task that will complete when each entries in <paramref name="function"/>
        /// has been executed and awaited.</returns>
        /// <remarks>
        /// <para>If one entry of the delegate throws, execution stops and the remaining entries are ignored.</para>
        /// </remarks>
        public static async ValueTask AwaitEach<T1, T2, T3, T4, T5>(this Func<T1, T2, T3, T4, T5, ValueTask> function, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (function != null)
            {
                foreach (var d in function.GetInvocationList())
                {
                    await ((Func<T1, T2, T3, T4, T5,ValueTask>)d)(arg1, arg2, arg3, arg4, arg5).CfAwait();
                }
            }
        }
    }
}
