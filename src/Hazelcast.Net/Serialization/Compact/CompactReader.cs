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
using System.Numerics;
using Hazelcast.Core;
using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact
{
    internal class CompactReader : CompactReaderWriterBase, ICompactReader
    {
        private readonly ObjectDataInput _input;
        private readonly int _dataLength;
        private readonly int _offsetPosition;
        private readonly Func<ObjectDataInput, int, int, int> _offsetReader;

        public CompactReader(IReadWriteObjectsFromIObjectDataInputOutput serializer, ObjectDataInput input, Schema schema, Type objectType)
            : base(serializer, schema, input.Position)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));

            if (schema.HasReferenceFields)
            {
                _dataLength = input.ReadInt();
                _offsetPosition = DataStartPosition + _dataLength;
            }

            _offsetReader = GetOffsetReader(_dataLength);
        }

        public Type ObjectType { get; }

        private static Func<ObjectDataInput, int, int, int> GetOffsetReader(int dataLength)
        {
            if (dataLength < byte.MaxValue) return (input, start, index) =>
            {
                input.MoveTo(start + index * BytesExtensions.SizeOfByte);
                return input.ReadByte();
            };

            if (dataLength < ushort.MaxValue) return (input, start, index) =>
            {
                input.MoveTo(start + index * BytesExtensions.SizeOfShort);
                return input.ReadUShort();
            };

            return (input, start, index) =>
            {
                input.MoveTo(start + index * BytesExtensions.SizeOfInt);
                return input.ReadInt();
            };
        }

        private T? ReadNullable<T>(string name, FieldKind kind, Func<ObjectDataInput, T> read)
            where T : struct
        {
            var field = GetValidField(name, kind);
            var offset = _offsetReader(_input, _offsetPosition, field.Index);
            if (offset < 0) return null;

            _input.MoveTo(DataStartPosition + offset);
            return read(_input);
        }

        private T? ReadReference<T>(string name, FieldKind kind, Func<ObjectDataInput, T> read)
            where T : class
        {
            var field = GetValidField(name, kind);
            var offset = _offsetReader(_input, _offsetPosition, field.Index);
            if (offset < 0) return null;

            _input.MoveTo(DataStartPosition + offset);
            return read(_input);
        }

        private T?[]? ReadArrayOfNullable<T>(string name, FieldKind kind, Func<ObjectDataInput, T> read)
            where T : struct
        {
            var field = GetValidField(name, kind);
            var offset = _offsetReader(_input, _offsetPosition, field.Index);
            if (offset < 0) return null;

            _input.MoveTo(DataStartPosition + offset);

            var arrayDataLength = _input.ReadInt();
            var arrayOffsetReader = GetOffsetReader(arrayDataLength);

            var count = _input.ReadInt();
            var items = new T?[count];

            var arrayDataPosition = _input.Position;
            var arrayOffsetPosition = arrayDataPosition + arrayDataLength;
            for (var i = 0; i < count; i++)
            {
                var itemOffset = arrayOffsetReader(_input, arrayOffsetPosition, i);
                if (itemOffset < 0)
                {
                    items[i] = null;
                }
                else
                {
                    _input.MoveTo(arrayDataPosition + itemOffset);
                    items[i] = read(_input);
                }
            }

            return items;
        }

        private T?[]? ReadArrayOfReference<T>(string name, FieldKind kind, Func<ObjectDataInput, T> read)
            where T : class
        {
            var field = GetValidField(name, kind);
            var offset = _offsetReader(_input, _offsetPosition, field.Index);
            if (offset < 0) return null;

            _input.MoveTo(DataStartPosition + offset);

            var arrayDataLength = _input.ReadInt();
            var arrayOffsetReader = GetOffsetReader(arrayDataLength);

            var count = _input.ReadInt();
            var items = new T?[count];

            var arrayDataPosition = _input.Position;
            var arrayOffsetPosition = arrayDataPosition + arrayDataLength;
            for (var i = 0; i < count; i++)
            {
                var itemOffset = arrayOffsetReader(_input, arrayOffsetPosition, i);
                if (itemOffset < 0)
                {
                    items[i] = null;
                }
                else
                {
                    _input.MoveTo(arrayDataPosition + itemOffset);
                    items[i] = read(_input);
                }
            }

            return items;
        }

        public bool ReadBoolean(string name)
        {
            var (position, offset) = GetBooleanFieldPosition(name);

            // how we write:
            //_output.MoveTo(position, BytesExtensions.SizeOfByte);
            //var bits = (byte)(value ? 1 << offset : 0);
            //var mask = (byte)(1 << offset);
            //_output.WriteBits(bits, mask);
            
            // how we read:
            _input.MoveTo(position);
            var bits = _input.ReadByte();
            var mask = (byte)(1 << offset);
            return (bits & mask) != 0;
        }

        public bool? ReadNullableBoolean(string name)
            => ReadNullable(name, FieldKind.Boolean, input => input.ReadBoolean());

        public bool[]? ReadArrayOfBoolean(string name)
        {
            var field = GetValidField(name, FieldKind.ArrayOfBoolean);
            var offset = _offsetReader(_input, _offsetPosition, field.Index);
            if (offset < 0) return null;

            _input.MoveTo(DataStartPosition + offset);

            var length = _input.ReadInt();
            var value = new bool[length];
            for (var i = 0; i < value.Length; )
            {
                var bits = _input.ReadByte();
                var mask = (byte) 0b_1000_0000;
                for (var n = 7; i < value.Length && n >= 0; n--)
                {
                    value[i++] = (bits & mask) > 0;
                    mask = (byte) (mask >> 1);
                }
            }
            return value;
        }

        public bool?[]? ReadArrayOfNullableBoolean(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableBoolean, input => input.ReadBoolean());

        public sbyte ReadInt8(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int8);
            _input.MoveTo(position);
            return _input.ReadSByte();
        }

        public sbyte? ReadNullableInt8(string name)
            => ReadNullable(name, FieldKind.Int8, input => input.ReadSByte());

        public sbyte[]? ReadArrayOfInt8(string name)
            => ReadReference(name, FieldKind.ArrayOfInt8, input => input.ReadSByteArray());

        public sbyte?[]? ReadArrayOfNullableInt8(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableInt8, input => input.ReadSByte());

        public short ReadInt16(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int16);
            _input.MoveTo(position);
            return _input.ReadShort();
        }

        public short? ReadNullableInt16(string name)
            => ReadNullable(name, FieldKind.Int16, input => input.ReadShort());

        public short[]? ReadArrayOfInt16(string name)
            => ReadReference(name, FieldKind.ArrayOfInt16, input => input.ReadShortArray());

        public short?[]? ReadArrayOfNullableInt16(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableInt16, input => input.ReadShort());

        public int ReadInt32(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int32);
            _input.MoveTo(position);
            return _input.ReadInt();
        }

        public int? ReadNullableInt32(string name)
            => ReadNullable(name, FieldKind.Int32, input => input.ReadInt());

        public int[]? ReadArrayOfInt32(string name)
            => ReadReference(name, FieldKind.ArrayOfInt32, input => input.ReadIntArray());

        public int?[]? ReadArrayOfNullableInt32(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableInt32, input => input.ReadInt());

        public long ReadInt64(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int64);
            _input.MoveTo(position);
            return _input.ReadLong();
        }

        public long? ReadNullableInt64(string name)
            => ReadNullable(name, FieldKind.Int64, input => input.ReadLong());

        public long[]? ReadArrayOfInt64(string name)
            => ReadReference(name, FieldKind.ArrayOfInt64, input => input.ReadLongArray());

        public long?[]? ReadArrayOfNullableInt64(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableInt64, input => input.ReadLong());

        public float ReadFloat32(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Float32);
            _input.MoveTo(position);
            return _input.ReadFloat();
        }

        public float? ReadNullableFloat32(string name)
            => ReadNullable(name, FieldKind.Float32, input => input.ReadFloat());

        public float[]? ReadArrayOfFloat32(string name)
            => ReadReference(name, FieldKind.Float32, input => input.ReadFloatArray());

        public float?[]? ReadArrayOfNullableFloat32(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableFloat32, input => input.ReadFloat());

        public double ReadFloat64(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Float64);
            _input.MoveTo(position);
            return _input.ReadDouble();
        }

        public double? ReadNullableFloat64(string name)
            => ReadNullable(name, FieldKind.Float64, input => input.ReadDouble());

        public double[]? ReadArrayOfFloat64(string name)
            => ReadReference(name, FieldKind.ArrayOfFloat64, input => input.ReadDoubleArray());

        public double?[]? ReadArrayOfNullableFloat64(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableFloat64, input => input.ReadDouble());

        public string? ReadNullableString(string name)
            => ReadReference(name, FieldKind.NullableString, input => input.ReadString());

        public string?[]? ReadArrayOfNullableString(string name)
            => ReadArrayOfReference(name, FieldKind.ArrayOfNullableString, input => input.ReadString());

        private static BigInteger ReadBigInteger(ObjectDataInput input)
        {
            // <bigint> := <length:i32> <byte:u8>*
            // where
            //   <length> is the number of <byte> items
            //   <byte>* is the byte array containing the two's complement representation of the integer in BIG_ENDIAN byte-order
            //      and contains the minimum number of bytes required, including at least one sign bit

            var bytes = input.ReadByteArray();
#if NETSTANDARD2_0
            // the extended ctor does not exist in netstandard 2.0 and the existing ctor expects signed little-endian bytes
            // so we have to reverse the bytes before creating the BigInteger - for CompactWriter we provide a polyfill
            // extension method but for reading... we cannot provide a polyfill ctor
            Array.Reverse(bytes);
            return new BigInteger(bytes); // signed, little-endian
#else
            return new BigInteger(bytes, false, true); // signed, big-endian
#endif
        }

        private static HBigDecimal ReadBigDecimal(ObjectDataInput input)
        {
            var unscaled = ReadBigInteger(input);
            var scale = input.ReadInt();
            return new HBigDecimal(unscaled, scale);
        }
        
        public HBigDecimal? ReadNullableDecimal(string name)
            => ReadNullable(name, FieldKind.NullableDecimal, ReadBigDecimal);

        public HBigDecimal?[]? ReadArrayOfNullableDecimal(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableDecimal, ReadBigDecimal);

        private HLocalTime ReadTime(ObjectDataInput input)
        {
            // time is hour:i8 minute:i8 second:i8 nanosecond:i32
            var hour = _input.ReadByte();
            var minute = _input.ReadByte();
            var second = _input.ReadByte();
            var nanosecond = _input.ReadInt();
            return new HLocalTime(hour, minute, second, nanosecond);
        }
        
        public HLocalTime? ReadNullableTime(string name)
            => ReadNullable(name, FieldKind.NullableTime, ReadTime);

        public HLocalTime?[]? ReadArrayOfNullableTime(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableTime, ReadTime);

        private HLocalDate ReadDate(ObjectDataInput input)
        {
            // date is year:i32 month:i8 day:i8
            var year = _input.ReadInt();
            var month = _input.ReadByte();
            var day = _input.ReadByte();
            return new HLocalDate(year, month, day);
        }

        public HLocalDate? ReadNullableDate(string name)
            => ReadNullable(name, FieldKind.NullableDate, ReadDate);

        public HLocalDate?[]? ReadArrayOfNullableDate(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableDate, ReadDate);

        private HLocalDateTime ReadTimeStamp(ObjectDataInput input)
        {
            var date = ReadDate(input);
            var time = ReadTime(input);
            return new HLocalDateTime(date, time);
        }
        
        public HLocalDateTime? ReadNullableTimeStamp(string name)
            => ReadNullable(name, FieldKind.NullableTimeStamp, ReadTimeStamp);

        public HLocalDateTime?[]? ReadArrayOfNullableTimeStamp(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableTimeStamp, ReadTimeStamp);

        private HOffsetDateTime ReadTimeStampWithTimeZone(ObjectDataInput input)
        {
            var timestamp = ReadTimeStamp(input);
            var offset = input.ReadInt();
            return new HOffsetDateTime(timestamp, offset);
        }
        
        public HOffsetDateTime? ReadNullableTimeStampWithTimeZone(string name)
            => ReadNullable(name, FieldKind.NullableTimeStampWithTimeZone, ReadTimeStampWithTimeZone);

        public HOffsetDateTime?[]? ReadArrayOfNullableTimeStampWithTimeZone(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableTimeStampWithTimeZone, ReadTimeStampWithTimeZone);

        public T? ReadNullableCompact<T>(string name)
            where T: class
            => ReadReference(name, FieldKind.NullableCompact, input => ObjectsReaderWriter.Read<T>(input));

        public T?[]? ReadArrayOfNullableCompact<T>(string name)
            where T : class
            => ReadArrayOfReference(name, FieldKind.ArrayOfNullableCompact, input => ObjectsReaderWriter.Read<T>(input));
    }
}
