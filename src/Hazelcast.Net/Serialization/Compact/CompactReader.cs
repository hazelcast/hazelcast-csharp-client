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
    internal class CompactReader : CompactReaderWriterBase, ICompactReader
    {
        private readonly ObjectDataInput _input;
        private readonly int _dataLength;
        private readonly int _offsetPosition;
        private readonly Func<ObjectDataInput, int, int, int> _offsetReader;

        public CompactReader(CompactSerializer serializer, ObjectDataInput input, Schema schema, Type objectType)
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
            throw new NotImplementedException(); // FIXME - implement ReadBoolean
        }

        public bool? ReadNullableBoolean(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadBooleanRef
        }

        public bool[]? ReadArrayOfBoolean(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadBooleans
        }

        public bool?[]? ReadArrayOfNullableBoolean(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadBooleanRefs
        }

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

        private static decimal ReadBigDecimalIntoDecimal(ObjectDataInput input)
        {
            var bigValue = input.ReadBigDecimal();
            if (bigValue.TryToDecimal(out var value)) return value;
            throw new OverflowException("Cannot read BigDecimal value into decimal.");
        }

        public decimal? ReadNullableDecimal(string name)
            => ReadNullable(name, FieldKind.NullableDecimal, ReadBigDecimalIntoDecimal);

        public decimal?[]? ReadArrayOfNullableDecimal(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableDecimal, ReadBigDecimalIntoDecimal);

        public HBigDecimal? ReadNullableBigDecimal(string name)
            => ReadNullable(name, FieldKind.NullableDecimal, input => input.ReadBigDecimal());

        public HBigDecimal?[]? ReadArrayOfNullableBigDecimal(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfNullableDecimal, input => input.ReadBigDecimal());

        public TimeSpan? ReadNullableTime(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeRef
        }

        public TimeSpan?[]? ReadArrayOfNullableTime(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeRefs
        }

#if NET6_0_OR_GREATER
        public TimeOnly? ReadTimeOnlyRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeOnlyRef
        }

        public TimeTimeOnlySpan?[]? ReadTimeOnlyRefs(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeOnlyRefs
        }
#endif

        public DateTime? ReadNullableDate(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadDateRef
        }

        public DateTime?[]? ReadArrayOfNullableDate(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadDateRef
        }

#if NET6_0_OR_GREATER
        public DateOnly? ReadDateOnlyRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadDateOnlyRef
        }

        public DateOnly?[]? ReadDateOnlyRefs(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadDateOnlyRef
        }
#endif

        public DateTime? ReadNullableTimeStamp(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampRef
        }

        public DateTime?[]? ReadArrayOfNullableTimeStamp(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampRefs
        }

        public DateTimeOffset? ReadNullableTimeStampWithTimeZone(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampWithTimeZoneRef
        }

        public DateTimeOffset?[]? ReadArrayOfNullableTimeStampWithTimeZone(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampWithTimeZoneRefs
        }

        public T? ReadNullableCompact<T>(string name)
            where T: class
            => ReadReference(name, FieldKind.NullableCompact, input => Serializer.Read<T>(input));

        public T?[]? ReadArrayOfCompactNullableObject<T>(string name)
            where T : class
            => ReadArrayOfReference(name, FieldKind.ArrayOfNullableCompact, input => Serializer.Read<T>(input));
    }
}
