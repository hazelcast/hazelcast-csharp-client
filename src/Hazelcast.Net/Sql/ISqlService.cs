using System.Threading.Tasks;

namespace Hazelcast.Sql
{
    public interface ISqlService
    {
        Task<SqlResult> ExecuteAsync(string sql, object[] parameters = null, SqlStatementOptions options = null);
        Task<SqlPage> FetchAsync(SqlQueryId queryId, int cursorBufferSize);
        Task CloseAsync(SqlQueryId queryId);
    }
}
