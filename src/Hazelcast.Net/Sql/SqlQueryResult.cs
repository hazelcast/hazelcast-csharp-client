// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Sql
{
    /// <inheritdoc cref="ISqlQueryResult"/>
    internal class SqlQueryResult : SqlResult, ISqlQueryResult,
        IAsyncEnumerator<SqlRow>
    {
        private readonly SerializationService _serializationService;
        private readonly Func<CancellationToken, Task<(SqlRowMetadata rowMetadata, SqlPage page)>> _initFunc;
        private readonly Func<CancellationToken, Task<SqlPage>> _nextFunc;

        private SqlRowMetadata _rowMetadata;
        private SqlPageEnumerator _pageEnumerator;

        private bool _initStarted;
        private bool _initFinished;

        public SqlRow Current => _pageEnumerator?.Current;

        internal SqlQueryResult(
            SerializationService serializationService,
            Func<CancellationToken, Task<(SqlRowMetadata rowMetadata, SqlPage page)>> initFunc,
            Func<CancellationToken, Task<SqlPage>> nextFunc,
            Func<Task> closeAction) : base(closeAction)
        {
            _serializationService = serializationService;
            _initFunc = initFunc;
            _nextFunc = nextFunc;
        }

        private async ValueTask InitAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_initFinished) return;
            _initStarted = true;

            var (rowMetadata, page) = await _initFunc(cancellationToken);
            _rowMetadata = rowMetadata;
            UpdateCurrentPage(page);

            _initFinished = true;
        }

        private void UpdateCurrentPage(SqlPage page) => _pageEnumerator = new SqlPageEnumerator(_serializationService, _rowMetadata, page);

        // Require closing if we have sent any request and didn't fetch last page in query
        protected override bool CloseRequired => _initStarted && !(_pageEnumerator is { IsLastPage: true });

        ValueTask<bool> IAsyncEnumerator<SqlRow>.MoveNextAsync() => MoveNextAsync(default);

        private async ValueTask<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            await InitAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            if (_pageEnumerator.MoveNext())
                return true;

            if (_pageEnumerator.IsLastPage)
                return false;

            var page = await _nextFunc(cancellationToken).CfAwait();
            UpdateCurrentPage(page);

            return _pageEnumerator.MoveNext();
        }

        IAsyncEnumerator<SqlRow> IAsyncEnumerable<SqlRow>.GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            // separate method is needed to make this one throw on invocation
            // otherwise it will only throw when enumeration has started (first MoveNextAsync is called)
            // CancellationToken will be forwarded from GetAsyncEnumerator by .NET "magic", so no need to pass it as a parameter
            return EnumerateInternal(CancellationToken.None).GetAsyncEnumerator(cancellationToken);
        }

        private async IAsyncEnumerable<SqlRow> EnumerateInternal([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (await MoveNextAsync(cancellationToken).CfAwait())
                yield return Current;
        }
    }
}
