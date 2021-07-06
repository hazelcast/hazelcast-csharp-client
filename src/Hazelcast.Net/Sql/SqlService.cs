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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.Sql
{
    internal class SqlService: ISqlService
    {
        private readonly Cluster _cluster;
        private readonly SerializationService _serializationService;

        internal SqlService(Cluster cluster, SerializationService serializationService)
        {
            _cluster = cluster;
            _serializationService = serializationService;
        }

        public async Task<SqlResult> ExecuteAsync(string sql, object[] parameters = null, SqlStatementOptions options = null)
        {
            parameters ??= Array.Empty<object>();
            options ??= SqlStatementOptions.Default;

            var connection = _cluster.Members.GetRandomConnection();
            if (connection == null)
            {
                throw new HazelcastSqlException(_cluster.ClientId, SqlErrorCode.ConnectionProblem,
                    "Client is not currently connected to the cluster."
                );
            }

            var queryId = SqlQueryId.FromMemberId(_cluster.ClientId);

            var serializedParameters = parameters
                .Select(p => _serializationService.ToData(p))
                .ToList();

            var requestMessage = SqlExecuteCodec.EncodeRequest(
                sql,
                serializedParameters,
                (long)options.Timeout.TotalMilliseconds,
                options.CursorBufferSize,
                options.Schema,
                (byte)options.ExpectedResultType,
                queryId
            );

            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage);
            var response = SqlExecuteCodec.DecodeResponse(responseMessage);

            if (response.Error is { } sqlError)
                throw new HazelcastSqlException(sqlError);

            // FIXME [Oleksii] discuss if throw immediately or forward to result
            return BuildResult(response, queryId, options.CursorBufferSize);
        }

        public async Task<SqlPage> FetchAsync(SqlQueryId queryId, int cursorBufferSize)
        {
            var requestMessage = SqlFetchCodec.EncodeRequest(queryId, cursorBufferSize);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage);
            var response = SqlFetchCodec.DecodeResponse(responseMessage);

            if (response.Error is { } sqlError)
                throw new HazelcastSqlException(sqlError);

            return response.RowPage;
        }

        public async Task CloseAsync(SqlQueryId queryId)
        {
            var requestMessage = SqlCloseCodec.EncodeRequest(queryId);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var response = SqlCloseCodec.DecodeResponse(responseMessage);
        }

        private SqlResult BuildResult(SqlExecuteCodec.ResponseParameters response, SqlQueryId queryId, int cursorBufferSize)
        {
            return new SqlResult(
                this, _serializationService, queryId, cursorBufferSize,
                response.RowMetadata is { } rowMetadata ? new SqlRowMetadata(rowMetadata) : null,
                response.RowPage, response.UpdateCount
            );
        }
    }
}
