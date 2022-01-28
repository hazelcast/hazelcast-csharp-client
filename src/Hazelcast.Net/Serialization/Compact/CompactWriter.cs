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

namespace Hazelcast.Serialization.Compact
{
    // notes:
    // when passing a lambda as an Action<> parameter, the compiler generates a supporting
    // class which statically caches the Action<> object so it's only instantiated once.

    // TODO: add support for writing HBigDecimal (and what else?)

    internal class CompactWriter : CompactReaderWriterBase, ICompactWriter
    {
        private readonly ObjectDataOutput _output;
        private readonly int[]? _offsets;

        private int _dataPosition;
        private bool _completed;

        public CompactWriter(ObjectDataOutput output, Schema schema)
            : base(schema, output.Position)
        {
            _output = output;

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

        private void WriteReference<T>(string name, FieldKind kind, T value, Action<ObjectDataOutput, T> write)
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

        public void WriteBooleanRef(string name, bool? value)
            => WriteNullable(name, FieldKind.BooleanRef, value, (output, v) => output.WriteBoolean(v));

        public void WriteBooleans(string name, bool[]? value)
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

        public void WriteBooleanRefs(string name, bool?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfBooleanRef, value, (output, v) => output.WriteBoolean(v));

        public void WriteSignedByte(string name, sbyte value)
        {
            var position = GetValueFieldPosition(name, FieldKind.SignedInteger8);
            _output.MoveTo(position, BytesExtensions.SizeOfByte);
            _output.WriteSByte(value);
        }

        public void WriteSignedByteRef(string name, sbyte? value)
            => WriteNullable(name, FieldKind.SignedInteger8, value, (output, v) => output.WriteSByte(v));

        public void WriteSignedBytes(string name, sbyte[]? value)
            => WriteReference(name, FieldKind.ArrayOfSignedInteger8, value, (output, v) => output.WriteSByteArray(v));

        public void WriteSignedByteRefs(string name, sbyte?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfSignedInteger8Ref, value, (output, v) => output.WriteSByte(v));

        public void WriteShort(string name, short value)
        {
            var position = GetValueFieldPosition(name, FieldKind.SignedInteger16);
            _output.MoveTo(position, BytesExtensions.SizeOfShort);
            _output.WriteShort(value);
        }

        public void WriteShortRef(string name, short? value)
            => WriteNullable(name, FieldKind.SignedInteger16, value, (output, v) => output.WriteShort(v));

        public void WriteShorts(string name, short[]? value)
            => WriteReference(name, FieldKind.ArrayOfSignedInteger16, value, (output, v) => output.WriteShortArray(v));

        public void WriteShortRefs(string name, short?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfSignedInteger16Ref, value, (output, v) => output.WriteShort(v));

        public void WriteInt(string name, int value)
        {
            var position = GetValueFieldPosition(name, FieldKind.SignedInteger32);
            _output.MoveTo(position, BytesExtensions.SizeOfInt);
            _output.WriteInt(value);
        }

        public void WriteIntRef(string name, int? value)
            => WriteNullable(name, FieldKind.SignedInteger32, value, (output, v) => output.WriteInt(v));

        public void WriteInts(string name, int[]? value)
            => WriteReference(name, FieldKind.ArrayOfSignedInteger32, value, (output, v) => output.WriteIntArray(v));

        public void WriteIntRefs(string name, int?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfSignedInteger32Ref, value, (output, v) => output.WriteInt(v));

        public void WriteLong(string name, long value)
        {
            var position = GetValueFieldPosition(name, FieldKind.SignedInteger64);
            _output.MoveTo(position, BytesExtensions.SizeOfLong);
            _output.WriteLong(value);
        }

        public void WriteLongRef(string name, long? value)
            => WriteNullable(name, FieldKind.SignedInteger64, value, (output, v) => output.WriteLong(v));

        public void WriteLongs(string name, long[]? value)
            => WriteReference(name, FieldKind.ArrayOfSignedInteger64, value, (output, v) => output.WriteLongArray(v));

        public void WriteLongRefs(string name, long?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfSignedInteger64Ref, value, (output, v) => output.WriteLong(v));

        public void WriteFloat(string name, float value)
        {
            var position = GetValueFieldPosition(name, FieldKind.Float);
            _output.MoveTo(position, BytesExtensions.SizeOfFloat);
            _output.WriteFloat(value);
        }

        public void WriteFloatRef(string name, float? value)
            => WriteNullable(name, FieldKind.Float, value, (output, v) => output.WriteFloat(v));

        public void WriteFloats(string name, float[]? value)
            => WriteReference(name, FieldKind.ArrayOfFloat, value, (output, v) => output.WriteFloatArray(v));

        public void WriteFloatRefs(string name, float?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfFloatRef, value, (output, v) => output.WriteFloat(v));

        public void WriteDouble(string name, double value)
        {
            var position = GetValueFieldPosition(name, FieldKind.Double);
            _output.MoveTo(position, BytesExtensions.SizeOfDouble);
            _output.WriteDouble(value);
        }

        public void WriteDoubleRef(string name, double? value)
            => WriteNullable(name, FieldKind.DoubleRef, value, (output, v) => output.WriteDouble(v));

        public void WriteDoubles(string name, double[]? value)
            => WriteReference(name, FieldKind.ArrayOfDouble, value, (output, v) => output.WriteDoubleArray(v));

        public void WriteDoubleRefs(string name, double?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfDoubleRef, value, (output, v) => output.WriteDouble(v));

        public void WriteString(string name, string? value)
            => WriteReference(name, FieldKind.String, value, (output, v) => output.WriteString(v));

        public void WriteStrings(string name, string?[]? value)
            => WriteArrayOfReference(name, FieldKind.ArrayOfString, value, (output, v) => output.WriteString(v));

        public void WriteDecimalRef(string name, decimal? value)
            => WriteNullable(name, FieldKind.DecimalRef, value, (output, v) => output.WriteBigDecimal(v));

        public void WriteDecimalRefs(string name, decimal?[]? value)
            => WriteArrayOfNullable(name, FieldKind.ArrayOfDecimalRef, value, (output, v) => output.WriteBigDecimal(v));

        public void WriteTime(string name, TimeSpan? value)
        {
            // TODO: range-check
            throw new NotImplementedException();
        }

        public void WriteTimes(string name, TimeSpan?[]? value)
        {
            throw new NotImplementedException();
        }

        public void WriteDate(string name, DateTime? value)
        {
            throw new NotImplementedException();
        }

        public void WriteDates(string name, DateTime?[]? value)
        {
            throw new NotImplementedException();
        }

        public void WriteDateTime(string name, DateTime? value)
        {
            throw new NotImplementedException();
        }

        public void WriteDateTimes(string name, DateTime?[]? value)
        {
            throw new NotImplementedException();
        }

        public void WriteDateTimeOffset(string name, DateTimeOffset? value)
        {
            throw new NotImplementedException();
        }

        public void WriteDateTimeOffsets(string name, DateTimeOffset?[]? value)
        {
            throw new NotImplementedException();
        }

        public void WriteObject(string name, object? value)
            // FIXME Java uses serializer.writeObject(out, val, includeSchemaOnBinary)
            // where serializer is a CompactStreamSerializer which is passed along with includeSchemaOnBinary when creating the writer
            //   CompactStreamSerializer is a StreamSerializer<object> and yes we have the IStreamSerializer<T> interface in csharp
            // and it's only used for writing objects and generic records really
            //=> WriteReference(name, FieldKind.Object, value, (output, v) => output.WriteObject(v));
            => throw new NotImplementedException();

        public void WriteObjects(string name, object?[]? value)
            //=> WriteReference(name, FieldKind.ArrayOfObject, value, (output, v) => output.WriteObjectArray(v));
            => throw new NotImplementedException();
    }
}
