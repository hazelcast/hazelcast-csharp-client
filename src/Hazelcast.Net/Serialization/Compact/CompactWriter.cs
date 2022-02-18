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
using Hazelcast.Core;
using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact
{
    // notes:
    // when passing a lambda as an Action<> parameter, the compiler generates a supporting
    // class which statically caches the Action<> object so it's only instantiated once.

    internal class CompactWriter : CompactReaderWriterBase, ICompactWriter
    {
        private readonly ObjectDataOutput _output;
        private readonly int[]? _offsets;

        private int _dataPosition;
        private bool _completed;

        public CompactWriter(CompactSerializer serializer, ObjectDataOutput output, Schema schema)
            : base(serializer, schema, output.Position)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));

            if (schema.HasReferenceFields) _offsets = new int[schema.ReferenceFieldCount];

            _dataPosition = DataStartPosition + schema.ValueFieldLength;
        }

        protected override SchemaField GetValidField(string name, FieldKind kind)
        {
            if (_completed)
                throw new InvalidOperationException("Cannot write to a completed CompactWriter.");
            return base.GetValidField(name, kind);
        }

        private static Action<ObjectDataOutput, int> GetOffsetWriter(int dataLength)
        {
            if (dataLength < byte.MaxValue) return (output, offset) => output.WriteByte((byte)offset);
            if (dataLength < ushort.MaxValue) return (output, offset) => output.WriteUShort((ushort)offset);
            return (output, offset) => output.WriteInt(offset);
        }

        public void Complete()
        {
            // make sure to position output at its end
            _output.MoveTo(_dataPosition);

            // no reference fields = no offsets to write, nothing to do
            if (!Schema.HasReferenceFields)
                return;

            // write the offsets, which are ordered (by index) already
            // at the current position, which is at the end of data
            var dataLength = _dataPosition - DataStartPosition;
            var offsets = _offsets!;
            var offsetWriter = GetOffsetWriter(dataLength);
            foreach (var offset in offsets) offsetWriter(_output, offset);
            var endPosition = _output.Position;

            // go back the the start of the buffer and write the data length
            _output.MoveTo(StartPosition, BytesExtensions.SizeOfInt);
            _output.WriteInt(dataLength);

            // make sure to position output at its end
            _output.MoveTo(endPosition);
        }

        public byte[] ToByteArray()
        {
            if (!_completed) Complete();
            _completed = true;
            return _output.ToByteArray();
        }

        // starting with C# 9 it will be possible to merge the two methods below into one unique method with
        // *no* generic constraint, but then when T is a value type (struct) the 'value == null' comparison
        // will still always have to box the value beforehand, and we should benchmark to see what makes sense.

        private void WriteNullable<T>(string name, FieldKind kind, T? value, Action<ObjectDataOutput, T> write)
            where T: struct
        {
            var field = GetValidField(name, kind);

            if (!value.HasValue)
            {
                _offsets![field.Index] = -1;
                return;
            }

            _offsets![field.Index] = _dataPosition - DataStartPosition;
            _output.MoveTo(_dataPosition);
            write(_output, value.Value);
            _dataPosition = _output.Position;
        }

        private void WriteReference<T>(string name, FieldKind kind, T? value, Action<ObjectDataOutput, T> write)
            where T: class
        {
            var field = GetValidField(name, kind);

            if (value == null)
            {
                _offsets![field.Index] = -1;
                return;
            }

            _offsets![field.Index] = _dataPosition - DataStartPosition;
            _output.MoveTo(_dataPosition);
            write(_output, value);
            _dataPosition = _output.Position;
        }

        // arrays of value types are serialized as:
        //
        // <val_array>      ::= <item_count> <val_value>* ; an array of value-type items
        // <item_count>     ::= i32 ; number of items
        // <val_value>      ::= byte* ; serialized value-type item
        //
        // and ObjectDataOutput already has supports for these arrays, so we don't need the method below
        //
        //private void WriteArrayOfValues<T>(string name, FieldKind kind, T[] value, Action<ObjectDataOutput, T> write)
        //    where T : struct
        //{ }
        //
        // on the other hand, arrays of reference / nullable types need their own special methods

        // starting with C# 9 it will be possible to merge the two methods below into one unique method with
        // *no* generic constraint, but then when T is a value type (struct) the 'value == null' comparison
        // will still always have to box the value beforehand, and we should benchmark to see what makes sense.

        private void WriteArrayOfNullable<T>(string name, FieldKind kind, T?[]? value, Action<ObjectDataOutput, T> write)
            where T : struct
        {
            var field = GetValidField(name, kind);

            if (value == null)
            {
                _offsets![field.Index] = -1;
                return;
            }

            _offsets![field.Index] = _dataPosition - DataStartPosition;
            _output.MoveTo(_dataPosition);

            var offsets = new int[value.Length];
            var arrayStartPosition = _output.Position;

            _output.WriteInt(0);
            _output.WriteInt(value.Length);

            var arrayDataPosition = _output.Position;

            for (var i = 0; i < value.Length; i++)
            {
                var v = value[i];
                if (v.HasValue)
                {
                    offsets[i] = _output.Position - arrayDataPosition;
                    write(_output, v.Value);
                }
                else
                {
                    offsets[i] = -1;
                }
            }

            for (var i = 0; i < value.Length; i++)
            {
                _output.WriteInt(offsets[i]);
            }

            _dataPosition = _output.Position;
            _output.MoveTo(arrayStartPosition);
            _output.WriteInt(_dataPosition - arrayDataPosition);
        }

        private void WriteArrayOfReference<T>(string name, FieldKind kind, T?[]? value, Action<ObjectDataOutput, T> write)
            where T : class
        {
            var field = GetValidField(name, kind);

            if (value == null)
            {
                _offsets![field.Index] = -1;
                return;
            }

            _offsets![field.Index] = _dataPosition - DataStartPosition;
            _output.MoveTo(_dataPosition);

            var offsets = new int[value.Length];
            var arrayStartPosition = _output.Position;

            _output.WriteInt(0);
            _output.WriteInt(value.Length);

            var arrayDataPosition = _output.Position;

            for (var i = 0; i < value.Length; i++)
            {
                var v = value[i];
                if (v != null)
                {
                    offsets[i] = _output.Position - arrayDataPosition;
                    write(_output, v);
                }
                else
                {
                    offsets[i] = -1;
                }
            }

            for (var i = 0; i < value.Length; i++)
            {
                _output.WriteInt(offsets[i]);
            }

            _dataPosition = _output.Position;
            _output.MoveTo(arrayStartPosition);
            _output.WriteInt(_dataPosition - arrayDataPosition);
        }

        public void WriteBoolean(string name, bool value)
        {
            var (position, offset) = GetBooleanFieldPosition(name);
            _output.MoveTo(position, BytesExtensions.SizeOfByte);
            var bits = (byte)(value ? 1 << offset : 0);
            var mask = (byte)(1 << offset);
            _output.WriteBits(bits, mask);
        }

        public void WriteNullableBoolean(string name, bool? value)
            => WriteNullable(name, FieldKind.NullableBoolean, value, (output, v) => output.WriteBoolean(v));

        public void WriteArrayOfBoolean(string name, bool[]? value)
        {
            var field = GetValidField(name, FieldKind.ArrayOfBoolean);

            if (value == null)
            {
                _offsets![field.Index] = -1;
                return;
            }

            _offsets![field.Index] = _output.Position;

            _output.WriteInt(value.Length);

            for (var i = 0; i < value.Length;)
            {
                var bits = (byte)0;
                var mask = (byte)0;
                for (var n = 7; i < value.Length && n >= 0; n--)
                {
                    var x = (byte)(1 << n);
                    if (value[i++]) bits &= x;
                    mask &= x;
                }
                _output.WriteBits(bits, mask);
            }
        }

        public void WriteArrayOfNullableBoolean(string name, bool?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfNullableBoolean, value, (output, v) => output.WriteBoolean(v));

        public void WriteInt8(string name, sbyte value)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int8);
            _output.MoveTo(position, BytesExtensions.SizeOfByte);
            _output.WriteSByte(value);
        }

        public void WriteNullableInt8(string name, sbyte? value)
            => WriteNullable(name, FieldKind.Int8, value, (output, v) => output.WriteSByte(v));

        public void WriteArrayOfInt8(string name, sbyte[]? value)
            => WriteReference(name, FieldKind.ArrayOfInt8, value, (output, v) => output.WriteSByteArray(v));

        public void WriteArrayOfNullableInt8(string name, sbyte?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfNullableInt8, value, (output, v) => output.WriteSByte(v));

        public void WriteInt16(string name, short value)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int16);
            _output.MoveTo(position, BytesExtensions.SizeOfShort);
            _output.WriteShort(value);
        }

        public void WriteNullableInt16(string name, short? value)
            => WriteNullable(name, FieldKind.Int16, value, (output, v) => output.WriteShort(v));

        public void WriteArrayOfInt16(string name, short[]? value)
            => WriteReference(name, FieldKind.ArrayOfInt16, value, (output, v) => output.WriteShortArray(v));

        public void WriteArrayOfNullableInt16(string name, short?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfNullableInt16, value, (output, v) => output.WriteShort(v));

        public void WriteInt32(string name, int value)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int32);
            _output.MoveTo(position, BytesExtensions.SizeOfInt);
            _output.WriteInt(value);
        }

        public void WriteNullableInt32(string name, int? value)
            => WriteNullable(name, FieldKind.Int32, value, (output, v) => output.WriteInt(v));

        public void WriteArrayOfInt32(string name, int[]? value)
            => WriteReference(name, FieldKind.ArrayOfInt32, value, (output, v) => output.WriteIntArray(v));

        public void WriteArrayOfNullableInt32(string name, int?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfNullableInt32, value, (output, v) => output.WriteInt(v));

        public void WriteInt64(string name, long value)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int64);
            _output.MoveTo(position, BytesExtensions.SizeOfLong);
            _output.WriteLong(value);
        }

        public void WriteNullableInt64(string name, long? value)
            => WriteNullable(name, FieldKind.Int64, value, (output, v) => output.WriteLong(v));

        public void WriteArrayOfInt64(string name, long[]? value)
            => WriteReference(name, FieldKind.ArrayOfInt64, value, (output, v) => output.WriteLongArray(v));

        public void WriteArrayOfNullableInt64(string name, long?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfNullableInt64, value, (output, v) => output.WriteLong(v));

        public void WriteFloat32(string name, float value)
        {
            var position = GetValueFieldPosition(name, FieldKind.Float32);
            _output.MoveTo(position, BytesExtensions.SizeOfFloat);
            _output.WriteFloat(value);
        }

        public void WriteNullableFloat32(string name, float? value)
            => WriteNullable(name, FieldKind.Float32, value, (output, v) => output.WriteFloat(v));

        public void WriteArrayOfFloat32(string name, float[]? value)
            => WriteReference(name, FieldKind.ArrayOfFloat32, value, (output, v) => output.WriteFloatArray(v));

        public void WriteArrayOfNullableFloat32(string name, float?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfNullableFloat32, value, (output, v) => output.WriteFloat(v));

        public void WriteFloat64(string name, double value)
        {
            var position = GetValueFieldPosition(name, FieldKind.Float64);
            _output.MoveTo(position, BytesExtensions.SizeOfDouble);
            _output.WriteDouble(value);
        }

        public void WriteNullableFloat64(string name, double? value)
            => WriteNullable(name, FieldKind.NullableFloat64, value, (output, v) => output.WriteDouble(v));

        public void WriteArrayOfFloat64(string name, double[]? value)
            => WriteReference(name, FieldKind.ArrayOfFloat64, value, (output, v) => output.WriteDoubleArray(v));

        public void WriteArrayOfNullableFloat64(string name, double?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfNullableFloat64, value, (output, v) => output.WriteDouble(v));

        public void WriteNullableString(string name, string? value)
            => WriteReference(name, FieldKind.NullableString, value, (output, v) => output.WriteString(v));

        public void WriteArrayOfNullableString(string name, string?[]? value)
            => WriteArrayOfReference(name, FieldKind.ArrayOfNullableString, value, (output, v) => output.WriteString(v));

        public void WriteNullableDecimal(string name, HBigDecimal? value)
            => WriteNullable(name, FieldKind.NullableDecimal, value, (output, v) => output.WriteBigDecimal(v));

        public void WriteArrayOfNullableDecimal(string name, HBigDecimal?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfNullableDecimal, value, (output, v) => output.WriteBigDecimal(v));

        public void WriteNullableTime(string name, TimeSpan? value)
        {
            // beware of range-check
            throw new NotImplementedException(); // FIXME - implement WriteTimeRef
        }

        public void WriteArrayOfNullableTime(string name, TimeSpan?[]? value)
        {
            // beware of range-check
            throw new NotImplementedException(); // FIXME - implement WriteTimeRefs
        }

        public void WriteNullableDate(string name, DateTime? value)
        {
            throw new NotImplementedException(); // FIXME - implement WriteDateRef
        }

        public void WriteArrayOfNullableDate(string name, DateTime?[]? value)
        {
            throw new NotImplementedException(); // FIXME - implement WriteDateRefs
        }

        public void WriteNullableTimeStamp(string name, DateTime? value)
        {
            throw new NotImplementedException(); // FIXME - implement WriteTimeStampRef
        }

        public void WriteArrayOfNullableTimeStamp(string name, DateTime?[]? value)
        {
            throw new NotImplementedException(); // FIXME - implement WriteTimeStampRefs
        }

        public void WriteNullableTimeStampWithTimeZone(string name, DateTimeOffset? value)
        {
            throw new NotImplementedException(); // FIXME - implement WriteTimeStampWithTimeZoneRef
        }

        public void WriteArrayOfNullableTimeStampWithTimeZone(string name, DateTimeOffset?[]? value)
        {
            throw new NotImplementedException(); // FIXME - implement WriteTimeStampWithTimeZoneRefs
        }

        // NOTE
        // Java has
        // - WriteCompact for writing a compact object (any kind of object, which is compact-serialized)
        // - WriteGenericRecord for writing a generic record (the written value has to be a compact generic record object)
        // We want
        // - WriteObject which will do different things depending on what the object is (generic record...)

        public void WriteNullableCompact(string name, object? value)
            => WriteReference(name, FieldKind.NullableCompact, value, (output, v) => Serializer.WriteObject(output, v));

        public void WriteArrayOfNullableCompact(string name, object?[]? value)
            => WriteArrayOfReference(name, FieldKind.ArrayOfNullableCompact, value, (output, v) => Serializer.WriteObject(output, v));
    }
}
