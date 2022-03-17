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
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Exceptions;

#pragma warning disable CA1724 // 'Schema' conflicts with System.Xml.Schema - well, yes. 

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Represents a compact serialization schema.
    /// </summary>
    public class Schema : IIdentifiedDataSerializable, IEquatable<Schema>
    {
        /// <inheritdoc />
        public int FactoryId => CompactSerializationHook.Constants.FactoryId;

        /// <inheritdoc />
        public int ClassId => CompactSerializationHook.Constants.ClassIds.Schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        internal Schema()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        public Schema(string typeName, IEnumerable<SchemaField> typeFields)
        {
            if (string.IsNullOrWhiteSpace(typeName)) throw new ArgumentException(ExceptionMessages.NullOrEmpty);
            if (typeFields == null) throw new ArgumentNullException(nameof(typeFields));
            Initialize(typeName, typeFields);
        }

        /// <summary>
        /// Gets the identifier of the schema.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// Gets the compact type-name.
        /// </summary>
        public string TypeName { get; private set; } = null!; // null! else warning in ctor, property set in Initialize()

        /// <summary>
        /// Gets the ordered schema fields.
        /// </summary>
        public IReadOnlyList<SchemaField> Fields { get; private set; } = null!; // null! else warning in ctor, property set in Initialize()

        internal IReadOnlyDictionary<string, SchemaField> FieldsMap { get; private set; } = null!; // null! else warning in ctor, property set in Initialize()

        internal int ValueFieldLength { get; private set; }

        internal int ReferenceFieldCount { get; private set; }

        internal bool HasReferenceFields => ReferenceFieldCount > 0;

        private static int _count;

        private void Initialize(string typeName, IEnumerable<SchemaField> typeFields)
        {
            TypeName = typeName;

            _count++;

            // the sorted set of fields, which will be used for fingerprinting - needs to be ordered
            // exactly in the same way as Java, which uses Comparator.naturalOrder() i.e. "natural
            // order", and good luck finding a definition for this, so we're going with whatever is
            // default in C# and hope it works.
            var fieldsMap = new SortedDictionary<string, SchemaField>();

            // ensure no duplicate field name
            var fieldNames = new HashSet<string>();

            // different type of fields
            List<SchemaField>? booleanFields = null;
            List<SchemaField>? valueFields = null;
            List<SchemaField>? referenceFields = null;

            // build the fields map, which is sorted (see above)
            foreach (var field in typeFields) fieldsMap[field.FieldName] = field;

            foreach (var field in fieldsMap.Values)
            {
                if (fieldNames.Contains(field.FieldName))
                    throw new ArgumentException($"Fields contain duplicate field name {field.FieldName}.", nameof(typeFields));
                fieldNames.Add(field.FieldName);
                if (field.Kind == FieldKind.Boolean)
                    (booleanFields ??= new List<SchemaField>()).Add(field);
                else if (field.Kind.IsValueType())
                    (valueFields ??= new List<SchemaField>()).Add(field);
                else
                    (referenceFields ??= new List<SchemaField>()).Add(field);
            }

            var offset = 0;

            if (valueFields != null)
            {
                // value fields come first, ordered by size DESC then by name ASC
                var fields = valueFields
                    .Select(x => (x, x.Kind.GetValueTypeSize()))
                    .OrderByDescending(x => x.Item2)
                    .ThenBy(x => x.Item1.FieldName);
                foreach (var (field, size) in fields)
                {
                    field.Offset = offset;
                    offset += size;
                }
            }

            if (booleanFields != null)
            {
                // boolean fields come next, ordered by name ASC
                byte bitOffset = 0;
                var fields = booleanFields.OrderBy(x => x.FieldName);
                foreach (var field in fields)
                {
                    field.Offset = offset;
                    field.BitOffset = bitOffset;
                    bitOffset += 1;
                    if (bitOffset == 8)
                    {
                        bitOffset = 0;
                        offset += 1;
                    }
                }

                if (bitOffset != 0) offset += 1;
            }

            if (referenceFields != null)
            {
                // reference fields come last, ordered by name ASC
                var index = 0;
                var fields = referenceFields.OrderBy(x => x.FieldName);
                foreach (var field in fields)
                {
                    field.Index = index++;
                }
            }

            ValueFieldLength = offset;
            ReferenceFieldCount = referenceFields?.Count ?? 0;

            Fields = fieldsMap.Values.ToArray();
            FieldsMap = fieldsMap;

            Id = ComputeId();
        }

        private long ComputeId()
        {
            var fingerprint = RabinFingerprint.InitialValue;
            fingerprint = RabinFingerprint.Fingerprint(fingerprint, TypeName);
            fingerprint = RabinFingerprint.Fingerprint(fingerprint, Fields.Count);
            foreach (var field in Fields) // fields are ordered for reproducible fingerprint
            {
                fingerprint = RabinFingerprint.Fingerprint(fingerprint, field.FieldName);
                fingerprint = RabinFingerprint.Fingerprint(fingerprint, (int)field.Kind);
            }

            return (long) fingerprint;
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteString(TypeName);
            output.WriteInt(Fields.Count);
            foreach (var field in Fields)
            {
                output.WriteString(field.FieldName);
                output.WriteInt((int)field.Kind);
            }
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var typeName = input.ReadString();

            var fieldCount = input.ReadInt();
            var typeFields = new SchemaField[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                var name = input.ReadString();
                var kind = FieldKindEnum.Parse(input.ReadInt());
                typeFields[i] = new SchemaField(name, kind);
            }

            Initialize(typeName, typeFields);
        }

        /// <inheritdoc />
        public bool Equals(Schema? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is Schema other && Equals(other);

        /// <inheritdoc />
        // ReSharper disable once NonReadonlyMemberInGetHashCode - it is in fact readonly
        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Schema? left, Schema? right)
            // ReSharper disable once MergeConditionalExpression - no, cleaner that way
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(Schema? left, Schema? right)
            => !(left == right);
    }
}
