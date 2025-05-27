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
using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact
{
    internal class SchemaBuilderWriter : ICompactWriter
    {
        private readonly string _typeName;
        private readonly HashSet<string> _names = new();
        private readonly List<SchemaField> _fields = new();

        public SchemaBuilderWriter(string typeName)
        {
            _typeName = typeName;
        }

        public void AddField(string name, FieldKind kind)
        {
            if (!_names.Add(name))
                throw new SerializationException($"Field '{name}' has already been written, fields can only be written once.");
            _fields.Add(new SchemaField(name, kind));
        }

        public Schema Build() => new(_typeName, _fields);

        // do NOT remove nor alter the <generated></generated> lines!
        // <generated>

         public void WriteBoolean(string name, bool value) => AddField(name, FieldKind.Boolean);
         public void WriteInt8(string name, sbyte value) => AddField(name, FieldKind.Int8);
         public void WriteInt16(string name, short value) => AddField(name, FieldKind.Int16);
         public void WriteInt32(string name, int value) => AddField(name, FieldKind.Int32);
         public void WriteInt64(string name, long value) => AddField(name, FieldKind.Int64);
         public void WriteFloat32(string name, float value) => AddField(name, FieldKind.Float32);
         public void WriteFloat64(string name, double value) => AddField(name, FieldKind.Float64);
         public void WriteArrayOfBoolean(string name, bool[]? value) => AddField(name, FieldKind.ArrayOfBoolean);
         public void WriteArrayOfInt8(string name, sbyte[]? value) => AddField(name, FieldKind.ArrayOfInt8);
         public void WriteArrayOfInt16(string name, short[]? value) => AddField(name, FieldKind.ArrayOfInt16);
         public void WriteArrayOfInt32(string name, int[]? value) => AddField(name, FieldKind.ArrayOfInt32);
         public void WriteArrayOfInt64(string name, long[]? value) => AddField(name, FieldKind.ArrayOfInt64);
         public void WriteArrayOfFloat32(string name, float[]? value) => AddField(name, FieldKind.ArrayOfFloat32);
         public void WriteArrayOfFloat64(string name, double[]? value) => AddField(name, FieldKind.ArrayOfFloat64);
         public void WriteNullableBoolean(string name, bool? value) => AddField(name, FieldKind.NullableBoolean);
         public void WriteNullableInt8(string name, sbyte? value) => AddField(name, FieldKind.NullableInt8);
         public void WriteNullableInt16(string name, short? value) => AddField(name, FieldKind.NullableInt16);
         public void WriteNullableInt32(string name, int? value) => AddField(name, FieldKind.NullableInt32);
         public void WriteNullableInt64(string name, long? value) => AddField(name, FieldKind.NullableInt64);
         public void WriteNullableFloat32(string name, float? value) => AddField(name, FieldKind.NullableFloat32);
         public void WriteNullableFloat64(string name, double? value) => AddField(name, FieldKind.NullableFloat64);
         public void WriteDecimal(string name, HBigDecimal? value) => AddField(name, FieldKind.Decimal);
         public void WriteString(string name, string? value) => AddField(name, FieldKind.String);
         public void WriteTime(string name, HLocalTime? value) => AddField(name, FieldKind.Time);
         public void WriteDate(string name, HLocalDate? value) => AddField(name, FieldKind.Date);
         public void WriteTimeStamp(string name, HLocalDateTime? value) => AddField(name, FieldKind.TimeStamp);
         public void WriteTimeStampWithTimeZone(string name, HOffsetDateTime? value) => AddField(name, FieldKind.TimeStampWithTimeZone);
         public void WriteCompact<T>(string name, T? value) => AddField(name, FieldKind.Compact);
         public void WriteArrayOfNullableBoolean(string name, bool?[]? value) => AddField(name, FieldKind.ArrayOfNullableBoolean);
         public void WriteArrayOfNullableInt8(string name, sbyte?[]? value) => AddField(name, FieldKind.ArrayOfNullableInt8);
         public void WriteArrayOfNullableInt16(string name, short?[]? value) => AddField(name, FieldKind.ArrayOfNullableInt16);
         public void WriteArrayOfNullableInt32(string name, int?[]? value) => AddField(name, FieldKind.ArrayOfNullableInt32);
         public void WriteArrayOfNullableInt64(string name, long?[]? value) => AddField(name, FieldKind.ArrayOfNullableInt64);
         public void WriteArrayOfNullableFloat32(string name, float?[]? value) => AddField(name, FieldKind.ArrayOfNullableFloat32);
         public void WriteArrayOfNullableFloat64(string name, double?[]? value) => AddField(name, FieldKind.ArrayOfNullableFloat64);
         public void WriteArrayOfDecimal(string name, HBigDecimal?[]? value) => AddField(name, FieldKind.ArrayOfDecimal);
         public void WriteArrayOfTime(string name, HLocalTime?[]? value) => AddField(name, FieldKind.ArrayOfTime);
         public void WriteArrayOfDate(string name, HLocalDate?[]? value) => AddField(name, FieldKind.ArrayOfDate);
         public void WriteArrayOfTimeStamp(string name, HLocalDateTime?[]? value) => AddField(name, FieldKind.ArrayOfTimeStamp);
         public void WriteArrayOfTimeStampWithTimeZone(string name, HOffsetDateTime?[]? value) => AddField(name, FieldKind.ArrayOfTimeStampWithTimeZone);
         public void WriteArrayOfString(string name, string?[]? value) => AddField(name, FieldKind.ArrayOfString);
         public void WriteArrayOfCompact<T>(string name, T?[]? value) => AddField(name, FieldKind.ArrayOfCompact);

        // </generated>
    }
}
