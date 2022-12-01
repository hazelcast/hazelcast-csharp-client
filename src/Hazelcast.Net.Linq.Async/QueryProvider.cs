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
    /// <summary>
    /// Implements IAsyncQueryProvider
    /// </summary>
    internal class QueryProvider : IAsyncQueryProvider
    {
        internal readonly ISqlService SqlService;
        private readonly string _mapName;
        private readonly QueryTranslator _translator;
        private readonly Type _rootType;

        public QueryProvider(ISqlService sqlService, string mapName, Type rootElementType)
        {
            SqlService = sqlService;
            _translator = new QueryTranslator(mapName, rootElementType);
            _mapName = mapName;
            _rootType = rootElementType;
        }

        public (Task<ISqlQueryResult>,LambdaExpression) ExecuteQuery(Expression expression)
        {
            var (sql, values,projector) = GetQuery(expression);
            var result= SqlService.ExecuteQueryAsync(sql, values);
            return (result, projector);
        }

        public (string, IReadOnlyCollection<object>, LambdaExpression) GetQuery(Expression expression)
        {
            var (sql, values, projector) = _translator.Translate(expression);
            return (sql, values, projector);
        }

        IAsyncQueryable<TElement> IAsyncQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            try
            {
                var itemType = typeof(TElement);
                var mapQ = Activator.CreateInstance(typeof(MapQuery<>).MakeGenericType(itemType),
                    this, expression)!;
                return ((IAsyncQueryable<TElement>) mapQ);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        private Type[] GetTypeArgumentsBasedKeyValueTypes(Type itemType)
        {
            var types = new Type[2];
            var keyValueTypes = _rootType.GenericTypeArguments;
            if (keyValueTypes[0] == itemType)
            {
                types[0] = itemType;
                types[1] = keyValueTypes[1]; //value
            }
            else if (keyValueTypes[1] == itemType)
            {
                types[0] = keyValueTypes[0]; //key
                types[1] = itemType;
            }
            else
            {
                types[0] = keyValueTypes[0];
                types[1] = keyValueTypes[1];
            }

            return types;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var itemType = typeof(TResult).GenericTypeArguments[0];
            return (TResult) Activator.CreateInstance(
                typeof(ObjectReader<>).MakeGenericType(itemType), this, expression)!;
        }

        public ValueTask<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
        {
            return new ValueTask<TResult>(Execute<TResult>(expression));
        }
    }
}
