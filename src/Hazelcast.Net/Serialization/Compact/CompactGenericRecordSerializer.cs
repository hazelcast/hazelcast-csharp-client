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
using System.Linq;
using Hazelcast.Core;

namespace Hazelcast.Serialization.Compact;

// an ICompactSerializer for compact generic records
internal class CompactGenericRecordSerializer : ICompactSerializer<CompactGenericRecordBase>
{
    // this is for *all* generic records, not for a specific type name,
    // and this method should never been invoked and is not supported.
    public string TypeName => throw new NotSupportedException();

    // reads a record
    public CompactGenericRecordBase Read(ICompactReader reader)
    {
        var schema = reader.MustBe<CompactReader>(nameof(reader)).Schema;

        var fieldValues = schema.Fields.ToDictionary(
            x => x.FieldName,
            x => reader.ReadAny(x.FieldName, x.Kind));

        return new CompactDictionaryGenericRecord(schema, fieldValues);
    }

    // writes a record
    public void Write(ICompactWriter writer, CompactGenericRecordBase value)
    {
        foreach (var field in value.Schema.Fields)
        {
            writer.WriteAny(field.FieldName, field.Kind, value.GetFieldValue(field.FieldName));
        }
    }
}
