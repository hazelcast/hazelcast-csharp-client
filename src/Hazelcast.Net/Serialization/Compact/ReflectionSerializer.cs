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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;

#nullable enable

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Implements a reflection-based compact serializer.
    /// </summary>
    internal class ReflectionSerializer : ICompactSerializer<object>
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();

        /// <inheritdoc />
        public string TypeName => throw new NotSupportedException();

        // internal for tests
        internal static T UnboxNonNull<T>(object? value)
        {
            if (value != null) return (T)value;
            throw new InvalidOperationException($"Cannot unbox null value as {typeof (T)}.");
        }

        // internal for tests
        internal static T ValueNonNull<T>(T? value)
            where T : struct
        {
            if (value.HasValue) return value.Value;
            throw new InvalidOperationException($"Cannot return null value as {typeof (T)}.");
        }

        private static TArray?[]? ToArray<TArray>(object? o, Func<object?, TArray?> convert) where TArray : struct
        {
            if (o is not Array { Rank: 1 } source) return null;
            var array = new TArray?[source.Length];
            var i = 0;
            foreach (var element in source) array[i++] = convert(element);
            return array;
        }

        private static TArray?[]? ToArray<TArray>(object? o, Func<object?, TArray> convert) where TArray : struct
        {
            if (o is not Array { Rank: 1 } source) return null;
            var array = new TArray?[source.Length];
            var i = 0;
            foreach (var element in source) array[i++] = convert(element);
            return array;
        }

        private static TArray[]? ToArray<TSource, TArray>(TSource[]? source, Func<TSource, TArray> convert) where TSource : struct where TArray : struct
        {
            if (source == null) return null;
            var array = new TArray[source.Length];
            for (var i = 0; i < source.Length; i++) array[i] = convert(source[i]);
            return array;
        }

        private static TArray[]? ToArray<TSource, TArray>(TSource?[]? source, Func<TSource?, TArray> convert) where TSource : struct where TArray : struct
        {
            if (source == null) return null;
            var array = new TArray[source.Length];
            for (var i = 0; i < source.Length; i++) array[i] = convert(source[i]);
            return array;
        }

        private static TArray?[]? ToArrayOfNullable<TSource, TArray>(TSource?[]? source, Func<TSource?, TArray?> convert) where TSource : struct where TArray : struct
        {
            if (source == null) return null;
            var array = new TArray?[source.Length];
            for (var i = 0; i < source.Length; i++) array[i] = convert(source[i]);
            return array;
        }

        // for writing, cannot cast an object to e.g. an HBigDecimal because the explicit conversion cannot
        // be resolved at compile time. and, this is clearer (at IL level) than (HBigDecimal?)(decimal?)o

        private static short CharToShort(object? o) => (short)(ushort)UnboxNonNull<char>(o);
        private static short? NullableCharToNullableShort(object? o) => o is char value ? (short)(ushort)value : null;

        private static short[]? CharsToShorts(object? o)
        {
            if (o == null) return null;
            if (o is not char[] x) throw new InvalidCastException();
            var shorts = new short[x.Length];
            for (var i = 0; i < x.Length; i++) shorts[i] = (short)(ushort)x[i];
            return shorts;
        }

        private static short?[]? NullableCharsToNullableShorts(object? o)
        {
            if (o == null) return null;
            if (o is not char?[] x) throw new InvalidCastException();
            var shorts = new short?[x.Length];
            for (var i = 0; i < x.Length; i++)
            {
                var c = x[i];
                shorts[i] = c is null ? null : (short)(ushort)c;
            }
            return shorts;
        }

        private static HBigDecimal? DecimalToBigDecimal(object? o) => o is decimal value ? new HBigDecimal(value) : null;
        private static HLocalTime? TimeSpanToTime(object? o) => o is TimeSpan value ? new HLocalTime(value) : null;
        private static HLocalDateTime? DateTimeToTimeStamp(object? o) => o is DateTime value ? new HLocalDateTime(value) : null;
        private static HOffsetDateTime? DateTimeOffsetToTimeStampWithTimeZone(object? o) => o is DateTimeOffset value ? new HOffsetDateTime(value) : null;
#if NET6_0_OR_GREATER
        private static HLocalTime? TimeOnlyToTime(object? o) => o is TimeOnly value ? new HLocalTime(value) : null;
        private static HLocalDate? DateOnlyToDate(object? o) => o is DateOnly value ? new HLocalDate(value) : null;
#endif

        private static readonly Dictionary<Type, Action<ICompactWriter, string, object?>> Writers
            = new()
            {
                // there is no typeof nullable reference type (e.g. string?) since they are not
                // actual CLR types, so we have to register writers here against the actual types
                // (e.g. string) even though the value we write may be null.

                // first, register some non-generated writers for convenient .NET types
                // TODO: consider adding more types, supporting lists...?

                { typeof (HBigDecimal), (w, n, o) => w.WriteDecimal(n, UnboxNonNull<HBigDecimal>(o)) },
                { typeof (HBigDecimal[]), (w, n, o) => w.WriteArrayOfDecimal(n, ToArray(o, UnboxNonNull<HBigDecimal>)) },
                { typeof (HLocalTime), (w, n, o) => w.WriteTime(n, UnboxNonNull<HLocalTime>(o)) },
                { typeof (HLocalTime[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, UnboxNonNull<HLocalTime>)) },
                { typeof (HLocalDate), (w, n, o) => w.WriteDate(n, UnboxNonNull<HLocalDate>(o)) },
                { typeof (HLocalDate[]), (w, n, o) => w.WriteArrayOfDate(n, ToArray(o, UnboxNonNull<HLocalDate>)) },
                { typeof (HLocalDateTime), (w, n, o) => w.WriteTimeStamp(n, UnboxNonNull<HLocalDateTime>(o)) },
                { typeof (HLocalDateTime[]), (w, n, o) => w.WriteArrayOfTimeStamp(n, ToArray(o, UnboxNonNull<HLocalDateTime>)) },
                { typeof (HOffsetDateTime), (w, n, o) => w.WriteTimeStampWithTimeZone(n, UnboxNonNull<HOffsetDateTime>(o)) },
                { typeof (HOffsetDateTime[]), (w, n, o) => w.WriteArrayOfTimeStampWithTimeZone(n, ToArray(o, UnboxNonNull<HOffsetDateTime>)) },

                { typeof (decimal), (w, n, o) => w.WriteDecimal(n, DecimalToBigDecimal(o)) },
                { typeof (decimal?), (w, n, o) => w.WriteDecimal(n, DecimalToBigDecimal(o)) },
                { typeof (decimal[]), (w, n, o) => w.WriteArrayOfDecimal(n, ToArray(o, DecimalToBigDecimal)) },
                { typeof (decimal?[]), (w, n, o) => w.WriteArrayOfDecimal(n, ToArray(o, DecimalToBigDecimal)) },

                { typeof (TimeSpan), (w, n, o) => w.WriteTime(n, TimeSpanToTime(o)) },
                { typeof (TimeSpan?), (w, n, o) => w.WriteTime(n, TimeSpanToTime(o)) },
                { typeof (TimeSpan[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, TimeSpanToTime)) },
                { typeof (TimeSpan?[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, TimeSpanToTime)) },

                { typeof (DateTime), (w, n, o) => w.WriteTimeStamp(n, DateTimeToTimeStamp(o)) },
                { typeof (DateTime?), (w, n, o) => w.WriteTimeStamp(n, DateTimeToTimeStamp(o)) },
                { typeof (DateTime[]), (w, n, o) => w.WriteArrayOfTimeStamp(n, ToArray(o, DateTimeToTimeStamp)) },
                { typeof (DateTime?[]), (w, n, o) => w.WriteArrayOfTimeStamp(n, ToArray(o, DateTimeToTimeStamp)) },

                { typeof (DateTimeOffset), (w, n, o) => w.WriteTimeStampWithTimeZone(n, DateTimeOffsetToTimeStampWithTimeZone(o)) },
                { typeof (DateTimeOffset?), (w, n, o) => w.WriteTimeStampWithTimeZone(n, DateTimeOffsetToTimeStampWithTimeZone(o)) },
                { typeof (DateTimeOffset[]), (w, n, o) => w.WriteArrayOfTimeStampWithTimeZone(n, ToArray(o, DateTimeOffsetToTimeStampWithTimeZone)) },
                { typeof (DateTimeOffset?[]), (w, n, o) => w.WriteArrayOfTimeStampWithTimeZone(n, ToArray(o, DateTimeOffsetToTimeStampWithTimeZone)) },

#if NET6_0_OR_GREATER
                { typeof (TimeOnly), (w, n, o) => w.WriteTime(n, TimeOnlyToTime(o)) },
                { typeof (TimeOnly?), (w, n, o) => w.WriteTime(n, TimeOnlyToTime(o)) },
                { typeof (TimeOnly[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, TimeOnlyToTime)) },
                { typeof (TimeOnly?[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, TimeOnlyToTime)) },

                { typeof (DateOnly), (w, n, o) => w.WriteDate(n, DateOnlyToDate(o)) },
                { typeof (DateOnly?), (w, n, o) => w.WriteDate(n, DateOnlyToDate(o)) },
                { typeof (DateOnly[]), (w, n, o) => w.WriteArrayOfDate(n, ToArray(o, DateOnlyToDate)) },
                { typeof (DateOnly?[]), (w, n, o) => w.WriteArrayOfDate(n, ToArray(o, DateOnlyToDate)) },
#endif

                // do NOT remove nor alter the <generated></generated> lines!
                // <generated>

                { typeof (bool), (w, n, o) => w.WriteBoolean(n, UnboxNonNull<bool>(o)) },
                { typeof (sbyte), (w, n, o) => w.WriteInt8(n, UnboxNonNull<sbyte>(o)) },
                { typeof (short), (w, n, o) => w.WriteInt16(n, UnboxNonNull<short>(o)) },
                { typeof (int), (w, n, o) => w.WriteInt32(n, UnboxNonNull<int>(o)) },
                { typeof (long), (w, n, o) => w.WriteInt64(n, UnboxNonNull<long>(o)) },
                { typeof (float), (w, n, o) => w.WriteFloat32(n, UnboxNonNull<float>(o)) },
                { typeof (double), (w, n, o) => w.WriteFloat64(n, UnboxNonNull<double>(o)) },
                { typeof (bool[]), (w, n, o) => w.WriteArrayOfBoolean(n, (bool[]?)o) },
                { typeof (sbyte[]), (w, n, o) => w.WriteArrayOfInt8(n, (sbyte[]?)o) },
                { typeof (short[]), (w, n, o) => w.WriteArrayOfInt16(n, (short[]?)o) },
                { typeof (int[]), (w, n, o) => w.WriteArrayOfInt32(n, (int[]?)o) },
                { typeof (long[]), (w, n, o) => w.WriteArrayOfInt64(n, (long[]?)o) },
                { typeof (float[]), (w, n, o) => w.WriteArrayOfFloat32(n, (float[]?)o) },
                { typeof (double[]), (w, n, o) => w.WriteArrayOfFloat64(n, (double[]?)o) },
                { typeof (bool?), (w, n, o) => w.WriteNullableBoolean(n, (bool?)o) },
                { typeof (sbyte?), (w, n, o) => w.WriteNullableInt8(n, (sbyte?)o) },
                { typeof (short?), (w, n, o) => w.WriteNullableInt16(n, (short?)o) },
                { typeof (int?), (w, n, o) => w.WriteNullableInt32(n, (int?)o) },
                { typeof (long?), (w, n, o) => w.WriteNullableInt64(n, (long?)o) },
                { typeof (float?), (w, n, o) => w.WriteNullableFloat32(n, (float?)o) },
                { typeof (double?), (w, n, o) => w.WriteNullableFloat64(n, (double?)o) },
                { typeof (HBigDecimal?), (w, n, o) => w.WriteDecimal(n, (HBigDecimal?)o) },
                { typeof (string), (w, n, o) => w.WriteString(n, (string?)o) },
                { typeof (HLocalTime?), (w, n, o) => w.WriteTime(n, (HLocalTime?)o) },
                { typeof (HLocalDate?), (w, n, o) => w.WriteDate(n, (HLocalDate?)o) },
                { typeof (HLocalDateTime?), (w, n, o) => w.WriteTimeStamp(n, (HLocalDateTime?)o) },
                { typeof (HOffsetDateTime?), (w, n, o) => w.WriteTimeStampWithTimeZone(n, (HOffsetDateTime?)o) },
                { typeof (bool?[]), (w, n, o) => w.WriteArrayOfNullableBoolean(n, (bool?[]?)o) },
                { typeof (sbyte?[]), (w, n, o) => w.WriteArrayOfNullableInt8(n, (sbyte?[]?)o) },
                { typeof (short?[]), (w, n, o) => w.WriteArrayOfNullableInt16(n, (short?[]?)o) },
                { typeof (int?[]), (w, n, o) => w.WriteArrayOfNullableInt32(n, (int?[]?)o) },
                { typeof (long?[]), (w, n, o) => w.WriteArrayOfNullableInt64(n, (long?[]?)o) },
                { typeof (float?[]), (w, n, o) => w.WriteArrayOfNullableFloat32(n, (float?[]?)o) },
                { typeof (double?[]), (w, n, o) => w.WriteArrayOfNullableFloat64(n, (double?[]?)o) },
                { typeof (HBigDecimal?[]), (w, n, o) => w.WriteArrayOfDecimal(n, (HBigDecimal?[]?)o) },
                { typeof (HLocalTime?[]), (w, n, o) => w.WriteArrayOfTime(n, (HLocalTime?[]?)o) },
                { typeof (HLocalDate?[]), (w, n, o) => w.WriteArrayOfDate(n, (HLocalDate?[]?)o) },
                { typeof (HLocalDateTime?[]), (w, n, o) => w.WriteArrayOfTimeStamp(n, (HLocalDateTime?[]?)o) },
                { typeof (HOffsetDateTime?[]), (w, n, o) => w.WriteArrayOfTimeStampWithTimeZone(n, (HOffsetDateTime?[]?)o) },
                { typeof (string[]), (w, n, o) => w.WriteArrayOfString(n, (string?[]?)o) },

                // </generated>

                { typeof (char), (w, n, o) => w.WriteInt16(n, CharToShort(o)) },
                { typeof (char?), (w, n, o) => w.WriteNullableInt16(n, NullableCharToNullableShort(o)) },
                { typeof (char[]), (w, n, o) => w.WriteArrayOfInt16(n, CharsToShorts(o)) },
                { typeof (char?[]), (w, n, o) => w.WriteArrayOfNullableInt16(n, NullableCharsToNullableShorts(o)) },
            };

        private static readonly Dictionary<Type, Func<ICompactReader, string, object?>> Readers
            = new()
            {
// ReSharper disable RedundantCast
#pragma warning disable IDE0004

                // some casts are redundant, but let's force ourselves to cast everywhere,
                // so that we are 100% we detect potential type mismatch errors

                // there is no typeof nullable reference type (e.g. string?) since they are not
                // actual CLR types, so we have to register readers here against the actual types
                // (e.g. string) even though the value we read may be null.

                // first, register some non-generated readers for convenient .NET types
                // TODO: consider adding more types, supporting lists...?

                { typeof (HBigDecimal), (r, n) => (HBigDecimal)ValueNonNull(r.ReadDecimal(n)) },
                { typeof (HBigDecimal[]), (r, n) => ToArray(r.ReadArrayOfDecimal(n), x => (HBigDecimal)ValueNonNull(x)) },
                { typeof (HLocalTime), (r, n) => (HLocalTime)ValueNonNull(r.ReadTime(n)) },
                { typeof (HLocalTime[]), (r, n) => ToArray(r.ReadArrayOfTime(n), x => (HLocalTime)ValueNonNull(x)) },
                { typeof (HLocalDate), (r, n) => (HLocalDate)ValueNonNull(r.ReadDate(n)) },
                { typeof (HLocalDate[]), (r, n) => ToArray(r.ReadArrayOfDate(n), x => (HLocalDate)ValueNonNull(x)) },
                { typeof (HLocalDateTime), (r, n) => (HLocalDateTime)ValueNonNull(r.ReadTimeStamp(n)) },
                { typeof (HLocalDateTime[]), (r, n) => ToArray(r.ReadArrayOfTimeStamp(n), x => (HLocalDateTime)ValueNonNull(x)) },
                { typeof (HOffsetDateTime), (r, n) => (HOffsetDateTime)ValueNonNull(r.ReadTimeStampWithTimeZone(n)) },
                { typeof (HOffsetDateTime[]), (r, n) => ToArray(r.ReadArrayOfTimeStampWithTimeZone(n), x => (HOffsetDateTime)ValueNonNull(x)) },

                { typeof (decimal), (r, n) => (decimal)ValueNonNull(r.ReadDecimal(n)) },
                { typeof (decimal?), (r, n) => (decimal?)r.ReadDecimal(n) },
                { typeof (decimal[]), (r, n) => ToArray(r.ReadArrayOfDecimal(n), x => (decimal)ValueNonNull(x)) },
                { typeof (decimal?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfDecimal(n), x => (decimal?)x) },

                { typeof (TimeSpan), (r, n) => (TimeSpan)ValueNonNull(r.ReadTime(n)) },
                { typeof (TimeSpan?), (r, n) => (TimeSpan?)r.ReadTime(n) },
                { typeof (TimeSpan[]), (r, n) => ToArray(r.ReadArrayOfTime(n), x => (TimeSpan)ValueNonNull(x)) },
                { typeof (TimeSpan?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfTime(n), x => (TimeSpan?)x) },

                { typeof (DateTime), (r, n) => (DateTime)ValueNonNull(r.ReadTimeStamp(n)) },
                { typeof (DateTime?), (r, n) => (DateTime?)r.ReadTimeStamp(n) },
                { typeof (DateTime[]), (r, n) => ToArray(r.ReadArrayOfTimeStamp(n), x => (DateTime)ValueNonNull(x)) },
                { typeof (DateTime?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfTimeStamp(n), x => (DateTime?)x) },

                { typeof (DateTimeOffset), (r, n) => (DateTimeOffset)ValueNonNull(r.ReadTimeStampWithTimeZone(n)) },
                { typeof (DateTimeOffset?), (r, n) => (DateTimeOffset?)r.ReadTimeStampWithTimeZone(n) },
                { typeof (DateTimeOffset[]), (r, n) => ToArray(r.ReadArrayOfTimeStampWithTimeZone(n), x => (DateTimeOffset)ValueNonNull(x)) },
                { typeof (DateTimeOffset?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfTimeStampWithTimeZone(n), x => (DateTimeOffset?)x) },

#if NET6_0_OR_GREATER
                { typeof (TimeOnly), (r, n) => (TimeOnly)ValueNonNull(r.ReadTime(n)) },
                { typeof (TimeOnly?), (r, n) => (TimeOnly?)r.ReadTime(n) },
                { typeof (TimeOnly[]), (r, n) => ToArray(r.ReadArrayOfTime(n), x => (TimeOnly)ValueNonNull(x)) },
                { typeof (TimeOnly?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfTime(n), x => (TimeOnly?)x) },

                { typeof (DateOnly), (r, n) => (DateOnly)ValueNonNull(r.ReadDate(n)) },
                { typeof (DateOnly?), (r, n) => (DateOnly?)r.ReadDate(n) },
                { typeof (DateOnly[]), (r, n) => ToArray(r.ReadArrayOfDate(n), x => (DateOnly)ValueNonNull(x)) },
                { typeof (DateOnly?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfDate(n), x => (DateOnly?)x) },
#endif

                // do NOT remove nor alter the <generated></generated> lines!
                // <generated>

                { typeof (bool), (r, n) => (bool)r.ReadBoolean(n) },
                { typeof (sbyte), (r, n) => (sbyte)r.ReadInt8(n) },
                { typeof (short), (r, n) => (short)r.ReadInt16(n) },
                { typeof (int), (r, n) => (int)r.ReadInt32(n) },
                { typeof (long), (r, n) => (long)r.ReadInt64(n) },
                { typeof (float), (r, n) => (float)r.ReadFloat32(n) },
                { typeof (double), (r, n) => (double)r.ReadFloat64(n) },
                { typeof (bool[]), (r, n) => (bool[]?)r.ReadArrayOfBoolean(n) },
                { typeof (sbyte[]), (r, n) => (sbyte[]?)r.ReadArrayOfInt8(n) },
                { typeof (short[]), (r, n) => (short[]?)r.ReadArrayOfInt16(n) },
                { typeof (int[]), (r, n) => (int[]?)r.ReadArrayOfInt32(n) },
                { typeof (long[]), (r, n) => (long[]?)r.ReadArrayOfInt64(n) },
                { typeof (float[]), (r, n) => (float[]?)r.ReadArrayOfFloat32(n) },
                { typeof (double[]), (r, n) => (double[]?)r.ReadArrayOfFloat64(n) },
                { typeof (bool?), (r, n) => (bool?)r.ReadNullableBoolean(n) },
                { typeof (sbyte?), (r, n) => (sbyte?)r.ReadNullableInt8(n) },
                { typeof (short?), (r, n) => (short?)r.ReadNullableInt16(n) },
                { typeof (int?), (r, n) => (int?)r.ReadNullableInt32(n) },
                { typeof (long?), (r, n) => (long?)r.ReadNullableInt64(n) },
                { typeof (float?), (r, n) => (float?)r.ReadNullableFloat32(n) },
                { typeof (double?), (r, n) => (double?)r.ReadNullableFloat64(n) },
                { typeof (HBigDecimal?), (r, n) => (HBigDecimal?)r.ReadDecimal(n) },
                { typeof (string), (r, n) => (string?)r.ReadString(n) },
                { typeof (HLocalTime?), (r, n) => (HLocalTime?)r.ReadTime(n) },
                { typeof (HLocalDate?), (r, n) => (HLocalDate?)r.ReadDate(n) },
                { typeof (HLocalDateTime?), (r, n) => (HLocalDateTime?)r.ReadTimeStamp(n) },
                { typeof (HOffsetDateTime?), (r, n) => (HOffsetDateTime?)r.ReadTimeStampWithTimeZone(n) },
                { typeof (bool?[]), (r, n) => (bool?[]?)r.ReadArrayOfNullableBoolean(n) },
                { typeof (sbyte?[]), (r, n) => (sbyte?[]?)r.ReadArrayOfNullableInt8(n) },
                { typeof (short?[]), (r, n) => (short?[]?)r.ReadArrayOfNullableInt16(n) },
                { typeof (int?[]), (r, n) => (int?[]?)r.ReadArrayOfNullableInt32(n) },
                { typeof (long?[]), (r, n) => (long?[]?)r.ReadArrayOfNullableInt64(n) },
                { typeof (float?[]), (r, n) => (float?[]?)r.ReadArrayOfNullableFloat32(n) },
                { typeof (double?[]), (r, n) => (double?[]?)r.ReadArrayOfNullableFloat64(n) },
                { typeof (HBigDecimal?[]), (r, n) => (HBigDecimal?[]?)r.ReadArrayOfDecimal(n) },
                { typeof (HLocalTime?[]), (r, n) => (HLocalTime?[]?)r.ReadArrayOfTime(n) },
                { typeof (HLocalDate?[]), (r, n) => (HLocalDate?[]?)r.ReadArrayOfDate(n) },
                { typeof (HLocalDateTime?[]), (r, n) => (HLocalDateTime?[]?)r.ReadArrayOfTimeStamp(n) },
                { typeof (HOffsetDateTime?[]), (r, n) => (HOffsetDateTime?[]?)r.ReadArrayOfTimeStampWithTimeZone(n) },
                { typeof (string[]), (r, n) => (string?[]?)r.ReadArrayOfString(n) },

                // </generated>

                { typeof (char), (r, n) => (char)r.ReadInt16(n) },
                { typeof (char?), (r, n) => (char?)r.ReadNullableInt16(n) },
                { typeof (char[]), (r, n) => ToArray(r.ReadArrayOfInt16(n), x => (char)(short)x) },
                { typeof (char?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfNullableInt16(n), x => (char?)(short?)x) },

// ReSharper restore RedundantCast
#pragma warning restore IDE0004

            };

        private static Action<ICompactWriter, string, object?> GetWriter(Type type)
        {
            static void WriteUnknownType(ICompactWriter writer, string name, Type type, object? obj)
            {
                if (type.IsArray && type.GetArrayRank() == 1)
                {
                    var elementType = type.GetElementType()!; // cannot be null, type is an array
                    var writeObject = writer.GetType().GetMethod(nameof(ICompactWriter.WriteArrayOfCompact));
                    var writeObjectOfType = writeObject!.MakeGenericMethod(elementType);
                    writeObjectOfType.Invoke(writer, new[] { name, obj });
                }
                else
                {
                    var writeObject = writer.GetType().GetMethod(nameof(ICompactWriter.WriteCompact));
                    var writeObjectOfType = writeObject!.MakeGenericMethod(type);
                    writeObjectOfType.Invoke(writer, new[] { name, obj });
                }
            }

            return Writers.TryGetValue(type, out var write)
                ? write
                : (writer, name, obj) => WriteUnknownType(writer, name, type, obj);
        }

        private static Func<ICompactReader, string, object?> GetReader(Type type)
        {
            static object? ReadUnknownType(ICompactReader reader, string name, Type type)
            {
                if (type.IsArray && type.GetArrayRank() == 1)
                {
                    var elementType = type.GetElementType()!; // cannot be null, type is an array
                    if (elementType.IsInterface)
                        throw new SerializationException($"Interface type {elementType} is not supported by reflection serialization.");
                    var readObject = reader.GetType().GetMethod(nameof(ICompactReader.ReadArrayOfCompact));
                    var readObjectOfType = readObject!.MakeGenericMethod(elementType);
                    return readObjectOfType.Invoke(reader, new object[] { name });
                }
                else
                {
                    if (type.IsInterface)
                        throw new SerializationException($"Interface type {type} is not supported by reflection serialization.");
                    var readObject = reader.GetType().GetMethod(nameof(ICompactReader.ReadCompact));
                    var readObjectOfType = readObject!.MakeGenericMethod(type);
                    return readObjectOfType.Invoke(reader, new object[] { name });
                }
            }

            return Readers.TryGetValue(type, out var read)
                ? read
                : (reader, name) => ReadUnknownType(reader, name, type);
        }

        private static PropertyInfo[] GetProperties(Type objectType)
            => Properties.GetOrAdd(objectType,
                type => type
                    // "[GetProperties] returns all public instance and static properties, both those defined
                    // by the type represented by the current Type object as well as those inherited from its
                    // base types."
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.CanRead && x.CanWrite)
                    .ToArray()
            );

        /// <inheritdoc />
        public virtual object Read(ICompactReader reader)
        {
            if (!(reader is CompactReader r))
                throw new ArgumentException($"Reader is not of type {nameof(CompactReader)}.", nameof(reader));

            var typeOfObj = r.ObjectType;

            object? obj;
            try
            {
                obj = Activator.CreateInstance(typeOfObj);
            }
            catch (Exception e)
            {
                throw new SerializationException($"Failed to create an instance of type {typeOfObj}.", e);
            }

            if (obj == null)
                throw new SerializationException($"Failed to create an instance of type {typeOfObj}.");

            // TODO: consider emitting the property setters

            foreach (var property in GetProperties(typeOfObj))
            {
                if (r.ValidateFieldNameInvariant(property.Name, out var fieldName))
                {
                    Type? t0 = null;
                    Type? t1 = null;
                    var isEnum = property.PropertyType.IsEnum;
                    var isNullableEnum = property.PropertyType.IsNullableOfT(out t0) && t0.IsEnum;
                    var isArray = property.PropertyType.IsArray && property.PropertyType.GetArrayRank() == 1;
                    var isArrayOfEnum = isArray && property.PropertyType.GetElementType()!.IsEnum;
                    var isArrayOfNullableEnum = isArray && property.PropertyType.GetElementType().IsNullableOfT(out t1) && t1.IsEnum;

                    object? value = null;
                    if (isEnum || isNullableEnum)
                    {
                        var enumType = isEnum ? property.PropertyType : t0;
                        var o = GetReader(typeof (string))(reader, fieldName);
                        if (o is string s)
                        {
                            var parsed = Enum.Parse(enumType, s);
                            value = isEnum ? parsed : typeof(Nullable<>).MakeGenericType(enumType).GetConstructor(new[] { enumType })!.Invoke(new[] { parsed });
                        }
                    }
                    else if (isArrayOfEnum || isArrayOfNullableEnum)
                    {
                        var enumType = isArrayOfEnum ? property.PropertyType.GetElementType()! : t1!;

                        var o = GetReader(typeof (string[]))(reader, fieldName);
                        if (o is Array a)
                        {
                            var elementType = isArrayOfNullableEnum ? typeof (Nullable<>).MakeGenericType(enumType) : enumType;
                            var elementCtor = isArrayOfNullableEnum ? elementType.GetConstructor(new[] { enumType }) : null;
                            var valueArray = Array.CreateInstance(elementType, a.Length);
                            value = valueArray;
                            for (var i = 0; i < a.Length; i++)
                                if (a.GetValue(i) is string s)
                                {
                                    var parsed = Enum.Parse(enumType, s);
                                    valueArray.SetValue(isArrayOfEnum ? parsed : elementCtor!.Invoke(new[] { parsed }), i);
                                }
                        }
                    }
                    else
                    {
                        value = GetReader(property.PropertyType)(reader, fieldName);
                    }

                    property.SetValue(obj, value);
                }
            }

            return obj;
        }

        private static bool GetValidFieldName(ICompactWriter writer, string propertyName, [NotNullWhen(true)] out string? fieldName)
        {
            // writer here can be a true CompactWriter, which holds a schema, and is able to provide
            // 'valid' field names (mostly mapping different casing in names e.g. 'Name' to 'name'),
            // but it also can be a SchemaBuilderWriter in which case we are writing the schema, and
            // have to accept the property names as field names.

            fieldName = propertyName;
            return !(writer is CompactWriter w) || w.ValidateFieldNameInvariant(propertyName, out fieldName);
        }

        /// <inheritdoc />
        public virtual void Write(ICompactWriter writer, object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            VerifyTypeIsSupported(obj);

            // TODO: consider emitting the property getters

            foreach (var property in GetProperties(obj.GetType()))
            {
                if (GetValidFieldName(writer, property.Name, out var fieldName))
                {
                    var isEnum = property.PropertyType.IsEnum;
                    var isNullableEnum = property.PropertyType.IsNullableOfT(out var t0) && t0.IsEnum;
                    var isArray = property.PropertyType.IsArray && property.PropertyType.GetArrayRank() == 1;
                    var isArrayOfEnum = isArray && property.PropertyType.GetElementType()!.IsEnum;
                    var isArrayOfNullableEnum = isArray && property.PropertyType.GetElementType().IsNullableOfT(out var t1) && t1.IsEnum;
                    var type =
                        isEnum || isNullableEnum ? typeof (string) :
                        isArrayOfEnum || isArrayOfNullableEnum ? typeof (string[]) :
                        property.PropertyType;
                    var value =
                        isEnum || isNullableEnum ? property.GetValue(obj)?.ToString() :
                        isArrayOfEnum || isArrayOfNullableEnum ? EnumsToStrings(property.GetValue(obj)) :
                        property.GetValue(obj);
                    GetWriter(type)(writer, fieldName, value);
                }
            }
        }

        private static string?[]? EnumsToStrings(object? o)
        {
            if (o == null) return null;
            var x = (Array)o;
            var r = new string?[x.Length];
            for (var i = 0; i < x.Length; i++) r[i] = x.GetValue(i)?.ToString();
            return r;
        }

        private static void VerifyTypeIsSupported(object o)
        {
            // for FullName to be null, the type would need to be derived from an open generic somehow,
            // which makes no sense since it is the actual type of a concrete object. we can assume that
            // type is not going to be null.
            var type = o.GetType();
            var name = type.FullName!;

            if (name.StartsWith("<", StringComparison.Ordinal))
                throw new SerializationException($"The {type} type cannot be serialized via zero-configuration "
                                                 + "Compact serialization because anonymous types are not supported.");

            if (NonSupportedNamespaces.Any(x => name.StartsWith(x, StringComparison.Ordinal)))
                throw new SerializationException($"The {name} type is not supported by zero-configuration Compact "
                                                 + "serialization. Consider writing a custom ICompactSerializer for this type.");
        }

        // for now, we do *not* support all System.* namespaces
        // we may want to add more later on
        private static readonly string[] NonSupportedNamespaces = { "System." };
    }
}
