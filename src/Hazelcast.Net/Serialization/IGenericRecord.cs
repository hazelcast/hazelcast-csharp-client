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
    /// <remarks>
    /// <para>This method is a convenience method to get a builder without creating
    /// the schema for the type, but by re-using the schema of this record. See
    /// <see cref="GenericRecordBuilder.Compact(string)"/> for creating a generic record
    /// in compact form, with a new schema.</para>
    /// </remarks>
    /// <example>
    /// var rec2 = rec1.NewBuilder()
    ///   .SetBoolean("field-bool", true)
    ///   .SetInt32("field-int", 1234)
    ///   .Build();
    /// </example>
    public IGenericRecordBuilder NewBuilder();

    /// <summary>
    /// Creates a new <see cref="GenericRecordBuilder"/> for this record's schema,
    /// with all the fields copied from this record.
    /// </summary>
    /// <returns>A new <see cref="GenericRecordBuilder"/>.</returns>
    /// <remarks>
    /// <para>This method produces an exact copy of this generic record, which can
    /// then be updated.</para>
    /// </remarks>
    /// <example>
    /// // this requires you to specify all properties
    /// var rec2 = rec1.NewBuilder()
    ///   .SetBoolean("field-bool", true)
    ///   .SetInt32("field-int", 1234)
    ///   .Build();
    ///
    /// // this allows you to only specify the modified properties
    /// var rec3 = rec1.NewBuilderWithClone()
    ///   .SetInt32("field-int", 1234)
    ///   .Build();
    /// </example>
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
    /// Gets the value of a <see cref="IGenericRecord"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public IGenericRecord? GetGenericRecord(string fieldname);

    /// <summary>
    /// Gets the value of an array of <see cref="IGenericRecord"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public IGenericRecord?[]? GetArrayOfGenericRecord(string fieldname);
}