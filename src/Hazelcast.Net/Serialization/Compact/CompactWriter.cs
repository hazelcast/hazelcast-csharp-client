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
using System.Linq;
using System.Numerics;
using Hazelcast.Core;
using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact
{
    // notes:
    // when passing a lambda as an Action<> parameter, the compiler generates a supporting
    // class which statically caches the Action<> object so it's only instantiated once.

    internal class CompactWriter : CompactReaderWriterBase, ICompactWriter
    {
        private readonly IWriteObjectsToObjectDataOutput _objectsWriter;
        private readonly ObjectDataOutput _output;
        private readonly int[]? _offsets;

        private int _dataPosition;
        private bool _completed;

        public CompactWriter(IWriteObjectsToObjectDataOutput objectsWriter, ObjectDataOutput output, Schema schema)
            : base(schema, output.Position)
        {
            _objectsWriter = objectsWriter ?? throw new ArgumentNullException(nameof(objectsWriter));
            _output = output ?? throw new ArgumentNullException(nameof(output));

            if (schema.HasReferenceFields) _offsets = new int[schema.ReferenceFieldCount];

            _dataPosition = DataStartPosition + schema.ValueFieldLength;
        }

        protected override SchemaField GetValidField(string name)
        {
            if (_completed)
                throw new InvalidOperationException("Cannot write to a completed CompactWriter.");
            return base.GetValidField(name);
        }

        private static Action<ObjectDataOutput, int> GetOffsetWriter(int dataLength)
        {
            if (dataLength < byte.MaxValue) return (output, offset) => output.WriteByte(offset < 0 ? byte.MaxValue : (byte)offset);
            if (dataLength < ushort.MaxValue) return (output, offset) => output.WriteUShort(offset < 0 ? ushort.MaxValue : (ushort)offset);
            return (output, offset) => output.WriteInt(offset); // specs say "otherwise offset are i32"
        }

        public void Complete()
        {
            if (_completed) return;

            _completed = true;

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

        // starting with C# 9 it is possible to merge the two methods below into one unique method with
        // *no* generic constraint, but then when T is a value type (struct) the 'value == null' comparison
        // will still always have to box the value beforehand, and we should benchmark to see what makes sense.

        private void WriteNullable<T>(string name, FieldKind kind, T? value, Action<ObjectDataOutput, T> write)
            where T : struct
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
        {
            var field = GetValidField(name, kind);

            if (value == null) // boxes structs = works with T being Nullable<>
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

            var arrayDataLength = _output.Position - arrayDataPosition;
            var offsetWriter = GetOffsetWriter(arrayDataLength);
            for (var i = 0; i < value.Length; i++) offsetWriter(_output, offsets[i]);

            _dataPosition = _output.Position;
            _output.MoveTo(arrayStartPosition);
            _output.WriteInt(arrayDataLength);
        }

        private void WriteArrayOfReference<T>(string name, FieldKind kind, T?[]? value, Action<ObjectDataOutput, T> write)
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
                if (v != null) // boxes structs = works with T being Nullable<>
                {
                    offsets[i] = _output.Position - arrayDataPosition;
                    write(_output, v);
                }
                else
                {
                    offsets[i] = -1;
                }
            }

            var arrayDataLength = _output.Position - arrayDataPosition;
            var offsetWriter = GetOffsetWriter(arrayDataLength);
            for (var i = 0; i < value.Length; i++) offsetWriter(_output, offsets[i]);

            _dataPosition = _output.Position;
            _output.MoveTo(arrayStartPosition);
            _output.WriteInt(arrayDataLength);
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

            _offsets![field.Index] = _dataPosition - DataStartPosition;
            _output.MoveTo(_dataPosition);

            _output.WriteInt(value.Length);

            for (var i = 0; i < value.Length;)
            {
                var bits = (byte)0;
                var mask = (byte)0;
                for (var n = 7; i < value.Length && n >= 0; n--)
                {
                    var x = (byte)(1 << n);
                    if (value[i++]) bits |= x;
                    mask |= x;
                }
                _output.WriteBits(bits, mask);
            }

            _dataPosition = _output.Position;
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
            => WriteNullable(name, FieldKind.NullableInt8, value, (output, v) => output.WriteSByte(v));

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
            => WriteNullable(name, FieldKind.NullableInt16, value, (output, v) => output.WriteShort(v));

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
            => WriteNullable(name, FieldKind.NullableInt32, value, (output, v) => output.WriteInt(v));

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
            => WriteNullable(name, FieldKind.NullableInt64, value, (output, v) => output.WriteLong(v));

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
            => WriteNullable(name, FieldKind.NullableFloat32, value, (output, v) => output.WriteFloat(v));

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

        public void WriteString(string name, string? value)
            => WriteReference(name, FieldKind.String, value, (output, v) => output.WriteString(v));

        public void WriteArrayOfString(string name, string?[]? value)
            => WriteArrayOfReference(name, FieldKind.ArrayOfString, value, (output, v) => output.WriteString(v));

        private static void WriteBigInteger(ObjectDataOutput output, BigInteger value)
        {
            // <bigint> := <length:i32> <byte:u8>*
            // where
            //   <length> is the number of <byte> items
            //   <byte>* is the byte array containing the two's complement representation of the integer in BIG_ENDIAN byte-order
            //      and contains the minimum number of bytes required, including at least one sign bit
            
            var bytes = value.ToByteArray(false, true); // signed, big-endian
            output.WriteByteArray(bytes);
        }
        
        private static void WriteBigDecimal(ObjectDataOutput output, HBigDecimal value)
        {
            WriteBigInteger(output, value.UnscaledValue);
            output.WriteInt(value.Scale);
        }
        
        public void WriteDecimal(string name, HBigDecimal? value)
            => WriteNullable(name, FieldKind.Decimal, value, WriteBigDecimal);

        public void WriteArrayOfDecimal(string name, HBigDecimal?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfDecimal, value, WriteBigDecimal);

        private static void WriteTime(ObjectDataOutput output, HLocalTime value)
        {
            // time is hour:i8 minute:i8 second:i8 nanosecond:i32
            output.WriteByte(value.Hour);
            output.WriteByte(value.Minute);
            output.WriteByte(value.Second);
            output.WriteInt(value.Nanosecond);
        }
        
        public void WriteTime(string name, HLocalTime? value)
            => WriteNullable(name, FieldKind.Time, value, WriteTime);

        public void WriteArrayOfTime(string name, HLocalTime?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfTime, value, WriteTime);

        private static void WriteDate(ObjectDataOutput output, HLocalDate value)
        {
            // date is year:i32 month:i8 day:i8
            output.WriteInt(value.Year);
            output.WriteByte(value.Month);
            output.WriteByte(value.Day);
        }
        
        public void WriteDate(string name, HLocalDate? value)
            => WriteNullable(name, FieldKind.Date, value, WriteDate);

        public void WriteArrayOfDate(string name, HLocalDate?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfDate, value, WriteDate);

        private static void WriteTimeStamp(ObjectDataOutput output, HLocalDateTime value)
        {
            WriteDate(output, value.Date);
            WriteTime(output, value.Time);
        }
        
        public void WriteTimeStamp(string name, HLocalDateTime? value)
            => WriteNullable(name, FieldKind.TimeStamp, value, WriteTimeStamp);

        public void WriteArrayOfTimeStamp(string name, HLocalDateTime?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfTimeStamp, value, WriteTimeStamp);

        private static void WriteTimeStampWithTimeZone(ObjectDataOutput output, HOffsetDateTime value)
        {
            if (value.Offset.Ticks % TimeSpan.FromSeconds(1).Ticks != 0)
                throw new SerializationException("Cannot compact-serialize HOffsetDateTime value with greater-than-second offset precision.");
            WriteTimeStamp(output, value.LocalDateTime);
            output.WriteInt((int) value.Offset.TotalSeconds);
        }
        
        public void WriteTimeStampWithTimeZone(string name, HOffsetDateTime? value)
            => WriteNullable(name, FieldKind.TimeStampWithTimeZone, value, WriteTimeStampWithTimeZone);

        public void WriteArrayOfTimeStampWithTimeZone(string name, HOffsetDateTime?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfTimeStampWithTimeZone, value, WriteTimeStampWithTimeZone);

        // WriteReference & WriteArrayOfReference do handle Nullable<T> correctly, will detect nulls,
        // but when not null, since ObjectsReaderWriter.Write accepts an object, potential Nullable<T>
        // values will be boxed and therefore be T (so we'll look for a schema for T, not T?) = all good

        public void WriteCompact<T>(string name, T? value)
            => WriteReference(name, FieldKind.Compact, value, (output, v) => _objectsWriter.Write(output, v));

        public void WriteArrayOfCompact<T>(string name, T?[]? value)
        {
            // verify that all object in the array are exactly of type T - there is zero reason for this
            // constraint other than it is enforced in Java and we want to be identical to Java, but even
            // in Java this constraint could probably be lifted without problems.
            if (value != null)
            {
                var typeOfT = typeof(T);
                var error = value.FirstOrDefault(x => x != null && x.GetType() != typeOfT);
                if (error != null)
                    throw new SerializationException("It is not allowed to serialize an array of Compact serializable "
                                                         + $"objects containing different item types. A {typeOfT.Name}[] array "
                                                         + $"cannot contain an object of type {error.GetType().Name}.");
            }

            WriteArrayOfReference(name, FieldKind.ArrayOfCompact, value, (output, v) => _objectsWriter.Write(output, v));
        } 
    }
}
