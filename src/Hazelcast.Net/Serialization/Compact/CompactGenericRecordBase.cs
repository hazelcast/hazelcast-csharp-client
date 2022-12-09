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

using System;
using System.Collections.Generic;

namespace Hazelcast.Serialization.Compact;

/// <summary>
/// Provides a base class for compact serialization <see cref="IGenericRecord"/> implementations.
/// </summary>
internal abstract class CompactGenericRecordBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompactGenericRecordBase"/> class.
    /// </summary>
    /// <param name="schema">The compact serialization <see cref="Schema"/> of the generic record.</param>
    protected CompactGenericRecordBase(Schema schema)
    {
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));
    }

    /// <summary>
    /// Gets the schema.
    /// </summary>
    public Schema Schema { get; }

    /// <summary>
    /// Gets the collection of field names for this record.
    /// </summary>
    public IReadOnlyCollection<string> FieldNames => Schema.FieldNames;

    /// <summary>
    /// Gets the <see cref="FieldKind"/> of the specified field, or <see cref="FieldKind.NotAvailable"/>
    /// if no field exists with the specified name.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The <see cref="FieldKind"/> of the field, or <see cref="FieldKind.NotAvailable"/> if
    /// no field exists with the specified <paramref name="fieldname"/>.</returns>
    public FieldKind GetFieldKind(string fieldname) => Schema.TryGetField(fieldname, out var field) ? field.Kind : FieldKind.NotAvailable;

    public abstract object? GetFieldValue(string fieldname);
}