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
    /// <summary>
    /// Represents columns metadata for a <see cref="SqlRow"/>.
    /// </summary>
    public class SqlRowMetadata
    {
        private readonly IList<SqlColumnMetadata> _columns;
        private readonly Dictionary<string, int> _indexByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlRowMetadata"/> class.
        /// </summary>
        /// <param name="columns">All columns metadata.</param>
        internal SqlRowMetadata(IList<SqlColumnMetadata> columns)
        {
            _columns = columns;

            // column names are case-sensitive
            // https://github.com/hazelcast/hazelcast/issues/17080
            _indexByName = Enumerable.Range(0, _columns.Count)
                .ToDictionary(i => _columns[i].Name, i => i);
        }

        /// <summary>
        /// Gets metadata for the column at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Metadata for the column at the specified index.</returns>
        public SqlColumnMetadata this[int index] => _columns[index];

        /// <summary>
        /// Gets the index of a column identified by its name.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The index of the column matching the specified name, or -1 if no column exists with that name.</returns>
        public int GetColumnIndexByName(string name) => _indexByName.TryGetValue(name, out var index) ? index : -1;

        /// <summary>
        /// Gets all columns metadata.
        /// </summary>
        public IEnumerable<SqlColumnMetadata> Columns => _columns;
    }
}
