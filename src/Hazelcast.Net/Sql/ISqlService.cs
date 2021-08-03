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

using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Sql
{
    public interface ISqlService
    {
        /// <summary>
        /// Executes SQL query (SELECT ...).
        /// </summary>
        /// <param name="sql">SQL query to execute.</param>
        /// <param name="parameters">
        /// <para>Ordered list of parameters to be substituted in the query.</para>
        /// <para>
        /// Parameter placeholder can be specified via "?".
        /// Each occurrence of "?" will use next parameter in the list.
        /// </para>
        /// </param>
        /// <param name="options">
        /// <see cref="SqlStatementOptions"/> to use with the query.
        /// <see cref="SqlStatementOptions.Default"/> is used if not specified.
        /// </param>
        /// <returns>
        /// <see cref="ISqlQueryResult"/> representing set of rows returned from this query.
        /// </returns>
        Task<ISqlQueryResult> ExecuteQueryAsync(string sql, object[] parameters = null,
            SqlStatementOptions options = null
        );

        /// <summary>
        /// Executes SQL command (CREATE/UPDATE/DELETE ...).
        /// </summary>
        /// <param name="sql">SQL command to execute.</param>
        /// <param name="parameters">
        /// <para>Ordered list of parameters to be substituted in the query.</para>
        /// <para>
        /// Parameter placeholder can be specified via "?".
        /// Each occurrence of "?" will use next parameter in the list.
        /// </para>
        /// </param>
        /// <param name="options">
        /// <see cref="SqlStatementOptions"/> to use with the query.
        /// <see cref="SqlStatementOptions.Default"/> is used if not specified.
        /// </param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel executing query.</param>
        /// <returns><see cref="ISqlCommandResult"/> containing command execution status and affected rows count.</returns>
        Task<ISqlCommandResult> ExecuteCommandAsync(string sql, object[] parameters = null,
            SqlStatementOptions options = null, CancellationToken cancellationToken = default
        );
    }
}
