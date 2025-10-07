// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Serialization;

namespace Hazelcast.Sql
{
    /// <summary>
    /// Contains extension methods for the <see cref="ISqlService"/> interface.
    /// </summary>
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
        public static Task<ISqlQueryResult> ExecuteQueryAsync(this ISqlService service, string sql, params object[] parameters)
            => service?.ExecuteQueryAsync(sql, parameters) ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="service">The <see cref="ISqlService"/> which executes the query.</param>
        /// <param name="sql">The SQL query text to execute.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An <see cref="ISqlQueryResult"/> instance that represents the result of the query.</returns>
        public static Task<ISqlQueryResult> ExecuteQueryAsync(this ISqlService service, string sql, CancellationToken cancellationToken)
            => service?.ExecuteQueryAsync(sql, cancellationToken: cancellationToken) ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Executes a SQL query.
        /// </summary>
        /// <param name="service">The <see cref="ISqlService"/> which executes the query.</param>
        /// <param name="sql">The SQL query text to execute.</param>
        /// <param name="options">Options for the SQL query.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An <see cref="ISqlQueryResult"/> instance that represents the result of the query.</returns>
        public static Task<ISqlQueryResult> ExecuteQueryAsync(this ISqlService service, string sql, SqlStatementOptions options, CancellationToken cancellationToken = default)
            => service?.ExecuteQueryAsync(sql, options: options, cancellationToken: cancellationToken) ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Executes an SQL command.
        /// </summary>
        /// <param name="service">The <see cref="ISqlService"/> which executes the command.</param>
        /// <param name="sql">The SQL command text to execute.</param>
        /// <param name="parameters">Parameters for the SQL command.</param>
        /// <returns>The number of rows affected byt the command.</returns>
        public static Task<long> ExecuteCommandAsync(this ISqlService service, string sql, params object[] parameters)
            => service?.ExecuteCommandAsync(sql, parameters) ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Executes an SQL command.
        /// </summary>
        /// <param name="service">The <see cref="ISqlService"/> which executes the command.</param>
        /// <param name="sql">The SQL command text to execute.</param>
        /// <param name="options">Options for the SQL command.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The number of rows affected byt the command.</returns>
        public static Task<long> ExecuteCommandAsync(this ISqlService service, string sql, SqlStatementOptions options, CancellationToken cancellationToken = default)
            => service?.ExecuteCommandAsync(sql, options: options, cancellationToken: cancellationToken) ?? throw new ArgumentNullException(nameof(service));

        // TODO: finalize these methods and make them public (when doing Linq?)
        // https://docs.hazelcast.com/hazelcast/latest/sql/mapping-to-maps.html

        private static string GetMapping<T>(string name, SerializationService serializationService)
        {
            var type = typeof(T);

            // Java objects
            if (TypesMap.TryToJava<T>(out var javaType))
            {
                return $"'{name}Format' = 'java', '{name}JavaClass' = '{javaType}'";
            }

            // portable objects
            if (typeof(IPortable).IsAssignableFrom(type))
            {
                try
                {
                    var p = Activator.CreateInstance<T>() as IPortable;
                    return $"'{name}Format' = 'portable', '{name}PortableFactoryId' = '{p.FactoryId}', '{name}PortableClassId' = '{p.ClassId}'";
                    // optional: "'{name}PortableVersion' = '0'";
                }
                catch
                {
                    throw new NotSupportedException($"Failed to map C# type {type} (which is IPortable) to a Java type.");
                }
            }

            // assume compact
            var typeName = serializationService.CompactSerializer.GetTypeName(typeof (T));
            return $"'{name}Format' = 'compact', '{name}CompactTypeName' = '{typeName}'";
        }

        /// <summary>
        /// Creates an <see cref="IHMap{TKey,TValue}"/> mapping.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="sql">The SQL service.</param>
        /// <param name="map">The map.</param>
        /// <param name="columns">Expressions specifying the columns to map.</param>
        /// <returns>A <see cref="Task"/> that will complete when the mapping has been created.</returns>
        internal static Task CreateMapping<TKey, TValue>(this ISqlService sql, IHMap<TKey, TValue> map, params Expression<Func<TValue, object>>[] columns)
        {
            if (sql == null) throw new ArgumentNullException(nameof(sql));
            if (map == null) throw new ArgumentNullException(nameof(map));

            var serializationService = sql.MustBe<SqlService>().SerializationService;
            var keyMapping = GetMapping<TKey>("key", serializationService);
            var valueMapping = GetMapping<TValue>("value", serializationService);

            var command = new StringBuilder();
            command.Append("CREATE MAPPING \"");
            command.Append(map.Name);
            command.Append("\" EXTERNAL NAME \"");
            command.Append(map.Name);
            command.Append("\" ");
            if (columns != null && columns.Length > 0)
            {
                command.Append('(');
                var first = true;
                foreach (var expression in columns)
                {
                    var member = expression.Body as MemberExpression;
                    if (member == null)
                    {
                        if (expression.Body is UnaryExpression unary)
                            member = unary.Operand as MemberExpression;
                        if (member == null) throw new InvalidOperationException("Failed to parse expression.");
                    }
                    var propertyInfo = member.Member as PropertyInfo;
                    if (propertyInfo == null) throw new InvalidOperationException("Failed to parse expression (not a property?).");
                    var name = propertyInfo.Name;
                    var type = propertyInfo.PropertyType;

                    if (first) first = false;
                    else command.Append(", ");

                    // 'value' would be an illegal name and we escape it to '_value'
                    if (name.Equals("value", StringComparison.OrdinalIgnoreCase)) name = "_" + name;

                    command.Append(name);
                    command.Append(' ');
                    command.Append(TypesMap.ToSql(type));
                }
                command.Append(") ");
            }
            else
            {
                var first = true;
                foreach (var property in typeof (TValue).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!property.CanRead || !property.CanWrite || !TypesMap.TryToSql(property.PropertyType, out var sqlType))
                        continue;

                    if (first)
                    {
                        command.Append('(');
                        first = false;
                    }
                    else
                    {
                        command.Append(", ");
                    }

                    var name = property.Name;
                    if (name.Equals("value", StringComparison.OrdinalIgnoreCase)) name = "_" + name;

                    command.Append(name);
                    command.Append(' ');
                    command.Append(sqlType);

                    // TODO but for compact we are missing the 'EXTERNAL NAME' thing
                    // it should match the field name but we don't have a way to get field names?
                }
                if (!first) command.Append(") ");
            }
            command.Append("TYPE IMAP OPTIONS(");
            command.Append(keyMapping);
            command.Append(", ");
            command.Append(valueMapping);
            command.Append(')');

            Console.WriteLine(command.ToString());
            return sql.ExecuteCommandAsync(command.ToString());
        }

        /// <summary>
        /// Drops an <see cref="IHMap{TKey,TValue}"/> mapping.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="sql">The SQL service.</param>
        /// <param name="map">The map.</param>
        /// <returns>A <see cref="Task"/> that will complete when the mapping has been dropped.</returns>
        internal static Task DropMapping<TKey, TValue>(this ISqlService sql, IHMap<TKey, TValue> map)
        {
            if (sql == null) throw new ArgumentNullException(nameof(sql));
            if (map == null) throw new ArgumentNullException(nameof(map));

            var command = @$"DROP MAPPING ""{map.Name}""";
            return sql.ExecuteCommandAsync(command);
        }

        /// <summary>
        /// Gets the existing mappings.
        /// </summary>
        /// <param name="sql">The SQL service.</param>
        /// <returns>A list of existing mappings names.</returns>
        internal static async Task<IReadOnlyList<string>> ShowMappings(this ISqlService sql)
        {
            if (sql == null) throw new ArgumentNullException(nameof(sql));

            // gets 1 column named 'name' containing the name of the mapping as Varchar
            var queryResult = await sql.ExecuteQueryAsync("SHOW MAPPINGS").CfAwait();

            var mappings = new List<string>();
            await foreach (var row in queryResult)
                mappings.Add(row.GetColumn<string>(0));
            return mappings;
        }
    }
}
