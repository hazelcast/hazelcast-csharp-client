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
using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact
{
    internal class SchemaBuilderWriter : ICompactWriter
    {
        private readonly string _typeName;
        private readonly List<SchemaField> _fields = new List<SchemaField>();

        public SchemaBuilderWriter(string typeName)
        {
            _typeName = typeName;
        }

        private void AddField(string name, FieldKind kind)
            => _fields.Add(new SchemaField(name, kind));

        public Schema Build()
            => new Schema(_typeName, _fields);

        public void WriteBoolean(string name, bool value)
            => AddField(name, FieldKind.Boolean);

        public void WriteBooleanRef(string name, bool? value)
            => AddField(name, FieldKind.BooleanRef);

        public void WriteArrayOfBoolean(string name, bool[]? value)
            => AddField(name, FieldKind.ArrayOfBoolean);

        public void WriteArrayOfBooleanRef(string name, bool?[]? value)
            => AddField(name, FieldKind.ArrayOfBooleanRef);

        public void WriteInt8(string name, sbyte value)
            => AddField(name, FieldKind.Int8);

        public void WriteInt8Ref(string name, sbyte? value)
            => AddField(name, FieldKind.Int8Ref);

        public void WriteArrayOfInt8(string name, sbyte[]? value)
            => AddField(name, FieldKind.ArrayOfInt8);

        public void WriteArrayOfInt8Ref(string name, sbyte?[]? value)
            => AddField(name, FieldKind.ArrayOfInt8Ref);

        public void WriteInt16(string name, short value)
            => AddField(name, FieldKind.Int16);

        public void WriteInt16Ref(string name, short? value)
            => AddField(name, FieldKind.Int16Ref);

        public void WriteArrayOfInt16(string name, short[]? value)
            => AddField(name, FieldKind.ArrayOfInt16);

        public void WriteArrayOfInt16Ref(string name, short?[]? value)
            => AddField(name, FieldKind.ArrayOfInt16Ref);

        public void WriteInt32(string name, int value)
            => AddField(name, FieldKind.Int32);

        public void WriteInt32Ref(string name, int? value)
            => AddField(name, FieldKind.Int32Ref);

        public void WriteArrayOfInt32(string name, int[]? value)
            => AddField(name, FieldKind.ArrayOfInt32);

        public void WriteArrayOfInt32Ref(string name, int?[]? value)
            => AddField(name, FieldKind.ArrayOfInt32Ref);

        public void WriteInt64(string name, long value)
            => AddField(name, FieldKind.Int64);

        public void WriteInt64Ref(string name, long? value)
            => AddField(name, FieldKind.Int64Ref);

        public void WriteArrayOfInt64(string name, long[]? value)
            => AddField(name, FieldKind.ArrayOfInt64);

        public void WriteArrayOfInt64Ref(string name, long?[]? value)
            => AddField(name, FieldKind.ArrayOfInt64Ref);

        public void WriteFloat32(string name, float value)
            => AddField(name, FieldKind.Float32);

        public void WriteFloat32Ref(string name, float? value)
            => AddField(name, FieldKind.Float32Ref);

        public void WriteArrayOfFloat32(string name, float[]? value)
            => AddField(name, FieldKind.ArrayOfFloat32);

        public void WriteArrayOfFloat32Ref(string name, float?[]? value)
            => AddField(name, FieldKind.ArrayOfFloat32Ref);

        public void WriteFloat64(string name, double value)
            => AddField(name, FieldKind.Float64);

        public void WriteFloat64Ref(string name, double? value)
            => AddField(name, FieldKind.Float64Ref);

        public void WriteArrayOfFloat64(string name, double[]? value)
            => AddField(name, FieldKind.ArrayOfFloat64);

        public void WriteArrayOfFloat64Ref(string name, double?[]? value)
            => AddField(name, FieldKind.ArrayOfFloat64Ref);

        public void WriteStringRef(string name, string? value)
            => AddField(name, FieldKind.StringRef);

        public void WriteArrayOfStringRef(string name, string?[]? value)
            => AddField(name, FieldKind.ArrayOfStringRef);

        public void WriteDecimalRef(string name, decimal? value)
            => AddField(name, FieldKind.DecimalRef);

        public void WriteArrayOfDecimalRef(string name, decimal?[]? value)
            => AddField(name, FieldKind.ArrayOfDecimalRef);

        public void WriteDecimalRef(string name, HBigDecimal? value)
            => AddField(name, FieldKind.DecimalRef);

        public void WriteArrayOfDecimalRef(string name, HBigDecimal?[]? value)
            => AddField(name, FieldKind.ArrayOfDecimalRef);

        public void WriteTimeRef(string name, TimeSpan? value)
            => AddField(name, FieldKind.TimeRef);

        public void WriteArrayOfTimeRef(string name, TimeSpan?[]? value)
            => AddField(name, FieldKind.ArrayOfTimeRef);

        public void WriteDateRef(string name, DateTime? value)
            => AddField(name, FieldKind.DateRef);

        public void WriteArrayOfDateRef(string name, DateTime?[]? value)
            => AddField(name, FieldKind.ArrayOfDateRef);

        public void WriteTimeStampRef(string name, DateTime? value)
            => AddField(name, FieldKind.TimeStampRef);

        public void WriteArrayOfTimeStampRef(string name, DateTime?[]? value)
            => AddField(name, FieldKind.ArrayOfTimeStampRef);

        public void WriteTimeStampWithTimeZoneRef(string name, DateTimeOffset? value)
            => AddField(name, FieldKind.TimeStampWithTimeZoneRef);

        public void WriteArrayOfTimeStampWithTimeZoneRef(string name, DateTimeOffset?[]? value)
            => AddField(name, FieldKind.ArrayOfTimeStampWithTimeZoneRef);

        public void WriteCompactRef(string name, object? value)
            => AddField(name, FieldKind.CompactRef);

        public void WriteArrayOfCompactRef(string name, object?[]? value)
            => AddField(name, FieldKind.ArrayOfCompactRef);
    }
}
