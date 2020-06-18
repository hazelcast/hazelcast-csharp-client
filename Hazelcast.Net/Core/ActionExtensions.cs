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

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="Action"/> class.
    /// </summary>
    internal static class ActionExtensions
    {
        /// <summary>
        /// Wraps a synchronous action into an asynchronous one.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>An asynchronous action wrapping the specified synchronous action.</returns>
        public static Func<TArg1, CancellationToken, ValueTask> AsAsync<TArg1>(this Action<TArg1> action)
            => (arg1, _) =>
            {
                action(arg1);
                return default;
            };

        /// <summary>
        /// Wraps a synchronous action into an asynchronous one.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>An asynchronous action wrapping the specified synchronous action.</returns>
        public static Func<TArg1, TArg2, CancellationToken, ValueTask> AsAsync<TArg1, TArg2>(this Action<TArg1, TArg2> action)
            => (arg1, arg2, _) =>
            {
                action(arg1, arg2);
                return default;
            };
    }
}