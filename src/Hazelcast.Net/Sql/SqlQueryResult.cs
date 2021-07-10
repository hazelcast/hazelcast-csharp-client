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
    internal class SqlQueryResult : ISqlQueryResult
    {
        private readonly SerializationService _serializationService;
        private readonly Task _initTask;
        private readonly Func<Task<SqlPage>> _fetchNextFunc;
        private readonly Func<Task> _closeFunc;

        private SqlRowMetadata _rowMetadata;
        private SqlPageEnumerator _pageEnumerator;

        private bool _queryClosed;
        private bool _disposed;

        public SqlRow Current => _pageEnumerator?.Current;

        internal SqlQueryResult(
            SerializationService serializationService,
            Task<(SqlRowMetadata rowMetadata, SqlPage page)> fetchFirstTask,
            Func<Task<SqlPage>> fetchNextFunc,
            Func<Task> closeFunc)
        {
            _serializationService = serializationService;
            _initTask = fetchFirstTask.ContinueWith(InitFromTaskAsync).Unwrap();
            _fetchNextFunc = fetchNextFunc;
            _closeFunc = closeFunc;
        }

        private async Task InitFromTaskAsync(Task<(SqlRowMetadata rowMetadata, SqlPage page)> fetchFirstTask)
        {
            var (rowMetadata, page) = await fetchFirstTask; // Ensure task succeeded or forward exception

            _rowMetadata = rowMetadata;
            UpdateCurrentPage(page);
        }

        private void UpdateCurrentPage(SqlPage page)
        {
            _pageEnumerator = new SqlPageEnumerator(_serializationService, _rowMetadata, page);
            _queryClosed = page.IsLast; // no need to close
        }

        public async ValueTask DisposeAsync()
        {
            if (!_queryClosed)
            {
                await _closeFunc().CfAwait();
                _queryClosed = true;
            }

            _disposed = true;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            await _initTask;

            EnsureCanEnumerate();

            if (_pageEnumerator.MoveNext())
                return true;

            if (_pageEnumerator.IsLastPage)
                return false;

            var page = await _fetchNextFunc().CfAwait();
            UpdateCurrentPage(page);

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
            if (_disposed)
                throw new ObjectDisposedException(nameof(SqlQueryResult));
        }
    }
}