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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Sql
{
    // FIXME [Oleksii] Clarify if should be thread safe
    public class SqlResult : IAsyncEnumerator<SqlRow>
    {
        /// <summary>
        /// If equals <see cref="UpdateCount"/>, this <see cref="SqlResult"/> contains rows data instead of count of updated rows.
        /// </summary>
        public const int NoUpdateCount = -1;

        private readonly SqlService _sqlService;
        private readonly SerializationService _serializationService;

        private readonly SqlQueryId _queryId;
        private readonly int _cursorBufferSize;
        private readonly SqlRowMetadata _rowMetadata;

        private SqlPageEnumerator _pageEnumerator;

        private bool _closed;

        public SqlRow Current => _pageEnumerator.Current;
        public long UpdateCount { get; } = NoUpdateCount;

        internal SqlResult(
            SqlService sqlService, SerializationService serializationService,
            SqlQueryId queryId, int cursorBufferSize,
            SqlRowMetadata rowMetadata, SqlPage page, long updateCount
        )
        {
            _sqlService = sqlService;
            _serializationService = serializationService;
            _queryId = queryId;
            _cursorBufferSize = cursorBufferSize;

            if (rowMetadata != null)
                _rowMetadata = rowMetadata;
            else
                UpdateCount = updateCount;

            _pageEnumerator = new SqlPageEnumerator(serializationService, rowMetadata, page);
        }

        public async ValueTask DisposeAsync()
        {
            if (_closed)
                return;

            await _sqlService.CloseAsync(_queryId);
            _closed = true;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            EnsureCanEnumerate();

            if (_pageEnumerator.MoveNext())
                return true;

            if (_pageEnumerator.IsLastPage)
                return false;

            var page = await _sqlService.FetchAsync(_queryId, _cursorBufferSize);
            _pageEnumerator = new SqlPageEnumerator(_serializationService, _rowMetadata, page);

            return _pageEnumerator.MoveNext();
        }

        public IAsyncEnumerable<SqlRow> EnumerateOnceAsync()
        {
            async IAsyncEnumerable<SqlRow> Enumerate()
            {
                while (await MoveNextAsync().CfAwait())
                    yield return Current;
            }

            EnsureCanEnumerate();
            return Enumerate();
        }

        public IEnumerable<SqlRow> EnumerateOnce()
        {
            IEnumerable<SqlRow> Enumerate()
            {
                // FIXME [Oleksii] discuss synchronous approach
                while (MoveNextAsync().GetAwaiter().GetResult())
                    yield return Current;
            }

            EnsureCanEnumerate();
            return Enumerate();
        }

        private void EnsureCanEnumerate()
        {
            if (_closed)
                throw new ObjectDisposedException(nameof(SqlResult));

            if (_rowMetadata == null)
                throw new InvalidOperationException("This result contains only update count.");
        }
    }
}