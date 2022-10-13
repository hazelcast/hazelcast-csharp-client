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
    IGenericRecord Build();

    /// <summary>
    /// Adds a object field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    /// <remarks>
    /// <para>It is only legal to set a generic record object produced by the same
    /// type of builder. For instance, a compact generic record can only accept a
    /// compact generic record. Trying to set a different kind of generic record
    /// results in a exception.</para>
    /// </remarks>
    IGenericRecordBuilder SetGenericRecord(string fieldname, IGenericRecord? value);

    /// <summary>
    /// Adds a array of objects field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    /// <remarks>
    /// <para>It is only legal to set a generic record object produced by the same
    /// type of builder. For instance, a compact generic record can only accept a
    /// compact generic record. Trying to set a different kind of generic record
    /// results in a exception.</para>
    /// </remarks>
    IGenericRecordBuilder SetArrayOfGenericRecord(string fieldname, IGenericRecord?[]? value);
}