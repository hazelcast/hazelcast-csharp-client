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

        public void WriteBooleans(string name, bool[]? value)
            => AddField(name, FieldKind.ArrayOfBoolean);

        public void WriteBooleanRefs(string name, bool?[]? value)
            => AddField(name, FieldKind.ArrayOfBooleanRef);

        public void WriteSByte(string name, sbyte value)
            => AddField(name, FieldKind.SignedInteger8);

        public void WriteSByteRef(string name, sbyte? value)
            => AddField(name, FieldKind.SignedInteger8Ref);

        public void WriteSBytes(string name, sbyte[]? value)
            => AddField(name, FieldKind.ArrayOfSignedInteger8);

        public void WriteSByteRefs(string name, sbyte?[]? value)
            => AddField(name, FieldKind.ArrayOfSignedInteger8Ref);

        public void WriteShort(string name, short value)
            => AddField(name, FieldKind.SignedInteger16);

        public void WriteShortRef(string name, short? value)
            => AddField(name, FieldKind.SignedInteger16Ref);

        public void WriteShorts(string name, short[]? value)
            => AddField(name, FieldKind.ArrayOfSignedInteger16);

        public void WriteShortRefs(string name, short?[]? value)
            => AddField(name, FieldKind.ArrayOfSignedInteger16Ref);

        public void WriteInt(string name, int value)
            => AddField(name, FieldKind.SignedInteger32);

        public void WriteIntRef(string name, int? value)
            => AddField(name, FieldKind.SignedInteger32Ref);

        public void WriteInts(string name, int[]? value)
            => AddField(name, FieldKind.ArrayOfSignedInteger32);

        public void WriteIntRefs(string name, int?[]? value)
            => AddField(name, FieldKind.ArrayOfSignedInteger32Ref);

        public void WriteLong(string name, long value)
            => AddField(name, FieldKind.SignedInteger64);

        public void WriteLongRef(string name, long? value)
            => AddField(name, FieldKind.SignedInteger64Ref);

        public void WriteLongs(string name, long[]? value)
            => AddField(name, FieldKind.ArrayOfSignedInteger64);

        public void WriteLongRefs(string name, long?[]? value)
            => AddField(name, FieldKind.ArrayOfSignedInteger64Ref);

        public void WriteFloat(string name, float value)
            => AddField(name, FieldKind.Float);

        public void WriteFloatRef(string name, float? value)
            => AddField(name, FieldKind.FloatRef);

        public void WriteFloats(string name, float[]? value)
            => AddField(name, FieldKind.ArrayOfFloat);

        public void WriteFloatRefs(string name, float?[]? value)
            => AddField(name, FieldKind.ArrayOfFloatRef);

        public void WriteDouble(string name, double value)
            => AddField(name, FieldKind.Double);

        public void WriteDoubleRef(string name, double? value)
            => AddField(name, FieldKind.DoubleRef);

        public void WriteDoubles(string name, double[]? value)
            => AddField(name, FieldKind.ArrayOfDouble);

        public void WriteDoubleRefs(string name, double?[]? value)
            => AddField(name, FieldKind.ArrayOfDoubleRef);

        public void WriteStringRef(string name, string? value)
            => AddField(name, FieldKind.StringRef);

        public void WriteStringRefs(string name, string?[]? value)
            => AddField(name, FieldKind.ArrayOfStringRef);

        public void WriteDecimalRef(string name, decimal? value)
            => AddField(name, FieldKind.DecimalRef);

        public void WriteDecimalRefs(string name, decimal?[]? value)
            => AddField(name, FieldKind.ArrayOfDecimalRef);

        public void WriteDecimalRef(string name, HBigDecimal? value)
            => AddField(name, FieldKind.DecimalRef);

        public void WriteDecimalRefs(string name, HBigDecimal?[]? value)
            => AddField(name, FieldKind.ArrayOfDecimalRef);

        public void WriteTimeRef(string name, TimeSpan? value)
            => AddField(name, FieldKind.TimeRef);

        public void WriteTimeRefs(string name, TimeSpan?[]? value)
            => AddField(name, FieldKind.ArrayOfTimeRef);

        public void WriteDateRef(string name, DateTime? value)
            => AddField(name, FieldKind.DateRef);

        public void WriteDateRefs(string name, DateTime?[]? value)
            => AddField(name, FieldKind.ArrayOfDateRef);

        public void WriteTimeStampRef(string name, DateTime? value)
            => AddField(name, FieldKind.TimeStampRef);

        public void WriteTimeStampRefs(string name, DateTime?[]? value)
            => AddField(name, FieldKind.ArrayOfTimeStampRef);

        public void WriteTimeStampWithTimeZoneRef(string name, DateTimeOffset? value)
            => AddField(name, FieldKind.TimeStampWithTimeZoneRef);

        public void WriteTimeStampWithTimeZoneRefs(string name, DateTimeOffset?[]? value)
            => AddField(name, FieldKind.ArrayOfTimeStampWithTimeZoneRef);

        public void WriteObjectRef(string name, object? value)
            => AddField(name, FieldKind.ObjectRef);

        public void WriteObjectRefs(string name, object?[]? value)
            => AddField(name, FieldKind.ArrayOfObjectRef);
    }
}
