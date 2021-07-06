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

using System.Collections.Generic;

namespace Hazelcast.Sql
{
    /// <summary>
    /// Contains SQL data returned from the returned to the client.
    /// </summary>
    internal class SqlPage
    {
        private readonly SqlColumnType[] _columnTypes;

        /// <summary>
        /// Holds returned data in this page. First index is column number, the second one is row number.
        /// </summary>
        /// <remarks>This is chosen this way because server sends SQL pages in columnar format.</remarks>
        private readonly IReadOnlyList<object>[] _data;

        public bool IsLast { get; }

        // FIXME [Oleksii] check if at least 1 row is guaranteed
        public int RowCount => _data[0].Count;

        public object this[int row, int column] => _data[column][row];

        public SqlPage(SqlColumnType[] columnTypes, IReadOnlyList<object>[] data, bool isLast)
        {
            _columnTypes = columnTypes;
            _data = data;
            IsLast = isLast;
        }
    }
}
