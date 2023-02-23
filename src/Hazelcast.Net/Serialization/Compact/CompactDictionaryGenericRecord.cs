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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Hazelcast.Serialization.Compact;

/// <summary>
/// Implements <see cref="IGenericRecord"/> for compact serialization with
/// values being kept in a dictionary.
/// </summary>
internal partial class CompactDictionaryGenericRecord : CompactGenericRecordBase, IGenericRecord
{
    private readonly IDictionary<string, object?> _fieldValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompactDictionaryGenericRecord"/> class.
    /// </summary>
    /// <param name="schema">The compact schema for the record.</param>
    /// <param name="fieldValues">The values for the fields.</param>
    public CompactDictionaryGenericRecord(Schema schema, IDictionary<string, object?> fieldValues)
        : base(schema)
    {
        _fieldValues = fieldValues ?? throw new ArgumentNullException(nameof(fieldValues));
    }

    /// <inheritdoc />
    public IGenericRecordBuilder NewBuilder() => new CompactGenericRecordBuilder(Schema);

    /// <inheritdoc />
    public IGenericRecordBuilder NewBuilderWithClone() => new CompactGenericRecordBuilder(Schema, _fieldValues);

    private void ValidateField(string fieldname, FieldKind kind, FieldKind altKind = FieldKind.NotAvailable)
    {
        if (!Schema.TryGetField(fieldname, out var field))
                throw new SerializationException($"Record with schema {Schema} does not contain a field named '{fieldname}'");

        if (field.Kind != kind && field.Kind != altKind)
            throw new SerializationException(
                $"Record with schema {Schema} has field '{fieldname}' of type {field.Kind}, not" +
                $" {kind}{(altKind == FieldKind.NotAvailable ? "" : $" nor {altKind}")}.");
    }

    /// <inheritdoc />
    public override object? GetFieldValue(string fieldname) => _fieldValues[fieldname];

    /// <inheritdoc />
    public IGenericRecord? GetGenericRecord(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Compact);
        return (IGenericRecord?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public IGenericRecord?[]? GetArrayOfGenericRecord(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfCompact);
        return (IGenericRecord?[]?)_fieldValues[fieldname];
    }

    private static T GetValueOf<T>(object? obj, string fieldname, FieldKind kind, FieldKind altKind)
        where T : struct
    {
        if (obj is T value) return value;

        throw new SerializationException(
            $"Get{kind} cannot return the value of field '{fieldname}' because value is null." +
            $"Use the Get{altKind} method instead.");
    }

    private static T[]? GetArrayOf<T>(object? obj, string fieldname, FieldKind kind, FieldKind altKind)
        where T : struct
    {
        if (obj == null) return null;
        if (obj is T[] array) return array;
        var convertible = HasToBe<T?[]>(obj);
        array = new T[convertible.Length];
        for (var i = 0; i < convertible.Length; i++)
        {
            array[i] = convertible[i] ?? throw new SerializationException(
                $"Get{kind} cannot return the value of field '{fieldname}' because the array contains a null value." +
                $"Use the Get{altKind} method instead.");
        }
        return array;
    }

    private static T?[]? GetArrayOfNullableOf<T>(object? obj)
        where T : struct
    {
        if (obj == null) return null;
        if (obj is T?[] array) return array;
        var convertible = HasToBe<T[]>(obj);
        array = new T?[convertible.Length];
        for (var i = 0; i < convertible.Length; i++) array[i] = convertible[i];
        return array;
    }

    // we *know* that the object *has* to be of a specified type
    // so we *could* write 'var convertible = obj as T[]!' but then we'd get a
    // null-ref exception in case anything goes wrong - but if we handle that
    // situation in the GetArray... method, it cannot be covered - so this trick
    // here ensures test coverage.

    [ExcludeFromCodeCoverage]
    private static T HasToBe<T>(object obj)
    {
        if (obj is T convertible) return convertible;
        throw new SerializationException($"Unexpected type {obj.GetType()} is not {typeof(T)}.");
    }
}