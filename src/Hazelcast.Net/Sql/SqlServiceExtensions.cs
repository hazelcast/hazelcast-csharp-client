using System.Threading.Tasks;

namespace Hazelcast.Sql
{
    public static class SqlServiceExtensions
    {
        /// <inheritdoc cref="ISqlService.ExecuteQueryAsync"/>
        public static Task<ISqlQueryResult> ExecuteQueryAsync(this ISqlService service, string sql, params object[] parameters) =>
            service.ExecuteQueryAsync(sql, parameters);
    }
}
