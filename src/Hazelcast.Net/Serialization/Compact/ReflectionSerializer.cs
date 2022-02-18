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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        // FIXME - missing reflection serializer writers and readers
        // FIXME - must handle the non-primitive types correctly

        private static readonly Dictionary<Type, Action<ICompactWriter, string, object?>> Writers 
            = new Dictionary<Type, Action<ICompactWriter, string, object?>>
            {
                { typeof (int), (w, n, o) => w.WriteInt32(n, UnboxNonNull<int>(o)) },
                { typeof (int?), (w, n, o) => w.WriteNullableInt32(n, (int?)o) },

                { typeof (decimal), (w, n, o) => w.WriteNullableDecimal(n, (HBigDecimal) UnboxNonNull<decimal>(o)) },
                { typeof (decimal?), (w, n, o) => w.WriteNullableDecimal(n, (HBigDecimal?) (decimal?) o) },

                // there is no typeof nullable reference type (e.g. string?) since they are not
                // actual CLR types, so we have to register writers here against the actual types
                // (e.g. string) even though the value we write may be null.

                { typeof (string), (w, n, o) => w.WriteNullableString(n, (string?)o) }

                // FIXME - missing reflection serializer writers
            };

        private static readonly Dictionary<Type, Func<ICompactReader, string, object?>> Readers 
            = new Dictionary<Type, Func<ICompactReader, string, object?>>
            {
// ReSharper disable RedundantCast
#pragma warning disable IDE0004

                // some casts are redundant, but let's force ourselves to cast everywhere,
                // so that we are 100% we detect potential type mismatch errors

                { typeof (int), (r, n) => (int) r.ReadInt32(n) },
                { typeof (int?), (r, n) => (int?) r.ReadNullableInt32(n) },

                { typeof (decimal), (r, n) => (decimal) ValueNonNull(r.ReadNullableDecimal(n)) },
                { typeof (decimal?), (r, n) => (decimal?) r.ReadNullableDecimal(n) },

                // there is no typeof nullable reference type (e.g. string?) since they are not
                // actual CLR types, so we have to register readers here against the actual types
                // (e.g. string) even though the value we read may be null.

                { typeof (string), (r, n) => (string?) r.ReadNullableString(n) }

                // FIXME - missing reflection serializer readers

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
                    // FIXME - cache
                    if (!(reader is CompactReader r)) throw new ArgumentException("", nameof(reader)); // FIXME - exception message
                    var readObject = typeof (CompactReader).GetMethod("ReadObjectRef");
                    var readObjectOfType = readObject.MakeGenericMethod(type);
                    return readObjectOfType.Invoke(r, new object[] { name });
                };
        }

        // FIXME - dead code
        /*
        private static bool TryGetWriter(Type type, out Action<ICompactWriter, string, object?> write)
        {
            if (Writers.TryGetValue(type, out write)) return true;

            // assume everything that is not a known type, is an object to compact-serializer
            write = (w, n, o) => w.WriteObjectRef(n, o);
            return true;
        }

        private static bool TryGetReader(Type type, out Func<ICompactReader, string, object?> read)
        {
            if (Readers.TryGetValue(type, out read)) return true;

            read = (r, n) =>
            {
                if (!(r is CompactReader reader)) throw new ArgumentException();
                var ro = typeof (CompactReader).GetMethod("ReadObjectRef");
                var rog = ro.MakeGenericMethod(type);
                return rog.Invoke(reader, new object[] { n });
            };

            return true;
        }
        */

        /// <inheritdoc />
        public virtual object Read(ICompactReader reader)
        {
            if (!(reader is CompactReader r)) 
                throw new ArgumentException($"Reader is not of type {nameof(CompactReader)}.", nameof(reader));

            var typeOfObj = r.ObjectType;

            // FIXME - cache
            // cache the set of properties that need to be read, per type
            // consider emitting property setter calls
            // cache (emit?) the ctor to avoid using the (slow) Activator

            object? obj;
            try
            {
                obj = Activator.CreateInstance(typeOfObj);
            }
            catch (Exception e)
            {
                throw new SerializationException($"Failed to create an instance of type {typeOfObj}.", e);
            }

            var properties = typeOfObj.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                if (property.CanRead && property.CanWrite &&
                    r.GetValidFieldName(property.Name, out var fieldName))
                {
                    property.SetValue(obj, GetReader(property.PropertyType)(reader, fieldName));
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
            return !(writer is CompactWriter w) || w.GetValidFieldName(propertyName, out fieldName);
        }

        /// <inheritdoc />
        public virtual void Write(ICompactWriter writer, object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            // FIXME - cache
            // cache the set of properties that need to be written, per type
            // consider emitting property getter calls

            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                if (property.CanRead &&  property.CanWrite &&
                    GetValidFieldName(writer, property.Name, out var fieldName))
                {
                    GetWriter(property.PropertyType)(writer, fieldName, property.GetValue(obj));
                }
            }
        }
    }
}
