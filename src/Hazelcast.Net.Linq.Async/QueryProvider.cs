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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Sql;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Linq
{
    /// <summary>
    /// Implements IAsyncQueryProvider
    /// </summary>
    internal class QueryProvider : IAsyncQueryProvider
    {
        private readonly ISqlService _sqlService;
        private readonly QueryTranslator _translator;
        private readonly ILogger _logger;

        public QueryProvider(ISqlService sqlService, Type rootElementType, ILoggerFactory loggerFactory)
        {
            _sqlService = sqlService ?? throw new ArgumentNullException(nameof(sqlService));
            _translator = new QueryTranslator(rootElementType);
            _logger = loggerFactory.CreateLogger<QueryProvider>();
        }

        public (Task<ISqlQueryResult>, LambdaExpression) ExecuteQuery(Expression expression, CancellationToken token = default)
        {
            var (sql, values, projector) = GetQuery(expression);
            _logger?.IfDebug()?.LogDebug("SQL Statement: \"{Sql}\"", sql);
            HConsole.WriteLine(this, sql);
            var result = _sqlService.ExecuteQueryAsync(sql, values, cancellationToken: token);
            return (result, projector);
        }

        private (string, object[], LambdaExpression) GetQuery(Expression expression)
        {
            return _translator.Translate(expression);
        }

        // Create Query is called when a new query is build existing one. So, each step may
        // do query on different type of object.
        // map.Select(...).[HERE CreateQuery Called]Where(...);
        IAsyncQueryable<TElement> IAsyncQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            try
            {
                // Note: Later parts of queryable object may not be HMap most probably,
                // since there is no join support yet. When crossing is supported, find a way to pass map name.
                var itemType = typeof(TElement);
                var mapQ = Activator.CreateInstance(typeof(QueryableMap<>)
                    .MakeGenericType(itemType), this, expression, nameof(TElement))!;
                return (IAsyncQueryable<TElement>) mapQ;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        public TResult Execute<TResult>(Expression expression, CancellationToken token = default)
        {
            try
            {
                var itemType = typeof(TResult).GenericTypeArguments[0];
                return (TResult) Activator.CreateInstance(typeof(ObjectReader<>)
                    .MakeGenericType(itemType), this, expression, token)!;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        public ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            return new ValueTask<TResult>(Execute<TResult>(expression, token));
        }
    }
}
