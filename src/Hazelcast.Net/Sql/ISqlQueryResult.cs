// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
    /// Represents the result of a SQL query (SELECT ...).
    /// </summary>
    /// <remarks>
    /// <para>The result of a SQL query is a one-off <see cref="IAsyncEnumerable{T}"/> of <see cref="SqlRow"/>s. It can be enumerated
    /// only once. Trying to iterate rows multiple times will throw <see cref="InvalidOperationException"/>.</para>
    /// <para>This class implements <see cref="IAsyncDisposable"/> and instances should be disposed when not needed in order to free
    /// server-side resources. Failing to dispose instances may impact performances on the server. Recommended way is to wrap
    /// execution in to an <c>await using</c> statement.</para>
    /// <para>This class is stateful and not thread-safe, executing its method in parallel may lead to unpredictable results.</para>
    /// </remarks>
    public interface ISqlQueryResult : IAsyncEnumerable<SqlRow>, IAsyncDisposable
    { }
}
