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

using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Sql
{
    /// <summary>
    /// Represents the Hazelcast SQL service.
    /// </summary>
    public interface ISqlService
    {
        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="sql">The SQL query text to execute.</param>
        /// <param name="parameters">Parameters for the SQL query.</param>
        /// <param name="options">Options for the SQL query (defaults to <see cref="SqlStatementOptions.Default"/>).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An <see cref="ISqlQueryResult"/> instance that represents the result of the query.</returns>
        /// <remarks>
        /// <para>The <paramref name="sql"/> query text can contain parameter placeholders, specified via a '?' character. Each
        /// occurrence of the '?' character is replaced by the next parameter from the <paramref name="parameters"/> ordered list.</para>
        /// </remarks>
        Task<ISqlQueryResult> ExecuteQueryAsync(string sql, object[] parameters = null, SqlStatementOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="sql">The SQL query text to execute.</param>        
        /// <param name="options">Options for the SQL query (defaults to <see cref="SqlStatementOptions.Default"/>).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <param name="parameters">Parameters for the SQL query.</param>
        /// <returns>An <see cref="ISqlQueryResult"/> instance that represents the result of the query.</returns>
        /// <remarks>
        /// <para>The <paramref name="sql"/> query text can contain parameter placeholders, specified via a '?' character. Each
        /// occurrence of the '?' character is replaced by the next parameter from the <paramref name="parameters"/> ordered list.</para>
        /// </remarks>
        Task<ISqlQueryResult> ExecuteQueryAsync(string sql, SqlStatementOptions options = null, CancellationToken cancellationToken = default, params object[] parameters);

        /// <summary>
        /// Executes a SQL command.
        /// </summary>
        /// <param name="sql">The SQL command text to execute.</param>
        /// <param name="parameters">Parameters for the SQL command.</param>
        /// <param name="options">Options for the SQL command (defaults to <see cref="SqlStatementOptions.Default"/>).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The number of rows affected byt the command.</returns>
        /// <remarks>
        /// <para>The <paramref name="sql"/> query text can contain parameter placeholders, specified via a '?' character. Each
        /// occurrence of the '?' character is replaced by the next parameter from the <paramref name="parameters"/> ordered list.</para>
        /// </remarks>
        Task<long> ExecuteCommandAsync(string sql, object[] parameters = null, SqlStatementOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a SQL command.
        /// </summary>
        /// <param name="sql">The SQL command text to execute.</param>        
        /// <param name="options">Options for the SQL command (defaults to <see cref="SqlStatementOptions.Default"/>).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <param name="parameters">Parameters for the SQL command.</param>
        /// <returns>The number of rows affected byt the command.</returns>
        /// <remarks>
        /// <para>The <paramref name="sql"/> query text can contain parameter placeholders, specified via a '?' character. Each
        /// occurrence of the '?' character is replaced by the next parameter from the <paramref name="parameters"/> ordered list.</para>
        /// </remarks>
        Task<long> ExecuteCommandAsync(string sql, SqlStatementOptions options = null, CancellationToken cancellationToken = default, params object[] parameters);
    }
}