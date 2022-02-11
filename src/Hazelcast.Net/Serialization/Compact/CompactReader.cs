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

        public bool? ReadBooleanRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadBooleanRef
        }

        public bool[]? ReadArrayOfBoolean(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadBooleans
        }

        public bool?[]? ReadArrayOfBooleanRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadBooleanRefs
        }

        public sbyte ReadInt8(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int8);
            _input.MoveTo(position);
            return _input.ReadSByte();
        }

        public sbyte? ReadInt8Ref(string name)
            => ReadNullable(name, FieldKind.Int8, input => input.ReadSByte());

        public sbyte[]? ReadArrayOfInt8(string name)
            => ReadReference(name, FieldKind.ArrayOfInt8, input => input.ReadSByteArray());

        public sbyte?[]? ReadArrayOfInt8Ref(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfInt8Ref, input => input.ReadSByte());

        public short ReadInt16(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int16);
            _input.MoveTo(position);
            return _input.ReadShort();
        }

        public short? ReadInt16Ref(string name)
            => ReadNullable(name, FieldKind.Int16, input => input.ReadShort());

        public short[]? ReadArrayOfInt16(string name)
            => ReadReference(name, FieldKind.ArrayOfInt16, input => input.ReadShortArray());

        public short?[]? ReadArrayOfInt16Ref(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfInt16Ref, input => input.ReadShort());
        
        public int ReadInt32(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int32);
            _input.MoveTo(position);
            return _input.ReadInt();
        }

        public int? ReadInt32Ref(string name)
            => ReadNullable(name, FieldKind.Int32, input => input.ReadInt());

        public int[]? ReadArrayOfInt32(string name)
            => ReadReference(name, FieldKind.ArrayOfInt32, input => input.ReadIntArray());

        public int?[]? ReadArrayOfInt32Ref(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfInt32Ref, input => input.ReadInt());

        public long ReadInt64(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Int64);
            _input.MoveTo(position);
            return _input.ReadLong();
        }

        public long? ReadInt64Ref(string name)
            => ReadNullable(name, FieldKind.Int64, input => input.ReadLong());

        public long[]? ReadArrayOfInt64(string name)
            => ReadReference(name, FieldKind.ArrayOfInt64, input => input.ReadLongArray());

        public long?[]? ReadArrayOfInt64Ref(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfInt64Ref, input => input.ReadLong());

        public float ReadFloat32(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Float32);
            _input.MoveTo(position);
            return _input.ReadFloat();
        }

        public float? ReadFloat32Ref(string name)
            => ReadNullable(name, FieldKind.Float32, input => input.ReadFloat());

        public float[]? ReadArrayOfFloat32(string name)
            => ReadReference(name, FieldKind.Float32, input => input.ReadFloatArray());

        public float?[]? ReadArrayOfFloat32Ref(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfFloat32Ref, input => input.ReadFloat());

        public double ReadFloat64(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Float64);
            _input.MoveTo(position);
            return _input.ReadDouble();
        }

        public double? ReadFloat64Ref(string name)
            => ReadNullable(name, FieldKind.Float64, input => input.ReadDouble());

        public double[]? ReadArrayOfFloat64(string name)
            => ReadReference(name, FieldKind.ArrayOfFloat64, input => input.ReadDoubleArray());

        public double?[]? ReadArrayOfFloat64Ref(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfFloat64Ref, input => input.ReadDouble());

        public string? ReadStringRef(string name)
            => ReadReference(name, FieldKind.StringRef, input => input.ReadString());

        public string?[]? ReadArrayOfStringRef(string name)
            => ReadArrayOfReference(name, FieldKind.ArrayOfStringRef, input => input.ReadString());

        private static decimal ReadBigDecimalIntoDecimal(ObjectDataInput input)
        {
            var bigValue = input.ReadBigDecimal();
            if (bigValue.TryToDecimal(out var value)) return value;
            throw new OverflowException("Cannot read BigDecimal value into decimal.");
        }

        public decimal? ReadDecimalRef(string name)
            => ReadNullable(name, FieldKind.DecimalRef, ReadBigDecimalIntoDecimal);

        public decimal?[]? ReadArrayOfDecimalRef(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfDecimalRef, ReadBigDecimalIntoDecimal);

        public HBigDecimal? ReadBigDecimalRef(string name)
            => ReadNullable(name, FieldKind.DecimalRef, input => input.ReadBigDecimal());

        public HBigDecimal?[]? ReadArrayOfBigDecimalRef(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfDecimalRef, input => input.ReadBigDecimal());

        public TimeSpan? ReadTimeRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeRef
        }

        public TimeSpan?[]? ReadArrayOfTimeRef(string name)
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

        public DateTime? ReadDateRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadDateRef
        }

        public DateTime?[]? ReadArrayOfDateRef(string name)
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

        public DateTime? ReadTimeStampRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampRef
        }

        public DateTime?[]? ReadArrayOfTimeStampRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampRefs
        }

        public DateTimeOffset? ReadTimeStampWithTimeZoneRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampWithTimeZoneRef
        }

        public DateTimeOffset?[]? ReadArrayOfTimeStampWithTimeZoneRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampWithTimeZoneRefs
        }

        public T? ReadCompactRef<T>(string name)
            where T: class
            => ReadReference(name, FieldKind.CompactRef, input => Serializer.Read<T>(input));

        public T?[]? ReadArrayOfCompactObjectRef<T>(string name)
            where T : class
            => ReadArrayOfReference(name, FieldKind.ArrayOfCompactRef, input => Serializer.Read<T>(input));
    }
}
