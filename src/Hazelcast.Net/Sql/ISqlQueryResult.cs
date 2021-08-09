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

namespace Hazelcast.Sql
{
    /// <summary>
    /// Represents SQL query (SELECT ...).
    /// Provides methods to enumerate queried rows or cancel/dispose the query.
    /// </summary>
    public interface ISqlQueryResult: IAsyncEnumerator<SqlRow>, IAsyncDisposable
    {
        /// <summary>
        /// <para>
        /// Creates a one-off <see cref="IAsyncEnumerable{T}"/> around <see cref="SqlRow"/>s in row set.
        /// </para>
        /// <para>
        /// Invoking this method again after enumeration has started, will throw <see cref="System.InvalidOperationException"/>.
        /// </para>
        /// <para>
        /// Reusing obtained enumerable may lead to unpredictable results.
        /// </para>
        /// </summary>
        /// <exception cref="ObjectDisposedException">SQL result was disposed.</exception>
        /// <exception cref="InvalidOperationException">Enumeration has already started.</exception>
        IAsyncEnumerable<SqlRow> EnumerateOnceAsync();

        /// <summary>
        /// <para>
        /// Creates a one-off <see cref="IEnumerable{T}"/> around <see cref="SqlRow"/>s in row set.
        /// </para>
        /// <para>
        /// Invoking this method again after enumeration has started, will throw <see cref="System.InvalidOperationException"/>.
        /// </para>
        /// <para>
        /// Reusing obtained enumerable may lead to unpredictable results.
        /// </para>
        /// </summary>
        /// <exception cref="ObjectDisposedException">SQL result was disposed.</exception>
        /// <exception cref="InvalidOperationException">Enumeration has already started.</exception>
        IEnumerable<SqlRow> EnumerateOnce();
    }
}