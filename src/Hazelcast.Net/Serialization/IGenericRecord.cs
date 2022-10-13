// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization;

/// <summary>
/// Defines a generic record.
/// </summary>
public partial interface IGenericRecord
{
    /// <summary>
    /// Creates a new <see cref="GenericRecordBuilder"/> for this record's schema,
    /// with all the fields being non-initialized.
    /// </summary>
    /// <returns>A new <see cref="GenericRecordBuilder"/>.</returns>
    public IGenericRecordBuilder NewBuilder();

    /// <summary>
    /// Creates a new <see cref="GenericRecordBuilder"/> for this record's schema,
    /// with all the fields copied from this record.
    /// </summary>
    /// <returns>A new <see cref="GenericRecordBuilder"/>.</returns>
    public IGenericRecordBuilder NewBuilderWithClone();

    /// <summary>
    /// Gets the collection of field names for this record.
    /// </summary>
    public IReadOnlyCollection<string> FieldNames { get; }

    /// <summary>
    /// Gets the <see cref="FieldKind"/> of the specified field, or <see cref="FieldKind.NotAvailable"/>
    /// if no field exists with the specified name.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The <see cref="FieldKind"/> of the field, or <see cref="FieldKind.NotAvailable"/> if
    /// no field exists with the specified name.</returns>
    public FieldKind GetFieldKind(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Compact"/> field as a <see cref="IGenericRecord"/>.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public IGenericRecord? GetGenericRecord(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfCompact"/> field as an array of <see cref="IGenericRecord"/>.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public IGenericRecord?[]? GetArrayOfGenericRecord(string fieldname);
}