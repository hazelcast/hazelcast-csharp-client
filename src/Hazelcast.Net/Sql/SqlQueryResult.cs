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
    internal class SqlQueryResult : SqlResult, ISqlQueryResult
    {
        private readonly SerializationService _serializationService;
        private readonly Task _initTask;
        private readonly Func<Task<SqlPage>> _nextFunc;

        private SqlRowMetadata _rowMetadata;
        private SqlPageEnumerator _pageEnumerator;

        private bool _enumerationStarted;

        public SqlRow Current => _pageEnumerator?.Current;

        internal SqlQueryResult(
            SerializationService serializationService,
            Task<(SqlRowMetadata rowMetadata, SqlPage page)> initTask,
            Func<Task<SqlPage>> nextFunc,
            Func<Task> closeAction) : base(closeAction)
        {
            _serializationService = serializationService;
            _initTask = initTask.ContinueWith(InitFromTaskAsync).Unwrap();
            _nextFunc = nextFunc;
        }

        private async Task InitFromTaskAsync(Task<(SqlRowMetadata rowMetadata, SqlPage page)> initTask)
        {
            var (rowMetadata, page) = await initTask; // Ensure task succeeded or forward exception
            _rowMetadata = rowMetadata;
            UpdateCurrentPage(page);
        }

        private void UpdateCurrentPage(SqlPage page) => _pageEnumerator = new SqlPageEnumerator(_serializationService, _rowMetadata, page);

        protected override bool CloseRequired => !(_pageEnumerator is { IsLastPage: true });

        public override async ValueTask DisposeAsync()
        {
            if (!_initTask.IsCompleted) // mark possible Query Cancelled exception as observed
            {
                _ = _initTask.ContinueWith(t => t.Exception?.Handle(
                    e => e is HazelcastSqlException { ErrorCode: (int)SqlErrorCode.CancelledByUser }
                ));
            }

            await base.DisposeAsync();
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            _enumerationStarted = true;

            await _initTask.CfAwait();

            if (_pageEnumerator.MoveNext())
                return true;

            if (_pageEnumerator.IsLastPage)
                return false;

            // do not try to continue enumeration if disposed(ing)
            ThrowIfDisposed();

            var page = await _nextFunc().CfAwait();
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

            ThrowIfDisposed();
            ThrowIfEnumerationNotStarted();
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

            ThrowIfDisposed();
            ThrowIfEnumerationNotStarted();
            return Enumerate();
        }

        private void ThrowIfEnumerationNotStarted()
        {
            if (_enumerationStarted)
                throw new InvalidOperationException("Result enumeration already started");
        }
    }
}
