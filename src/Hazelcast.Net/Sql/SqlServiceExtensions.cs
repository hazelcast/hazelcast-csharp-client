using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Sql
{
    public static class SqlServiceExtensions
    {
        /// <inheritdoc cref="ISqlService.ExecuteQueryAsync"/>
        public static Task<ISqlQueryResult> ExecuteQueryAsync(this ISqlService service, string sql, params object[] parameters) =>
            service.ExecuteQueryAsync(sql, parameters);

        /// <inheritdoc cref="ISqlService.ExecuteCommandAsync"/>
        public static Task<long> ExecuteCommandAsync(this ISqlService service, string sql, CancellationToken cancellationToken) =>
            service.ExecuteCommandAsync(sql, cancellationToken);
    }
}
