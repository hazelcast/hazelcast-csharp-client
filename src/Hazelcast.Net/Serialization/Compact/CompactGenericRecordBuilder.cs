// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization.Compact;

/// <summary>
/// Builds an <see cref="IGenericRecord"/> for compact serialization.
/// </summary>
internal partial class CompactGenericRecordBuilder : IGenericRecordBuilder
{
    private readonly IDictionary<string, object?> _fieldValues;
    private readonly HashSet<string>? _overwrittenFields;
    private readonly SchemaBuilderWriter? _schemaBuilder;
    private readonly Schema? _schema;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompactGenericRecordBuilder"/> class.
    /// </summary>
    /// <param name="typename">The compact type-name of the generic record.</param>
    public CompactGenericRecordBuilder(string typename)
    {
        // corresponds to Java DeserializedGenericRecordBuilder
        if (string.IsNullOrWhiteSpace(typename)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(typename));
        _schemaBuilder = new SchemaBuilderWriter(typename);
        _fieldValues = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompactGenericRecordBuilder"/> class.
    /// </summary>
    /// <param name="schema">The compact schema of the generic record.</param>
    public CompactGenericRecordBuilder(Schema schema)
    {
        // corresponds to Java DeserializedSchemaBoundGenericRecordBuilder
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        _fieldValues = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompactGenericRecordBuilder"/> class which clones an existing record.
    /// </summary>
    /// <param name="schema">The compact schema of the generic record.</param>
    /// <param name="fieldValues">Initial field values.</param>
    public CompactGenericRecordBuilder(Schema schema, IDictionary<string, object?> fieldValues)
    {
        // corresponds to Java DeserializedGenericRecordCloner
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        _fieldValues = fieldValues ?? throw new ArgumentNullException(nameof(fieldValues));
        _overwrittenFields = new HashSet<string>();
    }

    /// <inheritdoc />
    public IGenericRecord Build()
    {
        var schema = _schema;
        if (schema != null)
        {
            if (_overwrittenFields == null)
            {
                // corresponds to Java DeserializedSchemaBoundGenericRecordBuilder
                foreach (var field in schema.Fields)
                {
                    if (!_fieldValues.ContainsKey(field.FieldName))
                        throw new SerializationException($"Missing value for field '{field.FieldName}'. All fields must be set before building the record.");
                }
            }
            // else corresponds to DeserializedGenericRecordCloner - already have all fields
        }
        else
        {
            // corresponds to Java DeserializedGenericRecordBuilder
            schema = _schemaBuilder!.Build();
        }

        return new CompactDictionaryGenericRecord(schema, _fieldValues);
    }

    private IGenericRecordBuilder SetField(string fieldname, object? value, FieldKind kind)
    {
        if (string.IsNullOrWhiteSpace(fieldname)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(fieldname));

        // corresponds to Java DeserializedSchemaBoundGenericRecordBuilder, DeserializedGenericRecordCloner
        if (_schema != null && (!_schema.TryGetField(fieldname, out var field) || field.Kind != kind))
                throw new SerializationException($"Schema for type '{_schema.TypeName}' does not contain a field named '{fieldname}' of kind {kind}.");

        if (_overwrittenFields != null)
        {
            // corresponds to Java DeserializedGenericRecordCloner
            if (!_overwrittenFields.Add(fieldname))
                throw new SerializationException("Fields can be set only once.");
            _fieldValues[fieldname] = value; // replace
        }
        else
        {
            // corresponds to Java DeserializedGenericRecordBuilder, DeserializedSchemaBoundGenericRecordBuilder
            if (!_fieldValues.TryAdd(fieldname, value)) throw new SerializationException("Fields can be set only once.");
        }

        // corresponds to Java DeserializedGenericRecordBuilder
        if (_schemaBuilder != null)
            _schemaBuilder.AddField(fieldname, kind);

        return this;
    }

    /// <inheritdoc />
    public IGenericRecordBuilder SetGenericRecord(string fieldname, IGenericRecord? value)
    {
        if (value != null && value is not CompactGenericRecordBase)
            throw new ArgumentException("Value is not a compact generic record.", nameof(value));

        return SetField(fieldname, value, FieldKind.Compact);
    }

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfGenericRecord(string fieldname, IGenericRecord?[]? value)
    {
        if (value != null && value.Any(x => x != null && x is not CompactGenericRecordBase))
            throw new ArgumentException("Value contains a record which is not a compact generic record.", nameof(value));

        return SetField(fieldname, value, FieldKind.ArrayOfCompact);
    }
}