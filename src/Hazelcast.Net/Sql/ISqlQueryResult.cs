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
    /// <para>
    /// Represents SQL query (SELECT ...).
    /// Provides methods to enumerate queried rows or cancel/dispose the query.
    /// </para>
    /// <para>
    /// Query result serves as one-off <see cref="IAsyncEnumerable{T}"/> of <see cref="SqlRow"/>s, meaning it can be enumerated only once.
    /// Trying to iterate rows multiple times will throw <see cref="InvalidOperationException"/>.
    /// </para>
    /// <para>
    /// This object implements <see cref="IAsyncDisposable"/> and should be disposed when not needed.
    /// Recommended way is to wrap execution into <c>await using</c> statement.
    /// </para>
    /// <para>
    /// This object is stateful and not thread-safe.
    /// Executing it's method in parallel may lead to unpredictable results.
    /// </para>
    /// </summary>
    public interface ISqlQueryResult : IAsyncEnumerable<SqlRow>, IAsyncDisposable
    { }
}
