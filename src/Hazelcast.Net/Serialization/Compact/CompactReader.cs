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

        public CompactReader(CompactSerializer serializer, ObjectDataInput input, Schema schema, bool withSchema)
            : base(serializer, schema, input.Position, withSchema)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));

            if (schema.HasReferenceFields)
            {
                _dataLength = input.ReadInt();
                _offsetPosition = DataStartPosition + _dataLength;
            }

            _offsetReader = GetOffsetReader(_dataLength);
        }

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

        public bool[]? ReadBooleans(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadBooleans
        }

        public bool?[]? ReadBooleanRefs(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadBooleanRefs
        }

        public sbyte ReadSByte(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.SignedInteger8);
            _input.MoveTo(position);
            return _input.ReadSByte();
        }

        public sbyte? ReadSByteRef(string name)
            => ReadNullable(name, FieldKind.SignedInteger8, input => input.ReadSByte());

        public sbyte[]? ReadSBytes(string name)
            => ReadReference(name, FieldKind.ArrayOfSignedInteger8, input => input.ReadSByteArray());

        public sbyte?[]? ReadSByteRefs(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfSignedInteger8Ref, input => input.ReadSByte());

        public short ReadShort(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.SignedInteger16);
            _input.MoveTo(position);
            return _input.ReadShort();
        }

        public short? ReadShortRef(string name)
            => ReadNullable(name, FieldKind.SignedInteger16, input => input.ReadShort());

        public short[]? ReadShorts(string name)
            => ReadReference(name, FieldKind.ArrayOfSignedInteger16, input => input.ReadShortArray());

        public short?[]? ReadShortRefs(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfSignedInteger16Ref, input => input.ReadShort());
        
        public int ReadInt(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.SignedInteger32);
            _input.MoveTo(position);
            return _input.ReadInt();
        }

        public int? ReadIntRef(string name)
            => ReadNullable(name, FieldKind.SignedInteger32, input => input.ReadInt());

        public int[]? ReadInts(string name)
            => ReadReference(name, FieldKind.ArrayOfSignedInteger32, input => input.ReadIntArray());

        public int?[]? ReadIntRefs(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfSignedInteger32Ref, input => input.ReadInt());

        public long ReadLong(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.SignedInteger64);
            _input.MoveTo(position);
            return _input.ReadLong();
        }

        public long? ReadLongRef(string name)
            => ReadNullable(name, FieldKind.SignedInteger64, input => input.ReadLong());

        public long[]? ReadLongs(string name)
            => ReadReference(name, FieldKind.ArrayOfSignedInteger64, input => input.ReadLongArray());

        public long?[]? ReadLongRefs(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfSignedInteger64Ref, input => input.ReadLong());

        public float ReadFloat(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Float);
            _input.MoveTo(position);
            return _input.ReadFloat();
        }

        public float? ReadFloatRef(string name)
            => ReadNullable(name, FieldKind.Float, input => input.ReadFloat());

        public float[]? ReadFloats(string name)
            => ReadReference(name, FieldKind.Float, input => input.ReadFloatArray());

        public float?[]? ReadFloatRefs(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfFloatRef, input => input.ReadFloat());

        public double ReadDouble(string name)
        {
            var position = GetValueFieldPosition(name, FieldKind.Double);
            _input.MoveTo(position);
            return _input.ReadDouble();
        }

        public double? ReadDoubleRef(string name)
            => ReadNullable(name, FieldKind.Double, input => input.ReadDouble());

        public double[]? ReadDoubles(string name)
            => ReadReference(name, FieldKind.ArrayOfDouble, input => input.ReadDoubleArray());

        public double?[]? ReadDoubleRefs(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfDoubleRef, input => input.ReadDouble());

        public string? ReadStringRef(string name)
            => ReadReference(name, FieldKind.StringRef, input => input.ReadString());

        public string?[]? ReadStringRefs(string name)
            => ReadArrayOfReference(name, FieldKind.ArrayOfStringRef, input => input.ReadString());

        private static decimal ReadBigDecimalIntoDecimal(ObjectDataInput input)
        {
            var bigValue = input.ReadBigDecimal();
            if (bigValue.TryToDecimal(out var value)) return value;
            throw new OverflowException("Cannot read BigDecimal value into decimal.");
        }

        public decimal? ReadDecimalRef(string name)
            => ReadNullable(name, FieldKind.DecimalRef, ReadBigDecimalIntoDecimal);

        public decimal?[]? ReadDecimalRefs(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfDecimalRef, ReadBigDecimalIntoDecimal);

        public HBigDecimal? ReadBigDecimalRef(string name)
            => ReadNullable(name, FieldKind.DecimalRef, input => input.ReadBigDecimal());

        public HBigDecimal?[]? ReadBigDecimalRefs(string name)
            => ReadArrayOfNullable(name, FieldKind.ArrayOfDecimalRef, input => input.ReadBigDecimal());

        public TimeSpan? ReadTimeRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeRef
        }

        public TimeSpan?[]? ReadTimeRefs(string name)
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

        public DateTime?[]? ReadDateRefs(string name)
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

        public DateTime?[]? ReadTimeStampRefs(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampRefs
        }

        public DateTimeOffset? ReadTimeStampWithTimeZoneRef(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampWithTimeZoneRef
        }

        public DateTimeOffset?[]? ReadTimeStampWithTimeZoneRefs(string name)
        {
            throw new NotImplementedException(); // FIXME - implement ReadTimeStampWithTimeZoneRefs
        }

        public T? ReadObjectRef<T>(string name)
            where T: class
            => ReadReference(name, FieldKind.ObjectRef, input => Serializer.Read<T>(input, WithSchema));

        public T?[]? ReadObjectRefs<T>(string name)
            where T : class
            => ReadArrayOfReference(name, FieldKind.ArrayOfObjectRef, input => Serializer.Read<T>(input, WithSchema));
    }
}
