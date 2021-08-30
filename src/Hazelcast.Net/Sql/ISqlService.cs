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
        // FIXME turn this into proper documentation
        //
        // how cancellation works:
        //
        // for queries:
        // - if token1 is passed to ExecuteQueryAsync, this token1 is passed over to the ISqlQueryResult
        // - the ISqlQueryResult *must* be disposed in order to close the server-side query
        // - when enumerated (await foreach) the enumerator is disposed (always)
        //   disposing the enumerator disposes the result as well
        // - ISqlQueryResult is IAsyncEnumerable<SqlRow> which means one can either
        //   - GetAsyncEnumerator(token2) on it
        //   - foreach .WithCancellation(token2) on it
        //   in both cases, it will enumerate with a token which is either token1, token2, or a combination of both
        //   if any token is cancelled, the enumeration is cancelled, and throws a OperationCancelledException
        //
        // - the actual server operations (query first page, query next page) are being passed
        //   cancellation tokens (token1, token, token1) resp.
        //
        // for commands, there is no result, so it's only:
        // - if token is passed to ExecuteCommandAsync, this token is passed to the actual server operation
        //   (execute command)
        //
        // at server operation level, the token ends up being passed to:
        // - _cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
        // - SendAsyncInternal(invocation, cancellationToken).CfAwait();
        // - connection.SendAsync(invocation).CfAwait();
        //
        // in case connection.SendAsync fails, goes invocation.WaitRetryAsync which honors the cancellationToken
        // but other than that,
        //
        //   connection.SendAsync(invocation) -> .SendAsyncInternal(CancellationToken.None)
        //   and that token
        //   - can cancel waiting on a response from the server (once the request has been sent)
        //   - is passed to _messageConnection.SendAsync(invocation.RequestMessage, cancellationToken).CfAwait();
        //     where it can cancel sending the request - but the request goes out as a whole
        //   so, as of now, we *cannot* cancel for instance getting the first page...
        //
        // if we change:
        // - connection.SendAsync(invocation, token) -> .SendAsyncInternal(token)
        // - the token will be propagated, including the SQL token
        // - we can cancel getting the first page, or running a command
        //
        // DONE => see Hazelcast.Tests.Networking.NetworkingTests.CanCancel

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
    }
}
