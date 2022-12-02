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
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Sql;

namespace Hazelcast.Linq
{
    // Implements IAsyncEnumerator and reads result.
    internal class ObjectReader<TResult> : IAsyncEnumerator<TResult>, IAsyncDisposable
    {
        private readonly QueryProvider _queryProvider;
        private IAsyncEnumerator<SqlRow>? _enumerator;
        private ISqlQueryResult? _queryResult;
        private readonly Expression _expression;
        private Func<SqlRow, TResult>? _projector;

        public ObjectReader(QueryProvider queryProvider, Expression expression)
        {
            _queryProvider = queryProvider;
            _expression = expression;
        }

        public TResult Current { get; private set; }

        public ValueTask DisposeAsync()
        {
            if (_queryResult is not null)
                return _queryResult.DisposeAsync();

            return default;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_enumerator is null)
            {
                // execute sql
                var (result, projector) = _queryProvider.ExecuteQuery(_expression);
                _queryResult = await result.CfAwait();
                _projector = (Func<SqlRow, TResult>) projector.Compile();
                _enumerator = _queryResult.GetAsyncEnumerator();
            }

            if (await _enumerator.MoveNextAsync().CfAwait())
            {
                Current = _projector!(_enumerator.Current);
                return true;
            }

            return false;
        }
    }
}
