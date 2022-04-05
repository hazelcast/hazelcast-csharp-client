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
using Hazelcast.Models;

#nullable enable

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Implements a reflection-based compact serializer.
    /// </summary>
    internal class ReflectionSerializer : ICompactSerializer<object>
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        /// <inheritdoc />
        public string TypeName => throw new InvalidOperationException();

        private static T UnboxNonNull<T>(object? value)
        {
            if (value != null) return (T)value;
            throw new InvalidOperationException($"Cannot unbox null value as {typeof (T)}.");
        }

        private static T ValueNonNull<T>(T? value)
            where T : struct
        {
            if (value.HasValue) return value.Value;
            throw new InvalidOperationException($"Cannot return null value as {typeof(T)}.");
        }

        private static readonly Dictionary<Type, Action<ICompactWriter, string, object?>> Writers
            = new Dictionary<Type, Action<ICompactWriter, string, object?>>
            {
                // there is no typeof nullable reference type (e.g. string?) since they are not
                // actual CLR types, so we have to register writers here against the actual types
                // (e.g. string) even though the value we write may be null.

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
                { typeof (HBigDecimal), (w, n, o) => w.WriteNullableDecimal(n, (HBigDecimal?)o) },
                { typeof (HBigDecimal?), (w, n, o) => w.WriteNullableDecimal(n, (HBigDecimal?)o) },
                { typeof (string), (w, n, o) => w.WriteNullableString(n, (string?)o) },
                { typeof (HLocalTime), (w, n, o) => w.WriteNullableTime(n, (HLocalTime?)o) },
                { typeof (HLocalTime?), (w, n, o) => w.WriteNullableTime(n, (HLocalTime?)o) },
                { typeof (HLocalDate), (w, n, o) => w.WriteNullableDate(n, (HLocalDate?)o) },
                { typeof (HLocalDate?), (w, n, o) => w.WriteNullableDate(n, (HLocalDate?)o) },
                { typeof (HLocalDateTime), (w, n, o) => w.WriteNullableTimeStamp(n, (HLocalDateTime?)o) },
                { typeof (HLocalDateTime?), (w, n, o) => w.WriteNullableTimeStamp(n, (HLocalDateTime?)o) },
                { typeof (HOffsetDateTime), (w, n, o) => w.WriteNullableTimeStampWithTimeZone(n, (HOffsetDateTime?)o) },
                { typeof (HOffsetDateTime?), (w, n, o) => w.WriteNullableTimeStampWithTimeZone(n, (HOffsetDateTime?)o) },
                { typeof (bool?[]), (w, n, o) => w.WriteArrayOfNullableBoolean(n, (bool?[]?)o) },
                { typeof (sbyte?[]), (w, n, o) => w.WriteArrayOfNullableInt8(n, (sbyte?[]?)o) },
                { typeof (short?[]), (w, n, o) => w.WriteArrayOfNullableInt16(n, (short?[]?)o) },
                { typeof (int?[]), (w, n, o) => w.WriteArrayOfNullableInt32(n, (int?[]?)o) },
                { typeof (long?[]), (w, n, o) => w.WriteArrayOfNullableInt64(n, (long?[]?)o) },
                { typeof (float?[]), (w, n, o) => w.WriteArrayOfNullableFloat32(n, (float?[]?)o) },
                { typeof (double?[]), (w, n, o) => w.WriteArrayOfNullableFloat64(n, (double?[]?)o) },
                { typeof (HBigDecimal?[]), (w, n, o) => w.WriteArrayOfNullableDecimal(n, (HBigDecimal?[]?)o) },
                { typeof (HLocalTime?[]), (w, n, o) => w.WriteArrayOfNullableTime(n, (HLocalTime?[]?)o) },
                { typeof (HLocalDate?[]), (w, n, o) => w.WriteArrayOfNullableDate(n, (HLocalDate?[]?)o) },
                { typeof (HLocalDateTime?[]), (w, n, o) => w.WriteArrayOfNullableTimeStamp(n, (HLocalDateTime?[]?)o) },
                { typeof (HOffsetDateTime?[]), (w, n, o) => w.WriteArrayOfNullableTimeStampWithTimeZone(n, (HOffsetDateTime?[]?)o) },
                { typeof (string[]), (w, n, o) => w.WriteArrayOfNullableString(n, (string?[]?)o) },

                // </generated>
            };

        private static readonly Dictionary<Type, Func<ICompactReader, string, object?>> Readers
            = new Dictionary<Type, Func<ICompactReader, string, object?>>
            {
// ReSharper disable RedundantCast
#pragma warning disable IDE0004

                // some casts are redundant, but let's force ourselves to cast everywhere,
                // so that we are 100% we detect potential type mismatch errors

                // there is no typeof nullable reference type (e.g. string?) since they are not
                // actual CLR types, so we have to register readers here against the actual types
                // (e.g. string) even though the value we read may be null.

                // do NOT remove nor alter the <generated></generated> lines!
                // <generated>

                { typeof (bool), (r, n) => (bool) r.ReadBoolean(n) },
                { typeof (sbyte), (r, n) => (sbyte) r.ReadInt8(n) },
                { typeof (short), (r, n) => (short) r.ReadInt16(n) },
                { typeof (int), (r, n) => (int) r.ReadInt32(n) },
                { typeof (long), (r, n) => (long) r.ReadInt64(n) },
                { typeof (float), (r, n) => (float) r.ReadFloat32(n) },
                { typeof (double), (r, n) => (double) r.ReadFloat64(n) },
                { typeof (bool[]), (r, n) => (bool[]?) r.ReadArrayOfBoolean(n) },
                { typeof (sbyte[]), (r, n) => (sbyte[]?) r.ReadArrayOfInt8(n) },
                { typeof (short[]), (r, n) => (short[]?) r.ReadArrayOfInt16(n) },
                { typeof (int[]), (r, n) => (int[]?) r.ReadArrayOfInt32(n) },
                { typeof (long[]), (r, n) => (long[]?) r.ReadArrayOfInt64(n) },
                { typeof (float[]), (r, n) => (float[]?) r.ReadArrayOfFloat32(n) },
                { typeof (double[]), (r, n) => (double[]?) r.ReadArrayOfFloat64(n) },
                { typeof (bool?), (r, n) => (bool?) r.ReadNullableBoolean(n) },
                { typeof (sbyte?), (r, n) => (sbyte?) r.ReadNullableInt8(n) },
                { typeof (short?), (r, n) => (short?) r.ReadNullableInt16(n) },
                { typeof (int?), (r, n) => (int?) r.ReadNullableInt32(n) },
                { typeof (long?), (r, n) => (long?) r.ReadNullableInt64(n) },
                { typeof (float?), (r, n) => (float?) r.ReadNullableFloat32(n) },
                { typeof (double?), (r, n) => (double?) r.ReadNullableFloat64(n) },
                { typeof (HBigDecimal), (r, n) => (HBigDecimal?) ValueNonNull(r.ReadNullableDecimal(n)) },
                { typeof (HBigDecimal?), (r, n) => (HBigDecimal?) r.ReadNullableDecimal(n) },
                { typeof (string), (r, n) => (string?) r.ReadNullableString(n) },
                { typeof (HLocalTime), (r, n) => (HLocalTime?) ValueNonNull(r.ReadNullableTime(n)) },
                { typeof (HLocalTime?), (r, n) => (HLocalTime?) r.ReadNullableTime(n) },
                { typeof (HLocalDate), (r, n) => (HLocalDate?) ValueNonNull(r.ReadNullableDate(n)) },
                { typeof (HLocalDate?), (r, n) => (HLocalDate?) r.ReadNullableDate(n) },
                { typeof (HLocalDateTime), (r, n) => (HLocalDateTime?) ValueNonNull(r.ReadNullableTimeStamp(n)) },
                { typeof (HLocalDateTime?), (r, n) => (HLocalDateTime?) r.ReadNullableTimeStamp(n) },
                { typeof (HOffsetDateTime), (r, n) => (HOffsetDateTime?) ValueNonNull(r.ReadNullableTimeStampWithTimeZone(n)) },
                { typeof (HOffsetDateTime?), (r, n) => (HOffsetDateTime?) r.ReadNullableTimeStampWithTimeZone(n) },
                { typeof (bool?[]), (r, n) => (bool?[]?) r.ReadArrayOfNullableBoolean(n) },
                { typeof (sbyte?[]), (r, n) => (sbyte?[]?) r.ReadArrayOfNullableInt8(n) },
                { typeof (short?[]), (r, n) => (short?[]?) r.ReadArrayOfNullableInt16(n) },
                { typeof (int?[]), (r, n) => (int?[]?) r.ReadArrayOfNullableInt32(n) },
                { typeof (long?[]), (r, n) => (long?[]?) r.ReadArrayOfNullableInt64(n) },
                { typeof (float?[]), (r, n) => (float?[]?) r.ReadArrayOfNullableFloat32(n) },
                { typeof (double?[]), (r, n) => (double?[]?) r.ReadArrayOfNullableFloat64(n) },
                { typeof (HBigDecimal?[]), (r, n) => (HBigDecimal?[]?) r.ReadArrayOfNullableDecimal(n) },
                { typeof (HLocalTime?[]), (r, n) => (HLocalTime?[]?) r.ReadArrayOfNullableTime(n) },
                { typeof (HLocalDate?[]), (r, n) => (HLocalDate?[]?) r.ReadArrayOfNullableDate(n) },
                { typeof (HLocalDateTime?[]), (r, n) => (HLocalDateTime?[]?) r.ReadArrayOfNullableTimeStamp(n) },
                { typeof (HOffsetDateTime?[]), (r, n) => (HOffsetDateTime?[]?) r.ReadArrayOfNullableTimeStampWithTimeZone(n) },
                { typeof (string[]), (r, n) => (string?[]?) r.ReadArrayOfNullableString(n) },

                // </generated>

// ReSharper restore RedundantCast
#pragma warning restore IDE0004

            };

        private static Action<ICompactWriter, string, object?> GetWriter(Type type)
        {
            return Writers.TryGetValue(type, out var write)
                ? write
                : (writer, name, obj) => writer.WriteNullableCompact(name, obj);
        }

        private static Func<ICompactReader, string, object?> GetReader(Type type)
        {
            return Readers.TryGetValue(type, out var read)
                ? read
                : (reader, name) =>
                {
                    var readObject = reader.GetType().GetMethod(nameof(ICompactReader.ReadNullableCompact));
                    var readObjectOfType = readObject!.MakeGenericMethod(type);
                    return readObjectOfType.Invoke(reader, new object[] { name });
                };
        }

        private static PropertyInfo[] GetProperties(Type objectType)
            => Properties.GetOrAdd(objectType, 
                type => type
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
            
            // TODO: consider emitting the property setters

            foreach (var property in GetProperties(typeOfObj))
            {
                if (r.ValidateFieldNameInvariant(property.Name, out var fieldName))
                    property.SetValue(obj, GetReader(property.PropertyType)(reader, fieldName));
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

            // TODO: consider emitting the property getters

            foreach (var property in GetProperties(obj.GetType()))
            {
                if (GetValidFieldName(writer, property.Name, out var fieldName))
                    GetWriter(property.PropertyType)(writer, fieldName, property.GetValue(obj));
            }
        }
    }
}
