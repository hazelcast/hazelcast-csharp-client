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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Sql
{
    internal class SqlPageEnumerator: IEnumerator<SqlRow>
    {
        private readonly SerializationService _serializationService;
        private readonly SqlRowMetadata _rowMetadata;
        private readonly SqlPage _page;

        private int _currentIndex;
        private Lazy<SqlRow> _currentLazy;

        public SqlRow Current => _currentLazy?.Value;
        public bool IsLastPage => _page.IsLast;

        public SqlPageEnumerator(SerializationService serializationService, SqlRowMetadata rowMetadata, SqlPage page)
        {
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _rowMetadata = rowMetadata ?? throw new ArgumentNullException(nameof(rowMetadata));
            _page = page ?? throw new ArgumentNullException(nameof(page));

            ((IEnumerator)this).Reset();
        }

        public bool MoveNext()
        {
            _currentIndex++;
            if (_currentIndex >= _page.RowCount)
                return false;

            var currentIndex = _currentIndex;
            _currentLazy = new Lazy<SqlRow>(() => BuildRow(currentIndex));
            return true;
        }

        private SqlRow BuildRow(int rowIndex)
        {
            var values = Enumerable.Range(0, _page.ColumnCount)
                .Select(colIndex => _serializationService.ToObject(_page[rowIndex, colIndex]))
                .ToList(_page.RowCount);

            return new SqlRow(values, _rowMetadata);
        }

        object IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            _currentIndex = -1;
            _currentLazy = null;
        }

        void IDisposable.Dispose()
        { }
    }
}