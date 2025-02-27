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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Sql;

namespace Hazelcast.Linq
{
    // Implements IAsyncEnumerator and reads result.
    internal class ObjectReader<TResult> : IAsyncEnumerator<TResult>
    {
        private readonly QueryProvider _queryProvider;
        private readonly Expression _expression;
        private IAsyncEnumerator<SqlRow>? _sqlEnumerator;
        private ISqlQueryResult? _queryResult;
        private Func<SqlRow, TResult>? _projector;
        private readonly CancellationToken _cancellationToken;

        public ObjectReader(QueryProvider queryProvider, Expression expression, CancellationToken token)
        {
            _queryProvider = queryProvider;
            _cancellationToken = token;
            _expression = expression;
            Current = default!;
        }

        /// <inheritdoc />
        public TResult Current { get; private set; }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return _queryResult?.DisposeAsync() ?? default;
        }

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            if (_sqlEnumerator is null)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                // execute sql
                var (result, projector) = _queryProvider.ExecuteQuery(_expression, token: _cancellationToken);
                _queryResult = await result.CfAwait();
                _projector = (Func<SqlRow, TResult>) projector.Compile();
                _sqlEnumerator = _queryResult.GetAsyncEnumerator(cancellationToken: _cancellationToken);
            }

            // ReSharper disable once InvertIf
            if (await _sqlEnumerator.MoveNextAsync().CfAwait())
            {
                Current = _projector!(_sqlEnumerator.Current);
                return true;
            }

            return false;
        }
    }
}
