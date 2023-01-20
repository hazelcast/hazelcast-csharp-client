// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

#nullable enable

using System.Collections.Generic;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Builds schemas.
    /// </summary>
    internal class SchemaBuilder
    {
        private readonly string _typeName;
        private readonly List<SchemaField> _fields = new List<SchemaField>();

        private SchemaBuilder(string typeName)
        {
            _typeName = typeName;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="SchemaBuilder"/> for a compact type-name.
        /// </summary>
        /// <param name="typeName">The type-name.</param>
        /// <returns>A new <see cref="SchemaBuilder"/>.</returns>
        public static SchemaBuilder For(string typeName) => new SchemaBuilder(typeName);

        /// <summary>
        /// Adds a field to the schema.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="kind">The field kind of the field.</param>
        /// <returns></returns>
        public SchemaBuilder WithField(string fieldName, FieldKind kind)
        {
            _fields.Add(new SchemaField(fieldName, kind));
            return this;
        }

        public Schema Build() => new Schema(_typeName, _fields);
    }
}
