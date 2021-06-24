using System;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.Sql
{
    public class SqlService: ISqlService
    {
        private readonly Cluster _cluster;

        internal SqlService(Cluster cluster)
        {
            _cluster = cluster;
        }

        public async Task<SqlResult> ExecuteAsync(string sql, object[] parameters = null, SqlStatementOptions options = null)
        {
            options ??= SqlStatementOptions.Default;

            var connection = _cluster.Members.GetRandomConnection();
            if (connection == null)
            {
                throw new HazelcastSqlException(_cluster.ClientId, SqlErrorCode.ConnectionProblem,
                    "Client is not currently connected to the cluster."
                );
            }

            var queryId = SqlQueryId.FromMemberId(_cluster.ClientId);

            throw new NotImplementedException();
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
            var response = AtomicRefSetCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
