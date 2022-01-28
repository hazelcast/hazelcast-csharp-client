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

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Represents a compact serialization schema.
    /// </summary>
    public class Schema : IIdentifiedDataSerializable
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
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets the schema fields.
        /// </summary>
        public SchemaField[] Fields { get; private set; } // ordered

        internal Dictionary<string, SchemaField> FieldMap { get; private set; }

        internal int ValueFieldCount { get; private set; }

        internal int ValueFieldLength { get; private set; }

        internal int ReferenceFieldCount { get; private set; }

        internal bool HasReferenceFields => ReferenceFieldCount > 0;

        private void Initialize(string typeName, IEnumerable<SchemaField> typeFields) // FIXME shall we expose more stuff? for, like, writing things out?
        {
            TypeName = typeName;

            // FIXME need to revisit the ordering + field names must be unique

            // the ordered list of fields, which will be used for fingerprinting
            var fieldsList = new List<SchemaField>();

            List<SchemaField> booleanFields = null;
            List<SchemaField> valueFields = null;
            List<SchemaField> referenceFields = null;

            foreach (var field in typeFields.OrderBy(x => x.FieldName))
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
                var fields = valueFields
                    .Select(x => (x, x.Kind.GetValueTypeSize()))
                    .OrderByDescending(x => x.Item2)
                    .ThenBy(x => x.Item1.FieldName);
                foreach (var (field, size) in fields)
                {
                    fieldsList.Add(field);
                    field.Offset = offset;
                    offset += size;
                }
            }

            if (booleanFields != null)
            {
                byte bitOffset = 0;
                var fields = booleanFields.OrderBy(x => x.FieldName);
                foreach (var field in fields)
                {
                    fieldsList.Add(field);
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

            // FIXME Java also determines this + the number of reference type fields
            var valueFieldsLength = offset;

            if (referenceFields != null)
            {
                var index = 0;
                var fields = referenceFields.OrderBy(x => x.FieldName);
                foreach (var field in fields)
                {
                    fieldsList.Add(field);
                    field.Index = index++;
                }
            }

            ValueFieldCount = valueFields?.Count ?? 0; // FIXME include booleans?
            ValueFieldLength = offset;
            ReferenceFieldCount = referenceFields?.Count ?? 0;

            Fields = fieldsList.ToArray();
            FieldMap = fieldsList.ToDictionary(x => x.FieldName, x => x);

            Id = ComputeId();
        }

        private long ComputeId()
        {
            // FIXME in what order are FIELDS when calculating these things?!
            // we need a TEST that shows that swapping fields is not an issue!

            var fingerprint = RabinFingerprint.InitialValue;
            fingerprint = RabinFingerprint.Fingerprint(fingerprint, TypeName);
            fingerprint = RabinFingerprint.Fingerprint(fingerprint, Fields.Length);
            foreach (var field in Fields)
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
            output.WriteInt(Fields.Length);
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
    }
}
