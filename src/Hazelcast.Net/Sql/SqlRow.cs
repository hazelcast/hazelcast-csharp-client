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
    public class SqlRow
    {
        private const string KeyColumnName = "__key";
        private const string ValueColumnName = "this";

        private readonly IList<object> _values;

        public SqlRowMetadata Metadata { get; }

        public SqlRow(IList<object> values, SqlRowMetadata metadata)
        {
            _values = values;
            Metadata = metadata;
        }

        public T GetColumn<T>(int index) => (T)_values[index];
        public T GetColumn<T>(string name) => GetColumn<T>(Metadata.GetColumnIndexByName(name));
        public T GetKey<T>() => GetColumn<T>(KeyColumnName);
        public T GetValue<T>() => GetColumn<T>(ValueColumnName);
    }
}
