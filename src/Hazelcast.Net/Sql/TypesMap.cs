// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;

namespace Hazelcast.Sql
{
    internal static class TypesMap
    {
        // Serialization/ConstantSerializerDefinitions defines the constant serializers, for
        // instance it registers Serialization/ConstantSerializers/ByteSerializer as the serializer
        // for C# 'byte' type -- and that serializer uses WriteByte/ReadByte -- so a C# 'byte' goes
        // as a Java 'byte' even though C# 'byte' is unsigned and Java 'byte' is signed.
        //
        // We *have* to define an equivalent mapping here, i.e. we cannot decide, here, to map
        // C# 'byte' to Java 'int' -- that would cause an error when running SQL queries.
        //
        // Users have to be aware of the impedance mismatch.

        private static readonly Dictionary<Type, string>  CSharpToJavaMap = new Dictionary<Type, string>
            {
                // Java 'boolean' and C# 'bool' are equivalent.
                // Java 'Boolean' is the corresponding reference type (nullable).
                { typeof (bool), "boolean" },
                { typeof (bool?), "java.lang.Boolean" },

                // Java 'char' and C# 'char' are equivalent (single 16-bit Unicode character).
                // Java 'Char' is the corresponding reference type (nullable).
                { typeof (char), "char" },
                { typeof (char?), "java.lang.Char" },

                // Java 'byte' is an 8-bit signed integer (from -128 to +127, inclusive) number.
                // Java 'Byte' is the corresponding reference type (nullable) number.
                // C# 'byte' is an 8-bit unsigned integer (from 0 to +255, inclusive) number.
                // C# 'sbyte' is an 8-bit unsigned integer (from 0 to +255, inclusive) number.
                { typeof (sbyte), "byte" },
                { typeof (sbyte?), "java.lang.Byte" },
                { typeof (byte), "byte" },
                { typeof (byte?), "java.lang.Byte" },

                // Java 'short' is a 16-bit signed integer (from -32,768 to 32,767, inclusive) number.
                // C# 'short' is a 16-bit signed integer (from -32,768 to 32,767, inclusive) number.
                // C# 'ushort' is a 16-bit unsigned integer (from 0 to 65,535, inclusive) number.
                { typeof (short), "short" },
                { typeof (short?), "java.lang.Short" },
                { typeof (ushort), "short" },
                { typeof (ushort?), "java.lang.Short" },

                // Java 'int' is a 32-bit signed integer (from -2^31 to 2^31-1, inclusive) number.
                // Java 'Integer' is the corresponding reference type (nullable) number.
                // C# 'int' is a 32-bit signed integer (from -2^31 to 2^31-1, inclusive) number.
                // C# 'uint' is a 32-bit unsigned integer (from 0 to 2^32-1, inclusive) number.
                { typeof (int), "int" },
                { typeof (int?), "java.lang.Integer" },
                { typeof (uint), "int"},
                { typeof (uint?), "java.lang.Integer" },

                // Java 'long' is a 64-bit signed integer (from -2^63 to 2^63-1, inclusive) number.
                // Java 'Long' is the corresponding reference type (nullable) number.
                // C# 'long' is a 64-bit signed integer (from -2^63 to 2^63-1, inclusive) number.
                // C# 'ulong' is a 64-bit unsigned integer (from 0 to 2^64-1, inclusive) number.
                { typeof (long), "long" },
                { typeof (long?), "java.lang.Long" },
                { typeof (ulong), "java.math.long" },
                { typeof (ulong?), "java.math.Long" },

                // Java 'float' is a single-precision 32-bit IEEE 754 floating point number.
                // Java 'double' is a double-precision 64-bit IEEE 754 floating point number.
                // C# 'float' and 'double' are the same
                // Java 'java.lang.Float' and '.Double' are the corresponding reference types (nullable).
                { typeof (float), "float" },
                { typeof (float?), "java.lang.Float" },
                { typeof (double), "double" },
                { typeof (double?), "java.lang.Double" },

                // Java 'string' is equivalent to C# 'string'
                { typeof (string), "java.lang.String" },

                // Java 'BigDecimal' is an arbitrary-precision signed decimal number.
                // Java 'BigInteger' is an arbitrary-precision signed integer number.
                // Both are reference types (nullable).
                // C# 'decimal' is a 128-bits decimal value ... this sort-of works.
                { typeof (decimal), "java.math.BigDecimal" },
                { typeof (decimal?), "java.math.BigDecimal" },

                // custom
                // TODO: should we have mappings for DateTime and TimeSpan?
                { typeof (HLocalDate), "java.time.LocalDate" }, // SQL DATE
                { typeof (HLocalTime), "java.time.LocalTime" }, // SQL TIME
                { typeof (HLocalDateTime), "java.time.LocalDateTime" }, // SQL TIMESTAMP
                { typeof (HOffsetDateTime), "java.time.OffsetDateTime" }, // SQL TIMESTAMP_WITH_TIME_ZONE
                { typeof (HBigDecimal), "java.math.BigDecimal" }, // SQL DECIMAL
            };

        private static readonly Dictionary<Type, string> CSharpToSqlMap = new Dictionary<Type, string>
        {
            // TODO: many types are missing here
            //{ typeof (object), "BIGINT" },
            { typeof (bool), "BOOLEAN" },
            { typeof (char), "CHAR" },
            //{ typeof (object), "CHARACTER" },
            //{ typeof (object), "DATE" },
            //{ typeof (object), "DEC" },
            //{ typeof (object), "DECIMAL" },
            { typeof (double), "DOUBLE" },
            { typeof (int), "INT" },
            //{ typeof (object), "INTEGER" },
            //{ typeof (object), "NUMERIC" },
            //{ typeof (object), "OBJECT" },
            //{ typeof (object), "REAL" },
            //{ typeof (object), "SMALLINT" },
            //{ typeof (object), "TIME" },
            //{ typeof (object), "TIMESTAMP" },
            //{ typeof (object), "TINYINT" },
            { typeof (string), "VARCHAR" }
        };

        /// <summary>
        /// Maps a C# type to Java.
        /// </summary>
        /// <typeparam name="T">The type to map.</typeparam>
        /// <returns>The name of the Java type.</returns>
        public static string ToJava<T>() => ToJava(typeof (T));

        /// <summary>
        /// Tries to map a C# type to Java.
        /// </summary>
        /// <typeparam name="T">The type to map.</typeparam>
        /// <param name="javaType">The Java type name.</param>
        /// <returns>Whether the type could be mapped to a Java type.</returns>
        public static bool TryToJava<T>(out string javaType) => TryToJava(typeof (T), out javaType);

        /// <summary>
        /// Maps a C# type to Java.
        /// </summary>
        /// <param name="type">The type to map.</param>
        /// <returns>The name of the Java type.</returns>
        public static string ToJava(Type type)
        {
            if (CSharpToJavaMap.TryGetValue(type, out var javaType))
                return javaType;

            throw new NotSupportedException($"Failed to map C# type {type.Name} to a Java type.");
        }

        /// <summary>
        /// Tries to map a C# type to Java.
        /// </summary>
        /// <param name="type">The type to map.</param>
        /// <param name="javaType">The Java type name.</param>
        /// <returns>Whether the type could be mapped to a Java type.</returns>
        public static bool TryToJava(Type type, out string javaType)
            => CSharpToJavaMap.TryGetValue(type, out javaType);

        /// <summary>
        /// Maps a C# type to Sql.
        /// </summary>
        /// <typeparam name="T">The type to map.</typeparam>
        /// <returns>The name of the Sql type.</returns>
        public static string ToSql<T>() => ToSql(typeof (T));

        /// <summary>
        /// Tries to map a C# type to Sql.
        /// </summary>
        /// <typeparam name="T">The type to map.</typeparam>
        /// <param name="sqlType">The Sql type name.</param>
        /// <returns>Whether the type could be mapped to a Sql type.</returns>
        public static bool TryToSql<T>(out string sqlType) => TryToSql(typeof (T), out sqlType);

        /// <summary>
        /// Maps a C# type to Sql.
        /// </summary>
        /// <param name="type">The type to map.</param>
        /// <returns>The name of the Sql type.</returns>
        public static string ToSql(Type type)
        {
            if (CSharpToSqlMap.TryGetValue(type, out var sqlType))
                return sqlType;

            throw new NotSupportedException($"Failed to map C# type {type.Name} to a SQL type.");
        }

        /// <summary>
        /// Tries to map a C# type to Sql.
        /// </summary>
        /// <param name="type">The type to map.</param>
        /// <param name="sqlType">The Sql type name.</param>
        /// <returns>Whether the type could be mapped to a Sql type.</returns>
        public static bool TryToSql(Type type, out string sqlType)
            => CSharpToSqlMap.TryGetValue(type, out sqlType);
    }
}
