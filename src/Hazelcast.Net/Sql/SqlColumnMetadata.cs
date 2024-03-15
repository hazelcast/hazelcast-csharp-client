// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Sql
{
    /// <summary>
    /// SQL column metadata.
    /// </summary>
    public class SqlColumnMetadata
    {
        private static readonly char[] Quotes = {'"', '\''};

        /// <summary>
        /// Column name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Column type.
        /// </summary>
        public SqlColumnType Type { get; }

        /// <summary>
        /// Column nullability. If true, the column values can be null.
        /// </summary>
        public bool IsNullable { get; }

        public SqlColumnMetadata(string name, SqlColumnType type, bool isNullable)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = name.Trim(Quotes);
            Type = type;
            IsNullable = isNullable;
        }

        public override string ToString() => $"{Name} {Type}";
    }
}