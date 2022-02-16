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
        protected readonly CompactSerializer Serializer;
        protected readonly Schema Schema;
        protected readonly int StartPosition;
        protected readonly int DataStartPosition;

        protected CompactReaderWriterBase(CompactSerializer serializer, Schema schema, int startPosition)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));

            StartPosition = startPosition;

            DataStartPosition = schema.HasReferenceFields
                ? StartPosition + BytesExtensions.SizeOfInt
                : StartPosition;
        }

        public bool GetValidFieldName(string name, [NotNullWhen(true)] out string? fieldName)
        {
            if (Schema.FieldsMap.TryGetValue(name, out _))
            {
                fieldName = name;
                return true;
            }

            foreach (var field in Schema.Fields)
            {
                // FIXME - move all this to schema & cache?
                if (field.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    fieldName = field.FieldName;
                    return true;
                }
            }

            fieldName = null;
            return false;
        }

        public bool HasField(string name, FieldKind kind)
            => Schema.FieldsMap.TryGetValue(name, out var field) && field.Kind == kind;

        protected virtual SchemaField GetValidField(string name, FieldKind kind)
        {
            if (!Schema.FieldsMap.TryGetValue(name, out var field))
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
