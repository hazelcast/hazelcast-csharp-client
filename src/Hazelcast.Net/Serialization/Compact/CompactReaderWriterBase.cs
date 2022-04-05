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

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using Hazelcast.Core;

namespace Hazelcast.Serialization.Compact
{
    internal abstract class CompactReaderWriterBase
    {
        protected readonly IReadWriteObjectsFromIObjectDataInputOutput ObjectsReaderWriter;
        protected readonly Schema Schema;
        protected readonly int StartPosition;
        protected readonly int DataStartPosition;

        protected CompactReaderWriterBase(IReadWriteObjectsFromIObjectDataInputOutput serializer, Schema schema, int startPosition)
        {
            ObjectsReaderWriter = serializer ?? throw new ArgumentNullException(nameof(serializer));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));

            StartPosition = startPosition;

            DataStartPosition = schema.HasReferenceFields
                ? StartPosition + BytesExtensions.SizeOfInt
                : StartPosition;
        }

        /// <summary>
        /// Tries to get a field with the specified name (case-insensitive) and kind.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fieldName">The actual field name, when found.</param>
        /// <returns>Whether a field with the specified name (case-insensitive) was found.</returns>
        public bool ValidateFieldNameInvariant(string name, [NotNullWhen(true)] out string? fieldName)
        {
            if (Schema.TryGetField(name, out var field, caseSensitive: false))
            {
                fieldName = field.FieldName;
                return true;
            }

            fieldName = null;
            return false;
        }

        /// <summary>
        /// Determines whether the schema has a field with the specified name (case-sensitive) and kind.
        /// </summary>
        public bool HasField(string name, FieldKind kind)
            => Schema.TryGetField(name, out var field) && field.Kind == kind;

        /// <summary>
        /// Gets the schema field with the specified name (case-sensitive) and kind.
        /// </summary>
        /// <exception cref="SerializationException">No such field.</exception>
        protected virtual SchemaField GetValidField(string name, FieldKind kind)
        {
            if (!Schema.TryGetField(name, out var field))
                throw new SerializationException($"Invalid field name \"{name}\" for schema {Schema}.");
            if (field.Kind != kind)
                throw new SerializationException($"Invalid kind \"{kind}\" for field \"{name}\" of schema {Schema}, which is of kind \"{field.Kind}\".");

            return field;
        }

        protected (int, byte) GetBooleanFieldPosition(string name)
        {
            var field = GetValidField(name, FieldKind.Boolean);
            return (DataStartPosition + field.Offset, field.BitOffset);
        }

        protected int GetValueFieldPosition(string name, FieldKind kind)
        {
            var field = GetValidField(name, kind);
            return DataStartPosition + field.Offset;
        }
    }
}
