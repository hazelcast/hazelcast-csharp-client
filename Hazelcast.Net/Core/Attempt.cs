﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

#nullable enable

using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Creates instances of the <see cref="Attempt{TResult}"/> struct.
    /// </summary>
    public readonly struct Attempt
    {
        /// <summary>
        /// Represents a failed attempt.
        /// </summary>
        public static Attempt Failed { get; } = new Attempt();

        /// <summary>
        /// Creates a successful attempt with a result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value of the result.</param>
        /// <returns>A successful attempt.</returns>
        public static Attempt<TResult> Succeed<TResult>(TResult value)
            => new Attempt<TResult>(true, value);

        /// <summary>
        /// Creates a failed attempt.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value of the result.</param>
        /// <param name="exception">An optional captured exception.</param>
        /// <returns>A failed attempt.</returns>
        public static Attempt<TResult> Fail<TResult>(TResult value, Exception? exception = default)
            => new Attempt<TResult>(false, value, exception);

        /// <summary>
        /// Creates a failed attempt.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="exception">An optional captured exception.</param>
        /// <returns>A failed attempt.</returns>
        public static Attempt<TResult> Fail<TResult>(Exception? exception = default)
            => new Attempt<TResult>(false, exception: exception);
    }
}
