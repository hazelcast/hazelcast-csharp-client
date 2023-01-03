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

using System;
using System.Numerics;
using Hazelcast.Core;
using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact
{
    internal class CompactReader : CompactReaderWriterBase, ICompactReader
    {
        private readonly IReadObjectsFromObjectDataInput _objectsReader;
        private readonly ObjectDataInput _input;
        private readonly int _offsetPosition;
        private readonly Func<ObjectDataInput, int, int, int>? _offsetReader;

        public CompactReader(IReadObjectsFromObjectDataInput objectsReader, ObjectDataInput input, Schema schema, Type objectType)
            : base(schema, input.Position)
        {
            _objectsReader = objectsReader ?? throw new ArgumentNullException(nameof(objectsReader));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            ObjectType = objectType ?? throw new ArgumentNullException(nameof(objectType));

            if (schema.HasReferenceFields)
            {
                var dataLength = input.ReadInt();
                _offsetPosition = DataStartPosition + dataLength;
                _offsetReader = GetOffsetReader(dataLength);
            }
        }

        public Type ObjectType { get; }

        /// <summary>
        /// Gets the <see cref="FieldKind"/> of a field.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The <see cref="FieldKind"/> of the field, which can be <see cref="FieldKind.NotAvailable"/> if the field does not exist.</returns>
        public FieldKind GetFieldKind(string name) => Schema.TryGetField(name, out var field) ? field.Kind : FieldKind.NotAvailable;

        internal static Func<ObjectDataInput, int, int, int> GetOffsetReader(int dataLength)
        {
            if (dataLength < byte.MaxValue) return (input, start, index) =>
            {
                input.MoveTo(start + index * BytesExtensions.SizeOfByte);
                var offset = input.ReadByte();
                return offset == byte.MaxValue ? -1 : offset;
            };

            if (dataLength < ushort.MaxValue) return (input, start, index) =>
            {
                input.MoveTo(start + index * BytesExtensions.SizeOfShort);
                var offset = input.ReadUShort();
                return offset == ushort.MaxValue ? -1 : offset;
            };

            return (input, start, index) =>
            {
                input.MoveTo(start + index * BytesExtensions.SizeOfInt);
                return input.ReadInt(); // specs say "otherwise offset are i32"
            };
        }

        #region Helpers

        // reads a value of a struct type that can be null (value is Nullable<T>)
        private T? ReadNullable<T>(string name, FieldKind kind, Func<ObjectDataInput, T> read)
            where T : struct
        {
            var field = GetValidField(name, kind);
            return ReadNullable(field, read);
        }

        private T? ReadNullable<T>(SchemaField field, Func<ObjectDataInput, T> read)
            where T : struct
        {
            var offset = _offsetReader!(_input, _offsetPosition, field.Index);
            if (offset < 0) return null;

            _input.MoveTo(DataStartPosition + offset);
            return read(_input);
        }

        // reads a value of a struct type that can be null or not-null, and the value is expected to be not-null (value is T)
        private T ReadMaybeNullableNotNull<T>(string name, FieldKind kind, FieldKind nullableKind, Func<ObjectDataInput, T> read)
            where T : struct
        {
            var value = ReadMaybeNullable(name, kind, nullableKind, read);
            if (value.HasValue) return value.Value;
            throw new SerializationException($"Null value for field \"{name}\" of schema {Schema}, which is of kind \"{nullableKind}\".");
        }

        // reads a value of a struct type that can be null or not-null, and the value can be null (value is Nullable<T>)
        private T? ReadMaybeNullable<T>(string name, FieldKind kind, FieldKind nullableKind, Func<ObjectDataInput, T> read)
            where T : struct
        {
            var field = GetValidField(name);

            int offset;
            if (field.Kind == kind)
            {
                offset = field.Offset;
            }
            else if (field.Kind == nullableKind)
            {
                offset = _offsetReader!(_input, _offsetPosition, field.Index);
                if (offset < 0) return default;
            }
            else
            {
                throw new SerializationException($"Invalid kind \"{kind}\" or \"{nullableKind}\" for field \"{name}\" of schema {Schema}, which is of kind \"{field.Kind}\".");
            }

            _input.MoveTo(DataStartPosition + offset);
            return read(_input);
        }

        // reads an array of values of a struct type that can be null (value is Nullable<T>[])
        private T?[]? ReadArrayOfNullable<T>(string name, FieldKind kind, Func<ObjectDataInput, T> read)
            where T : struct
        {
            return ReadArrayOfNullable(GetValidField(name, kind), read);
        }

        private T?[]? ReadArrayOfNullable<T>(SchemaField field, Func<ObjectDataInput, T> read)
            where T : struct
        {
            var offset = _offsetReader!(_input, _offsetPosition, field.Index);
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

        // reads an array of values of a struct type that can be null or non-null, and the values are expected to be not-null (value is T[])
        private T[]? ReadArrayOfMaybeNullableNotNull<T>(string name, FieldKind kind, FieldKind nullableKind, Func<ObjectDataInput, T> read, Func<ObjectDataInput, T[]> readArray)
            where T : struct
        {
            var field = GetValidField(name);

            if (field.Kind == kind)
            {
                var offset = _offsetReader!(_input, _offsetPosition, field.Index);
                if (offset < 0) return default;

                _input.MoveTo(DataStartPosition + offset);
                return readArray(_input);
            }

            if (field.Kind == nullableKind)
            {
                var values = ReadArrayOfNullable(field, read);
                if (values == null) return null;
                var array = new T[values.Length];
                for (var i = 0; i < values.Length; i++) array[i] = values[i] ?? throw new SerializationException($"Null value in field \"{name}\" of schema {Schema}, which is of kind \"{field.Kind}\".");
                return array;
            }

            throw new SerializationException($"Invalid kind \"{kind}\" or \"{nullableKind}\" for field \"{name}\" of schema {Schema}, which is of kind \"{field.Kind}\".");
        }

        // reads an array of values of a struct type that can be null or non-null, and the values can be null (value is Nullable<T>[])
        private T?[]? ReadArrayOfMaybeNullable<T>(string name, FieldKind kind, FieldKind nullableKind, Func<ObjectDataInput, T> read, Func<ObjectDataInput, T[]> readArray)
            where T : struct
        {
            var field = GetValidField(name);

            if (field.Kind == kind)
            {
                var offset = _offsetReader!(_input, _offsetPosition, field.Index);
                if (offset < 0) return default;

                _input.MoveTo(DataStartPosition + offset);
                var values = readArray(_input);
                var array = new T?[values.Length];
                for (var i = 0; i < values.Length; i++) array[i] = values[i];
                return array;
            }

            if (field.Kind == nullableKind)
            {
                return ReadArrayOfNullable(field, read);
            }

            throw new SerializationException($"Invalid kind \"{kind}\" or \"{nullableKind}\" for field \"{name}\" of schema {Schema}, which is of kind \"{field.Kind}\".");
        }

        // reads a value of a class type that can be null (value is T)
        private T? ReadReference<T>(string name, FieldKind kind, Func<ObjectDataInput, T> read)
            where T : class
        {
            var field = GetValidField(name, kind);
            var offset = _offsetReader!(_input, _offsetPosition, field.Index);
            if (offset < 0) return default;

            _input.MoveTo(DataStartPosition + offset);
            return read(_input);
        }

        // reads an array of values of a class type that can be null (value is T[])
        private T?[]? ReadArrayOfReference<T>(string name, FieldKind kind, Func<ObjectDataInput, T> read)
        {
            var field = GetValidField(name, kind);
            var offset = _offsetReader!(_input, _offsetPosition, field.Index);
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
                    items[i] = default;
                }
                else
                {
                    _input.MoveTo(arrayDataPosition + itemOffset);
                    items[i] = read(_input);
                }
            }

            return items;
        }

        #endregion

        private bool? ReadMaybeBoolean(string name)
        {
            var field = GetValidField(name);

            if (field.Kind == FieldKind.Boolean)
            {
                var (position, offset) = (DataStartPosition + field.Offset, field.BitOffset);

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

            if (field.Kind == FieldKind.NullableBoolean)
            {
                return ReadNullable(field, input => input.ReadBoolean());
            }

            throw new SerializationException($"Invalid kind \"{FieldKind.Boolean}\" or \"{FieldKind.NullableBoolean}\" for field \"{name}\" of schema {Schema}, which is of kind \"{field.Kind}\".");
        }

        public bool ReadBoolean(string name)
        {
            var value = ReadMaybeBoolean(name);
            if (value.HasValue) return value.Value;
            throw new SerializationException($"Null value for field \"{name}\" of schema {Schema}, which is of kind \"{FieldKind.NullableBoolean}\".");
        }

        public bool? ReadNullableBoolean(string name)
            => ReadMaybeBoolean(name);

        private T[]? ReadArrayOfBoolean<T>(SchemaField field, Func<bool, T> f)// where T : struct
        {
            var offset = _offsetReader!(_input, _offsetPosition, field.Index);
            if (offset < 0) return default;

            _input.MoveTo(DataStartPosition + offset);

            var length = _input.ReadInt();
            var value = new T[length];
            for (var i = 0; i < value.Length;)
            {
                var bits = _input.ReadByte();
                var mask = (byte)0b_1000_0000;
                for (var n = 7; i < value.Length && n >= 0; n--)
                {
                    value[i++] = f((bits & mask) > 0);
                    mask = (byte)(mask >> 1);
                }
            }
            return value;
        }

        public bool[]? ReadArrayOfBoolean(string name)
        {
            var field = GetValidField(name);

            switch (field.Kind)
            {
                case FieldKind.ArrayOfBoolean:
                    return ReadArrayOfBoolean(field, x => x);

                case FieldKind.ArrayOfNullableBoolean:
                    var values = ReadArrayOfNullable(field, input => input.ReadBoolean());
                    if (values == null) return null;
                    var array = new bool[values.Length];
                    for (var i = 0; i < values.Length; i++) array[i] = values[i] ?? throw new SerializationException($"Null value in field \"{name}\" of schema {Schema}, which is of kind \"{field.Kind}\".");
                    return array;

                default:
                    throw new SerializationException($"Invalid kind \"{FieldKind.ArrayOfBoolean}\" or \"{FieldKind.ArrayOfNullableBoolean}\" for field \"{name}\" of schema {Schema}, which is of kind \"{field.Kind}\".");
            }
        }

        public bool?[]? ReadArrayOfNullableBoolean(string name)
        {
            var field = GetValidField(name);

            switch (field.Kind)
            {
                case FieldKind.ArrayOfBoolean:
                    return ReadArrayOfBoolean<bool?>(field, x => x);

                case FieldKind.ArrayOfNullableBoolean:
                    return ReadArrayOfNullable(field, input => input.ReadBoolean());

                default:
                    throw new SerializationException($"Invalid kind \"{FieldKind.ArrayOfBoolean}\" or \"{FieldKind.ArrayOfNullableBoolean}\" for field \"{name}\" of schema {Schema}, which is of kind \"{field.Kind}\".");
            }
        }

        public sbyte ReadInt8(string name)
            => ReadMaybeNullableNotNull(name, FieldKind.Int8, FieldKind.NullableInt8, input => input.ReadSByte());

        public sbyte? ReadNullableInt8(string name)
            => ReadMaybeNullable(name, FieldKind.Int8, FieldKind.NullableInt8, input => input.ReadSByte());

        public sbyte[]? ReadArrayOfInt8(string name)
            => ReadArrayOfMaybeNullableNotNull(name,
                FieldKind.ArrayOfInt8, FieldKind.ArrayOfNullableInt8,
                input => input.ReadSByte(), input => input.ReadSByteArray());

        public sbyte?[]? ReadArrayOfNullableInt8(string name)
            => ReadArrayOfMaybeNullable(name,
                FieldKind.ArrayOfInt8, FieldKind.ArrayOfNullableInt8,
                input => input.ReadSByte(), input => input.ReadSByteArray());

        public short ReadInt16(string name)
            => ReadMaybeNullableNotNull(name, FieldKind.Int16, FieldKind.NullableInt16, input => input.ReadShort());

        public short? ReadNullableInt16(string name)
            => ReadMaybeNullable(name, FieldKind.Int16, FieldKind.NullableInt16, input => input.ReadShort());

        public short[]? ReadArrayOfInt16(string name)
            => ReadArrayOfMaybeNullableNotNull(name,
                FieldKind.ArrayOfInt16, FieldKind.ArrayOfNullableInt16,
                input => input.ReadShort(), input => input.ReadShortArray());

        public short?[]? ReadArrayOfNullableInt16(string name)
            => ReadArrayOfMaybeNullable(name,
                FieldKind.ArrayOfInt16, FieldKind.ArrayOfNullableInt16,
                input => input.ReadShort(), input => input.ReadShortArray());

        public int ReadInt32(string name)
            => ReadMaybeNullableNotNull(name, FieldKind.Int32, FieldKind.NullableInt32, input => input.ReadInt());

        public int? ReadNullableInt32(string name)
            => ReadMaybeNullable(name, FieldKind.Int32, FieldKind.NullableInt32, input => input.ReadInt());

        public int[]? ReadArrayOfInt32(string name)
            => ReadArrayOfMaybeNullableNotNull(name,
                FieldKind.ArrayOfInt32, FieldKind.ArrayOfNullableInt32, 
                input => input.ReadInt(), input => input.ReadIntArray());

        public int?[]? ReadArrayOfNullableInt32(string name)
            => ReadArrayOfMaybeNullable(name,
                FieldKind.ArrayOfInt32, FieldKind.ArrayOfNullableInt32,
                input => input.ReadInt(), input => input.ReadIntArray());

        public long ReadInt64(string name)
            => ReadMaybeNullableNotNull(name, FieldKind.Int64, FieldKind.NullableInt64, input => input.ReadLong());

        public long? ReadNullableInt64(string name)
            => ReadMaybeNullable(name, FieldKind.Int64, FieldKind.NullableInt64, input => input.ReadLong());

        public long[]? ReadArrayOfInt64(string name)
            => ReadArrayOfMaybeNullableNotNull(name,
                FieldKind.ArrayOfInt64, FieldKind.ArrayOfNullableInt64,
                input => input.ReadLong(), input => input.ReadLongArray());

        public long?[]? ReadArrayOfNullableInt64(string name)
            => ReadArrayOfMaybeNullable(name,
                FieldKind.ArrayOfInt64, FieldKind.ArrayOfNullableInt64,
                input => input.ReadLong(), input => input.ReadLongArray());

        public float ReadFloat32(string name)
            => ReadMaybeNullableNotNull(name, FieldKind.Float32, FieldKind.NullableFloat32, input => input.ReadFloat());

        public float? ReadNullableFloat32(string name)
            => ReadMaybeNullable(name, FieldKind.Float32, FieldKind.NullableFloat32, input => input.ReadFloat());

        public float[]? ReadArrayOfFloat32(string name)
            => ReadArrayOfMaybeNullableNotNull(name,
                FieldKind.ArrayOfFloat32, FieldKind.ArrayOfNullableFloat32,
                input => input.ReadFloat(), input => input.ReadFloatArray());

        public float?[]? ReadArrayOfNullableFloat32(string name)
            => ReadArrayOfMaybeNullable(name,
                FieldKind.ArrayOfFloat32, FieldKind.ArrayOfNullableFloat32,
                input => input.ReadFloat(), input => input.ReadFloatArray());

        public double ReadFloat64(string name)
            => ReadMaybeNullableNotNull(name, FieldKind.Float64, FieldKind.NullableFloat64, input => input.ReadDouble());

        public double? ReadNullableFloat64(string name)
            => ReadMaybeNullable(name, FieldKind.Float64, FieldKind.NullableFloat64, input => input.ReadDouble());

        public double[]? ReadArrayOfFloat64(string name)
            => ReadArrayOfMaybeNullableNotNull(name,
                FieldKind.ArrayOfFloat64, FieldKind.ArrayOfNullableFloat64,
                input => input.ReadDouble(), input => input.ReadDoubleArray());

        public double?[]? ReadArrayOfNullableFloat64(string name)
            => ReadArrayOfMaybeNullable(name,
                FieldKind.ArrayOfFloat64, FieldKind.ArrayOfNullableFloat64,
                input => input.ReadDouble(), input => input.ReadDoubleArray());

        public string? ReadString(string name)
            => ReadReference(name, FieldKind.String, input => input.ReadString());

        public string?[]? ReadArrayOfString(string name)
            => ReadArrayOfReference(name, FieldKind.ArrayOfString, input => input.ReadString());

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
        
        public HBigDecimal? ReadDecimal(string name)
            => ReadNullable(name, FieldKind.Decimal, ReadBigDecimal);

        public HBigDecimal?[]? ReadArrayOfDecimal(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfDecimal, ReadBigDecimal);

        private HLocalTime ReadTime(ObjectDataInput input)
        {
            // time is hour:i8 minute:i8 second:i8 nanosecond:i32
            var hour = _input.ReadByte();
            var minute = _input.ReadByte();
            var second = _input.ReadByte();
            var nanosecond = _input.ReadInt();
            return new HLocalTime(hour, minute, second, nanosecond);
        }
        
        public HLocalTime? ReadTime(string name)
            => ReadNullable(name, FieldKind.Time, ReadTime);

        public HLocalTime?[]? ReadArrayOfTime(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfTime, ReadTime);

        private HLocalDate ReadDate(ObjectDataInput input)
        {
            // date is year:i32 month:i8 day:i8
            var year = _input.ReadInt();
            var month = _input.ReadByte();
            var day = _input.ReadByte();
            return new HLocalDate(year, month, day);
        }

        public HLocalDate? ReadDate(string name)
            => ReadNullable(name, FieldKind.Date, ReadDate);

        public HLocalDate?[]? ReadArrayOfDate(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfDate, ReadDate);

        private HLocalDateTime ReadTimeStamp(ObjectDataInput input)
        {
            var date = ReadDate(input);
            var time = ReadTime(input);
            return new HLocalDateTime(date, time);
        }
        
        public HLocalDateTime? ReadTimeStamp(string name)
            => ReadNullable(name, FieldKind.TimeStamp, ReadTimeStamp);

        public HLocalDateTime?[]? ReadArrayOfTimeStamp(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfTimeStamp, ReadTimeStamp);

        private HOffsetDateTime ReadTimeStampWithTimeZone(ObjectDataInput input)
        {
            var timestamp = ReadTimeStamp(input);
            var offset = input.ReadInt();
            return new HOffsetDateTime(timestamp, offset);
        }
        
        public HOffsetDateTime? ReadTimeStampWithTimeZone(string name)
            => ReadNullable(name, FieldKind.TimeStampWithTimeZone, ReadTimeStampWithTimeZone);

        public HOffsetDateTime?[]? ReadArrayOfTimeStampWithTimeZone(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfTimeStampWithTimeZone, ReadTimeStampWithTimeZone);

        private T ReadCompact<T>(IObjectDataInput input)
        {
            return typeof(T).IsNullableOfT(out var underlyingType)
                ? (T)_objectsReader.Read(input, underlyingType)
                : _objectsReader.Read<T>(input);
        }

        public T? ReadCompact<T>(string name)
        {
            var field = GetValidField(name, FieldKind.Compact);
            var offset = _offsetReader!(_input, _offsetPosition, field.Index);
            if (offset < 0) return default;

            _input.MoveTo(DataStartPosition + offset);
            return ReadCompact<T>(_input);
        }

        public T?[]? ReadArrayOfCompact<T>(string name)
            => ReadArrayOfReference(name, FieldKind.ArrayOfCompact, ReadCompact<T>);
    }
}
