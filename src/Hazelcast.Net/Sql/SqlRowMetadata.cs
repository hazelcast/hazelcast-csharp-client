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
using System.Linq;

namespace Hazelcast.Sql
{
    public class SqlRowMetadata
    {
        private const int ColumnNotFound = -1;

        private readonly IList<SqlColumnMetadata> _columns;

        private readonly Dictionary<string, int> _indexByName;

        public SqlRowMetadata(IList<SqlColumnMetadata> columns)
        {
            _columns = columns;

            // column names are case-sensitive
            // https://github.com/hazelcast/hazelcast/issues/17080
            _indexByName = Enumerable.Range(0, _columns.Count)
                .ToDictionary(i => _columns[i].Name, i => i);
        }

        public SqlColumnMetadata this[int index] => _columns[index];
        public int GetColumnIndexByName(string name) => _indexByName[name];
        public IEnumerable<SqlColumnMetadata> Columns => _columns;
    }
}
