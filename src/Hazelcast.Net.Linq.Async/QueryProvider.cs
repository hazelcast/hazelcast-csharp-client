// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Linq.Evaluation;
using Hazelcast.Sql;

namespace Hazelcast.Linq
{
    internal class QueryProvider : IAsyncQueryProvider
    {
        internal readonly ISqlService SqlService;
        private readonly string _mapName;
        private readonly QueryTranslator _translator;

        public QueryProvider(ISqlService sqlService, string mapName)
        {
            SqlService = sqlService;
            _translator = new QueryTranslator(mapName);
            _mapName = mapName;
        }

        public Task<ISqlQueryResult> ExecuteQuery(Expression expression)
        {
            var (sql, values) = this.GetQuery(expression);
            return this.SqlService.ExecuteQueryAsync(sql, values);
        }

        public (string, IReadOnlyCollection<object>) GetQuery(Expression expression)
        {
            var (sql, values) = _translator.Translate(ExpressionEvaluator.EvaluatePartially(expression));
            return (sql, values);
        }

        IAsyncQueryable<TElement> IAsyncQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            try
            {
                var itemType = typeof(TElement);
                var mapQ = Activator.CreateInstance(typeof(MapQuery<,>).MakeGenericType(itemType.GenericTypeArguments),
                    new object[] {this, expression})!;

                return ((IAsyncQueryable<TElement>) mapQ);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var itemType = typeof(TResult);
            var keyValueType = itemType.GenericTypeArguments[0].GetTypeInfo().GetGenericArguments();
            return (TResult) Activator.CreateInstance(
                typeof(ObjectReader<,>).MakeGenericType(keyValueType), new object[] {this, expression})!;
        }

        public ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            return new ValueTask<TResult>(this.Execute<TResult>(expression));
        }
    }
}
