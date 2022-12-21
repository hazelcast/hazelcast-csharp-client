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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Hazelcast.Exceptions;

// 'Schema' conflicts with System.Xml.Schema - well, yes.
#pragma warning disable CA1724  

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Represents a compact serialization schema.
    /// </summary>
    internal class Schema : IIdentifiedDataSerializable, IEquatable<Schema>
    {
        private Dictionary<string, SchemaField> _fieldsMap;
        private Dictionary<string, SchemaField>? _fieldsMapInvariant;
        private IReadOnlyList<string>? _fieldNames;

        /// <inheritdoc />
        public int FactoryId => CompactSerializationHook.Constants.FactoryId;

        /// <inheritdoc />
        public int ClassId => CompactSerializationHook.Constants.ClassIds.Schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        // TODO: remove pragma when Initialize has the attribute with C# 9
#pragma warning disable CS8618
        internal Schema()
#pragma warning restore CS8618
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        // TODO: remove pragma when Initialize has the attribute with C# 9
#pragma warning disable CS8618
        public Schema(string typeName, IEnumerable<SchemaField> typeFields)
#pragma warning restore CS8618
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

        /// <summary>
        /// Gets the ordered names of the fields.
        /// </summary>
        /// <remarks>Optimizes and caches the enumeration of fields.</remarks>
        public IReadOnlyList<string> FieldNames => _fieldNames ??= Fields.Select(x => x.FieldName).ToList();

        /// <summary>
        /// Tries to get a field.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="field">When this method returns, contains the <see cref="SchemaField"/> identified by the <paramref name="name"/>,
        /// if any, or <c>null</c> if no field exists with this name.</param>
        /// <param name="caseSensitive">Whether to perform a case-sensitive lookup.</param>
        /// <returns><c>true</c> if a field with the specified <paramref name="name"/> was found; otherwise <c>false</c>.</returns>
        internal bool TryGetField(string name, [NotNullWhen(true)] out SchemaField field, bool caseSensitive = true)
        {
            if (caseSensitive) return _fieldsMap.TryGetValue(name, out field!);
            _fieldsMapInvariant ??= new Dictionary<string, SchemaField>(_fieldsMap, StringComparer.OrdinalIgnoreCase);
            return _fieldsMapInvariant.TryGetValue(name, out field!);
        }
        
        /// <summary>
        /// Gets the size in bytes of value (non-nullable) fields.
        /// </summary>
        internal int ValueFieldLength { get; private set; }

        /// <summary>
        /// Gets the number of reference (nullable) fields.
        /// </summary>
        internal int ReferenceFieldCount { get; private set; }

        /// <summary>
        /// Whether the schema has reference (nullable) fields.
        /// </summary>
        internal bool HasReferenceFields => ReferenceFieldCount > 0;

        //TODO: enable the attribute and get rid of the nullable warnings with C# 9
        //[System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_fieldsMap))]
        private void Initialize(string typeName, IEnumerable<SchemaField> typeFields)
        {
            TypeName = typeName;

            _fieldsMap = new Dictionary<string, SchemaField>();

            // ensure no duplicate field name
            var fieldNames = new HashSet<string>();

            // different type of fields
            List<SchemaField>? booleanFields = null;
            List<SchemaField>? valueFields = null;
            List<SchemaField>? referenceFields = null;

            // build the fields map
            foreach (var field in typeFields)
            {
                if (field == null)
                    throw new ArgumentException("Fields contain a null field.", nameof(typeFields));
                if (!fieldNames.Add(field.FieldName))
                    throw new ArgumentException($"Fields contain duplicate field name {field.FieldName}.", nameof(typeFields));
                _fieldsMap[field.FieldName] = field;
            }

            // the sorted set of fields, which will be used for fingerprinting - needs to be ordered
            // exactly in the same way as Java, which uses Comparator.naturalOrder() i.e. "natural
            // order", and good luck finding a definition for this, apart from it being case-sensitive,
            // so we're going with whatever seems best in C# and hope it works.
            Fields = _fieldsMap.Values.OrderBy(x => x.FieldName, StringComparer.InvariantCulture).ToArray();

            foreach (var field in Fields)
            {
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Schema(TypeName=\"{TypeName}\", Id={Id})";
        }
    }
}
