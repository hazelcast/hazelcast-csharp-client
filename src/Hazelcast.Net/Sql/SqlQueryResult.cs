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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Sql
{
    /// <inheritdoc cref="ISqlQueryResult"/>
    internal class SqlQueryResult : ISqlQueryResult
    {
        private readonly SqlQueryId _queryId;
        private readonly Func<SqlQueryId, Task> _closeQuery;
        private readonly SerializationService _serializationService;
        private readonly CancellationToken _cancellationToken;
        private readonly Func<SqlQueryId, int, CancellationToken, Task<SqlPage>> _getNextPage;
        private readonly SqlRowMetadata _metadata;
        private readonly int _cursorBufferSize;
        private CancellationTokenSource _combinedCancellation;
        private bool _disposed;

        // enumeration variables
        // having them here allows for Enumerator to be a readonly struct
        private bool _enumerateStarted;
        private SqlPage _page;
        private SqlRow _currentRow;
        private int _currentRowIndex;

        internal SqlQueryResult(
            SerializationService serializationService,
            SqlRowMetadata metadata, SqlPage firstPage,
            int cursorBufferSize,
            Func<SqlQueryId, int, CancellationToken, Task<SqlPage>> getNextPage,
            SqlQueryId queryId,
            Func<SqlQueryId, Task> closeQuery,
            CancellationToken cancellationToken)
        {
            _queryId = queryId;
            _closeQuery = closeQuery;
            _serializationService = serializationService;
            _cursorBufferSize = cursorBufferSize;
            _metadata = metadata;
            _page = firstPage;
            _getNextPage = getNextPage;
            _cancellationToken = cancellationToken;
        }

        public IAsyncEnumerator<SqlRow> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SqlQueryResult));

            // cannot enumerate more than once, this is consistent with e.g. EF
            if (_enumerateStarted) throw new InvalidOperationException("The result of a query cannot be enumerated more than once.");
            _enumerateStarted = true;

            // combine cancellation tokens if needed
            if (cancellationToken == default)
            {
                cancellationToken = _cancellationToken;
            }
            else if (_cancellationToken != default)
            {
                _combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken);
                cancellationToken = _combinedCancellation.Token;
            }

            return new Enumerator(this, cancellationToken);
        }

        private readonly struct Enumerator : IAsyncEnumerator<SqlRow>
        {
            private readonly SqlQueryResult _result;
            private readonly CancellationToken _cancellationToken;

            public Enumerator(SqlQueryResult result, CancellationToken cancellationToken)
            {
                _result = result;
                _cancellationToken = cancellationToken;
                _result._currentRowIndex = -1;
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                _cancellationToken.ThrowIfCancellationRequested();

                while (_result._page != null)
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    // no more current row
                    _result._currentRow = null;

                    // try to increment index within the current page, return if successful
                    if (++_result._currentRowIndex < _result._page.RowCount) return true;

                    // reached end of current page, if there is no further page stop enumerating
                    if (_result._page.IsLast)
                    {
                        _result._page = null;
                        return false;
                    }

                    // otherwise, try to retrieve the next page
                    _result._page = await _result._getNextPage(_result._queryId, _result._cursorBufferSize, _cancellationToken).CfAwait();
                    _result._currentRowIndex = -1;
                }

                return false;
            }

            public SqlRow Current
            {
                get
                {
                    // ensure it is valid to get the current row
                    if (_cancellationToken.IsCancellationRequested || _result._currentRowIndex < 0 || _result._currentRowIndex >= _result._page.RowCount)
                        throw new InvalidOperationException();

                    // if the current row has already been hydrated, return it
                    if (_result._currentRow != null) return _result._currentRow;

                    // otherwise, hydrate the current row, cache it, and return it
                    var columns = new List<object>(_result._page.ColumnCount);
                    for (var columnIndex = 0; columnIndex < _result._page.ColumnCount; columnIndex++)
                        columns.Add(_result._serializationService.ToObject(_result._page[_result._currentRowIndex, columnIndex]));
                    return _result._currentRow = new SqlRow(columns, _result._metadata);
                }
            }

            public ValueTask DisposeAsync()
            {
                // the enumerator is disposed by 'await foreach' and why not use this opportunity to dispose the result as well?
                return _result.DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            // if _page is null, or _page.IsLast, we have retrieved the very last page from the server, and
            // the server has closed the query, and there is nothing we need to do anymore
            if (_page == null || _page.IsLast) return;

            // otherwise, the server is still running the query and we need to close it
            try
            {
                await _closeQuery(_queryId).CfAwait();
            }
            catch
            {
                // TODO: do better
            }

            // dispose the combined cancellation if it has been created
            _combinedCancellation?.Dispose();
        }
    }
}
