using System;
using System.Threading.Tasks;

namespace Hazelcast.Sql
{
    public static class SqlServiceExtensions
    {
        // NOTE: these are convenient extension methods, and the C# compiler is clever enough to figure
        // things out between the various overloads, including the 'params object[]' overload.

        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="service">The <see cref="ISqlService"/> which executes the query.</param>
        /// <param name="sql">The SQL query text to execute.</param>
        /// <param name="parameters">Parameters for the SQL query.</param>
        /// <returns>An <see cref="ISqlQueryResult"/> instance that represents the result of the query.</returns>
        public static Task<ISqlQueryResult> ExecuteQueryAsync(this ISqlService service, string sql, params object[] parameters) =>
            service?.ExecuteQueryAsync(sql, parameters) ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="service">The <see cref="ISqlService"/> which executes the query.</param>
        /// <param name="sql">The SQL query text to execute.</param>
        /// <param name="options">Options for the SQL query.</param>
        /// <returns>An <see cref="ISqlQueryResult"/> instance that represents the result of the query.</returns>
        public static Task<ISqlQueryResult> ExecuteQueryAsync(this ISqlService service, string sql, SqlStatementOptions options) =>
            service?.ExecuteQueryAsync(sql, options: options) ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Executes an SQL command.
        /// </summary>
        /// <param name="service">The <see cref="ISqlService"/> which executes the command.</param>
        /// <param name="sql">The SQL command text to execute.</param>
        /// <param name="parameters">Parameters for the SQL command.</param>
        /// <returns>The number of rows affected byt the command.</returns>
        public static Task<long> ExecuteCommandAsync(this ISqlService service, string sql, params object[] parameters) =>
            service?.ExecuteCommandAsync(sql, parameters) ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Executes an SQL command.
        /// </summary>
        /// <param name="service">The <see cref="ISqlService"/> which executes the command.</param>
        /// <param name="sql">The SQL command text to execute.</param>
        /// <param name="options">Options for the SQL command.</param>
        /// <returns>The number of rows affected byt the command.</returns>
        public static Task<long> ExecuteCommandAsync(this ISqlService service, string sql, SqlStatementOptions options) =>
            service?.ExecuteCommandAsync(sql, options: options) ?? throw new ArgumentNullException(nameof(service));
    }
}
