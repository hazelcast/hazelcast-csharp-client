namespace Hazelcast.Sql
{
    public static class SqlServiceExtensions
    {
        /// <inheritdoc cref="ISqlService.ExecuteQuery"/>
        public static ISqlQueryResult ExecuteQuery(this ISqlService service, string sql, params object[] parameters) =>
            service.ExecuteQuery(sql, parameters);

        /// <inheritdoc cref="ISqlService.ExecuteCommand"/>
        public static ISqlCommandResult ExecuteCommand(this ISqlService service, string sql) =>
            service.ExecuteCommand(sql, parameters: null, options: null);
    }
}
