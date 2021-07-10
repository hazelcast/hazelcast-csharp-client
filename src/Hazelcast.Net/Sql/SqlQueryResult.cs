﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    internal class SqlQueryResult: ISqlQueryResult
    {
        private readonly SerializationService _serializationService;
        private readonly Task _initTask;
        private readonly Func<Task<SqlPage>> _fetchNextFunc;
        private readonly Func<Task> _closeFunc;

        private SqlRowMetadata _rowMetadata;
        private SqlPageEnumerator _pageEnumerator;

        private Task _disposeTask;

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

            if (page.IsLast)
                _disposeTask = Task.CompletedTask; // no need to close
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposeTask == null)
            {
                _disposeTask = _closeFunc();
                var _ =_initTask.ContinueWith(t => t.Exception); // mark possible init exception as observed
            }

            await _disposeTask; // FIXME [Oleksii] discuss that if failed, exception will be rethrown
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            await _initTask;

            if (_pageEnumerator.MoveNext())
                return true;

            if (_pageEnumerator.IsLastPage)
                return false;

            // do not try to continue enumeration if disposed(ing)
            EnsureNotDisposed();

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

            EnsureNotDisposed();
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

            EnsureNotDisposed();
            return Enumerate();
        }

        private void EnsureNotDisposed()
        {
            if (_disposeTask != null)
                throw new ObjectDisposedException(nameof(SqlQueryResult));
        }
    }
}