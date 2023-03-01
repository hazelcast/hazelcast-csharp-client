// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Sql
{
    internal class SqlService : ISqlService
    {
        private readonly Cluster _cluster;
        private readonly SerializationService _serializationService;
        private readonly ReadOptimizedLruCache<string, int> _queryPartitionArgumentCache;
        private readonly ILogger<SqlService> _logger;
        private readonly HazelcastOptions _options;

        internal SqlService(HazelcastOptions options, Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _cluster = cluster;
            _serializationService = serializationService;
            _queryPartitionArgumentCache = new(options.Sql.PartitionArgumentCacheSize, options.Sql.PartitionArgumentCacheThreshold);
            _logger = loggerFactory.CreateLogger<SqlService>();
            _options = options;
        }

        internal SerializationService SerializationService => _serializationService;

        /// <inheritdoc/>
        public async Task<ISqlQueryResult> ExecuteQueryAsync(string sql, object[] parameters = null, SqlStatementOptions options = null, CancellationToken cancellationToken = default)
        {
            parameters ??= Array.Empty<object>();
            options ??= SqlStatementOptions.Default;
            var queryId = SqlQueryId.FromMemberId(_cluster.ClientId);

            SqlRowMetadata metadata;
            SqlPage firstPage;
            var partitionId = -1;

            try
            {
                (metadata, firstPage, partitionId) = await FetchFirstPageAsync(queryId, sql, parameters, options, cancellationToken).CfAwait();
            }
            catch (TaskCanceledException)
            {
                // maybe, the server is running the query, so better notify it
                // for any other exception: assume that the query did not start

                await CloseAsync(queryId).CfAwaitNoThrow(); // swallow the exception, nothing we can do really
                throw;
            }

            return new SqlQueryResult(_serializationService, metadata, firstPage, options.CursorBufferSize, FetchNextPageAsync, queryId, CloseAsync, partitionId, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<ISqlQueryResult> ExecuteQueryAsync(string sql, SqlStatementOptions options = null, CancellationToken cancellationToken = default, params object[] parameters)
        {
            return ExecuteQueryAsync(sql, parameters, options, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<long> ExecuteCommandAsync(string sql, object[] parameters = null, SqlStatementOptions options = null, CancellationToken cancellationToken = default)
        {
            parameters ??= Array.Empty<object>();
            options ??= SqlStatementOptions.Default;
            var queryId = SqlQueryId.FromMemberId(_cluster.ClientId);

            // commands self-close when returning = no need to close anything
            // and... in case token is cancelled, it's pretty much the same

            return await FetchUpdateCountAsync(queryId, sql, parameters, options, cancellationToken).CfAwait();
        }

        /// <inheritdoc/>
        public Task<long> ExecuteCommandAsync(string sql, SqlStatementOptions options = null, CancellationToken cancellationToken = default, params object[] parameters)
        {
            return ExecuteCommandAsync(sql, parameters, options, cancellationToken);
        }

        private async Task<(SqlExecuteCodec.ResponseParameters, int)> FetchAndValidateResponseAsync(SqlQueryId queryId,
            string sql, object[] parameters, SqlStatementOptions options, SqlResultType resultType,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = _cluster.Members.GetConnectionForSql();
            if (connection == null)
            {
                throw new HazelcastSqlException(_cluster.ClientId, SqlErrorCode.ConnectionProblem,
                    "Client is not currently connected to the cluster."
                );
            }

            var serializedParameters = parameters
                .Select(p => _serializationService.ToData(p))
                .ToList(parameters.Length);

            // note: skipUpdateStatistics was introduced at one point in the Java code and it's true if
            // the client connection type is MC_JAVA_CLIENT ie if the client is the MC client, and we
            // are not the MC client, we are the .NET client.

            var requestMessage = SqlExecuteCodec.EncodeRequest(
                sql,
                serializedParameters,
                (long) options.Timeout.TotalMilliseconds,
                options.CursorBufferSize,
                options.Schema,
                (byte) resultType,
                queryId,
                skipUpdateStatistics: false
            );

            var partitionId = GetPartitionIdOfQuery(sql, serializedParameters);

            var responseMessage = await _cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, partitionId, cancellationToken).CfAwait();

            var response = SqlExecuteCodec.DecodeResponse(responseMessage);

            // todo: argument index will appear here after protocol PR merged.
            SetArgumentIndex(sql, -1);

            if (response.Error != null)
                throw new HazelcastSqlException(_cluster.ClientId, response.Error);

            return (response, partitionId);
        }

        private void SetArgumentIndex(string sql, int argIndexFromServer)
        {
            if (_options.Networking.SmartRouting &&
                (!_queryPartitionArgumentCache.TryGetValue(sql, out var argIndex) || argIndexFromServer != argIndex))
            {
                if (argIndexFromServer == -1)
                    _queryPartitionArgumentCache.TryRemove(sql, out _);
                else
                    _queryPartitionArgumentCache.Add(sql, argIndexFromServer);
            }
        }

        /// <summary>
        /// Tries to find partition Id for the key if argument index exists for the statement and <see cref="NetworkingOptions.SmartRouting"/> enabled.
        /// </summary>
        /// <param name="sql">Sql Statement.</param>
        /// <param name="serializedParameters">Serialized parameters of the statement.</param>
        /// <returns>Partition Id if successful, otherwise -1</returns>
        private int GetPartitionIdOfQuery(string sql, List<IData> serializedParameters)
        {
            var partitionId = -1;

            // Todo: Update condition after TPC implementation.
            if (!_options.Networking.SmartRouting || !_queryPartitionArgumentCache.TryGetValue(sql, out var argIndex)) return partitionId;

            if (argIndex >= 0 && argIndex < serializedParameters.Count)
            {
                var serializedKey = serializedParameters[argIndex];
                partitionId = _cluster.State.Partitioner.GetPartitionId(serializedKey.PartitionHash);
            }
            else
            {
                _logger.IfDebug().LogDebug("Argument Index ({ArgIndex}) for query is out of range in parameters. SQL statement will send over a random connection.", argIndex);
            }

            return partitionId;
        }

        private async Task<(SqlRowMetadata rowMetadata, SqlPage page, int)> FetchFirstPageAsync(SqlQueryId queryId, string sql, object[] parameters, SqlStatementOptions options, CancellationToken cancellationToken)
        {
            var (result, partitionId) = await FetchAndValidateResponseAsync(queryId, sql, parameters, options, SqlResultType.Rows, cancellationToken).CfAwait();
            if (result.RowMetadata == null)
                throw new HazelcastSqlException(_cluster.ClientId, SqlErrorCode.Generic, "Expected row set in the response but got update count.");

            return (new SqlRowMetadata(result.RowMetadata), result.RowPage, partitionId);
        }

        private async Task<SqlPage> FetchNextPageAsync(SqlQueryId queryId, int cursorBufferSize, int partitionId, CancellationToken cancellationToken)
        {
            var requestMessage = SqlFetchCodec.EncodeRequest(queryId, cursorBufferSize);
            var responseMessage = await _cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, partitionId, cancellationToken).CfAwait();
            var response = SqlFetchCodec.DecodeResponse(responseMessage);

            if (response.Error != null) throw new HazelcastSqlException(_cluster.ClientId, response.Error);

            return response.RowPage;
        }

        private async Task<long> FetchUpdateCountAsync(SqlQueryId queryId, string sql, object[] parameters, SqlStatementOptions options, CancellationToken cancellationToken = default)
        {
            var (result, _) = await FetchAndValidateResponseAsync(queryId, sql, parameters, options, SqlResultType.UpdateCount, cancellationToken).CfAwait();
            if (result.RowMetadata != null)
                throw new HazelcastSqlException(_cluster.ClientId, SqlErrorCode.Generic, "Expected update count in the response but got row set.");

            return result.UpdateCount;
        }

        private async Task CloseAsync(SqlQueryId queryId)
        {
            var requestMessage = SqlCloseCodec.EncodeRequest(queryId);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            _ = SqlCloseCodec.DecodeResponse(responseMessage);
        }
    }
}
