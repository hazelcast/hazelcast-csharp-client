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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.Sql
{
    internal class SqlService : ISqlService
    {
        private readonly Cluster _cluster;
        private readonly SerializationService _serializationService;

        internal SqlService(Cluster cluster, SerializationService serializationService)
        {
            _cluster = cluster;
            _serializationService = serializationService;
        }

        public Task<ISqlQueryResult> ExecuteQueryAsync(string sql, object[] parameters = null, SqlStatementOptions options = null)
        {
            parameters ??= Array.Empty<object>();
            options ??= SqlStatementOptions.Default;
            var queryId = SqlQueryId.FromMemberId(_cluster.ClientId);

            return Task.FromResult<ISqlQueryResult>(
                new SqlQueryResult(
                    _serializationService,
                    FetchFirstPageAsync(queryId, sql, parameters, options),
                    () => FetchNextPageAsync(queryId, options.CursorBufferSize),
                    () => CloseAsync(queryId)
                )
            );
        }

        public Task<long> ExecuteCommandAsync(string sql, object[] parameters = null, SqlStatementOptions options = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            parameters ??= Array.Empty<object>();
            options ??= SqlStatementOptions.Default;
            var queryId = SqlQueryId.FromMemberId(_cluster.ClientId);

            return FetchUpdateCountAsync(queryId, sql, parameters, options, cancellationToken);
        }

        private async Task<SqlExecuteCodec.ResponseParameters> FetchAndValidateResponseAsync(SqlQueryId queryId,
            string sql, object[] parameters, SqlStatementOptions options, SqlResultType resultType,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = _cluster.Members.GetRandomConnection();
            if (connection == null)
            {
                throw new HazelcastSqlException(_cluster.ClientId, SqlErrorCode.ConnectionProblem,
                    "Client is not currently connected to the cluster."
                );
            }

            var serializedParameters = parameters
                .Select(p => _serializationService.ToData(p))
                .ToList(parameters.Length);

            var requestMessage = SqlExecuteCodec.EncodeRequest(
                sql,
                serializedParameters,
                (long)options.Timeout.TotalMilliseconds,
                options.CursorBufferSize,
                options.Schema,
                (byte)resultType,
                queryId
            );

            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage, cancellationToken);
            var response = SqlExecuteCodec.DecodeResponse(responseMessage);

            if (response.Error is { } sqlError)
                throw new HazelcastSqlException(sqlError);

            return response;

        }

        private async Task<(SqlRowMetadata rowMetadata, SqlPage page)> FetchFirstPageAsync(SqlQueryId queryId,
            string sql, object[] parameters, SqlStatementOptions options)
        {
            var result = await FetchAndValidateResponseAsync(queryId, sql, parameters, options, SqlResultType.Rows);
            if (result.RowMetadata == null)
            {
                throw new HazelcastSqlException(_cluster.ClientId, SqlErrorCode.Generic,
                    "Expected row set in response but got update count." // FIXME [Oleksii] review error message
                );
            }

            return (new SqlRowMetadata(result.RowMetadata), result.RowPage);
        }

        private async Task<SqlPage> FetchNextPageAsync(SqlQueryId queryId, int cursorBufferSize)
        {
            var requestMessage = SqlFetchCodec.EncodeRequest(queryId, cursorBufferSize);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage);
            var response = SqlFetchCodec.DecodeResponse(responseMessage);

            if (response.Error is { } sqlError)
                throw new HazelcastSqlException(sqlError);

            return response.RowPage;
        }

        private async Task<long> FetchUpdateCountAsync(SqlQueryId queryId,
            string sql, object[] parameters, SqlStatementOptions options,
            CancellationToken cancellationToken)
        {
            var result = await FetchAndValidateResponseAsync(queryId, sql, parameters, options, SqlResultType.UpdateCount, cancellationToken);
            if (result.RowMetadata != null)
            {
                throw new HazelcastSqlException(_cluster.ClientId, SqlErrorCode.Generic,
                    "Expected update count in response but got row set." // FIXME [Oleksii] review error message
                );
            }

            return result.UpdateCount;
        }

        private async Task CloseAsync(SqlQueryId queryId)
        {
            var requestMessage = SqlCloseCodec.EncodeRequest(queryId);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = SqlCloseCodec.DecodeResponse(responseMessage);
        }
    }
}