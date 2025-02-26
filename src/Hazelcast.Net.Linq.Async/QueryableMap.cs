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
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Hazelcast.Linq
{
    // It represents the queryable part of HMap.
    internal class QueryableMap<TElement> : IAsyncQueryable<TElement>, IQueryableMap
    {
        private readonly QueryProvider _queryProvider;
        private readonly Expression _expression;
        private IAsyncEnumerator<TElement>? _enumerator;

        // Called via activator at QueryProvider.
        // ReSharper disable once UnusedMember.Global
        public QueryableMap(QueryProvider provider, Expression expression, string name) : this(provider, name)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public QueryableMap(QueryProvider queryProvider, string name)
        {
            _queryProvider = queryProvider;
            _expression ??= Expression.Constant(this);
            Name = name;
        }

        /// <inheritdoc />
        public Type ElementType => typeof(TElement);

        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        /// <inheritdoc />
        public Expression Expression => _expression;

        public string Name { get; }

        /// <inheritdoc />
        public IAsyncQueryProvider Provider => _queryProvider;

        /// <inheritdoc />
        public IAsyncEnumerator<TElement> GetAsyncEnumerator(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_enumerator is not null)
                throw new InvalidOperationException("Cannot enumerate more than once.");
            
            _enumerator = _queryProvider.Execute<IAsyncEnumerator<TElement>>(_expression, token);

            return _enumerator;
        }
    }
}
