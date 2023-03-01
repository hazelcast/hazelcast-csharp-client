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

namespace Hazelcast.Serialization;

/// <summary>
/// Builds an <see cref="IGenericRecord"/>.
/// </summary>
public partial interface IGenericRecordBuilder
{
    /// <summary>
    /// Build the <see cref="IGenericRecord"/>.
    /// </summary>
    /// <returns>The <see cref="IGenericRecord"/>.</returns>
    /// <remarks>
    /// <para>In case the record was created with a schema, then all fields declared in the schema
    /// must have been assigned a value before the record can be built. Trying to build the record
    /// before all fields have been assigned a value triggers a <see cref="SerializationException"/>.</para>
    /// </remarks>
    IGenericRecord Build();

    /// <summary>
    /// Adds a <see cref="IGenericRecord"/> object field to the record.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    /// <remarks>
    /// <para>It is only legal to set a generic record object produced by the same
    /// type of builder. For instance, a compact generic record can only accept a
    /// compact generic record. Trying to set a different kind of generic record
    /// results in a exception.</para>
    /// <para>It is legal to set the field again only when the builder is created with
    /// <see cref="IGenericRecord.NewBuilderWithClone()"/>; it is otherwise illegal
    /// to set to the same field twice.</para>
    /// <para>This method allows nested structures; subclasses should also be
    /// created as <see cref="IGenericRecord"/> of the same nature of the nesting one.
    /// I.e. compact records can only nest compact records.
    ///</para>
    /// </remarks>
    /// <exception cref="SerializationException">The build has been initialized with a
    /// schema, and <paramref name="fieldname"/> is not the name of field of that schema, or
    /// the type of the field does not match the specified value, or the field value is set
    /// multiple times.</exception>
    IGenericRecordBuilder SetGenericRecord(string fieldname, IGenericRecord? value);

    /// <summary>
    /// Adds an array of <see cref="IGenericRecord"/> objects field to the record.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    /// <remarks>
    /// <para>It is only legal to set a generic record object produced by the same
    /// type of builder. For instance, a compact generic record can only accept a
    /// compact generic record. Trying to set a different kind of generic record
    /// results in a exception.</para>
    /// <para>It is legal to set the field again only when the builder is created with
    /// <see cref="IGenericRecord.NewBuilderWithClone()"/>; it is otherwise illegal
    /// to set to the same field twice.</para>
    /// <para>This method allows nested structures; subclasses should also be
    /// created as <see cref="IGenericRecord"/> of the same nature of the nesting one.
    /// I.e. compact records can only nest compact records.
    ///</para>
    /// </remarks>
    /// <exception cref="SerializationException">The build has been initialized with a
    /// schema, and <paramref name="fieldname"/> is not the name of field of that schema, or
    /// the type of the field does not match the specified value, or the field value is set
    /// multiple times.</exception>
    IGenericRecordBuilder SetArrayOfGenericRecord(string fieldname, IGenericRecord?[]? value);
}