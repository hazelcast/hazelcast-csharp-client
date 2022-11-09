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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="Type"/> class.
    /// </summary>
    internal static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> Names = new ConcurrentDictionary<Type, string>();
        private static Regex QualifiedNameFilter;

        /// <summary>
        /// Gets the type qualified name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The type qualified name.</returns>
        /// <remarks>
        /// <para>The type qualified name is the assembly qualified name, without the Version, Culture
        /// and PublicKeyToken elements.</para>
        /// </remarks>
        public static string GetQualifiedTypeName(this Type type)
            => Names.GetOrAdd(type, ConstructName);

        // internally, AssemblyQualifiedName is
        // Assembly.CreateQualifiedName(type.Assembly.FullName, type.FullName);
        // with
        // public static string CreateQualifiedName(string? assemblyName, string? typeName) => typeName + ", " + assemblyName;

        // assembly.FullName ends up running an extern method
        // type.FullName ends up in
        //  return ConstructName(ref m_fullname, TypeNameFormatFlags.FormatNamespace | TypeNameFormatFlags.FormatFullInst);
        // whereas
        //  Name -> ConstructName(ref m_name, TypeNameFormatFlags.FormatBasic);
        //  ToString -> ConstructName(ref m_toString, TypeNameFormatFlags.FormatNamespace);
        //
        // ConstructName = new RuntimeTypeHandle(m_runtimeType).ConstructName(formatFlags);
        // and... that ends up running an extern method

        // re-implementing it all by ourselves would be cumbersome, better filter out what we don't want

        private static string ConstructName(Type type)
        {
            var name = type.AssemblyQualifiedName;
            if (name == null) return null;

            QualifiedNameFilter ??= new Regex(", Version=[^,]+, Culture=[^,]+, PublicKeyToken=[a-z0-9]+", RegexOptions.Compiled);
            return QualifiedNameFilter.Replace(name, "");
        }
        
        /// <summary>
        /// Determines whether this type is a <see cref="Nullable{T}"/> type.
        /// </summary>
        /// <param name="type">This type.</param>
        /// <returns><c>true</c> if this type is a <see cref="Nullable{T}"/> type; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">This <paramref name="type"/> is <c>null</c>.</exception>
        public static bool IsNullableOfT(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        /// <summary>
        /// Determines whether this type is a <see cref="Nullable{T}"/> type.
        /// </summary>
        /// <param name="type">This type.</param>
        /// <param name="underlyingType">The underlying type, if the type is nullable.</param>
        /// <returns><c>true</c> if this type is a <see cref="Nullable{T}"/> type; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">This <paramref name="type"/> is <c>null</c>.</exception>
        public static bool IsNullableOfT(this Type type, out Type underlyingType)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
            {
                underlyingType = type.GetGenericArguments()[0];
                return true;
            }

            underlyingType = null;
            return false;
        }

        /// <summary>
        /// Determines whether this type is nullable, i.e. when it supports <c>null</c> value.
        /// </summary>
        /// <param name="type">This type.</param>
        /// <returns><c>true</c> if this type is nullable; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">This <paramref name="type"/> is <c>null</c>.</exception>
        /// <remarks>
        /// <para>A type is nullable when it is a reference type, or a <see cref="Nullable{T}"/> value type.</para>
        /// </remarks>
        public static bool IsNullable(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            // references are nullable
            var isValueType = type.IsValueType;
            if (!isValueType) return true;

            // Nullable<T> are nullable
#pragma warning disable CA1508 // Avoid dead conditional code
            // false positive, https://github.com/dotnet/roslyn-analyzers/issues/4763
            var isNullableType = type.IsNullableOfT();
#pragma warning restore CA1508
            if (isNullableType) return true;

            // anything else is not nullable
            return false;
        }

        /// <summary>
        /// Returns a string representing this type in C# style.
        /// </summary>
        /// <param name="type">This type.</param>
        /// <param name="fqn">Whether to keep fully-qualified name or shorten them.</param>
        /// <returns>A string representing this type in C# style.</returns>
        public static string ToCsString(this Type type, bool fqn = false)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var fullname = type.FullName;
            if (fullname == null) return type.ToString();

            if (type.IsNullableOfT(out var underlyingType))
                return $"{ToCsString(underlyingType, fqn)}?";
            if (type.IsGenericType) 
                return GenericToCsString(type, fullname, fqn);
            if (type.IsArray) 
                return $"{type.GetElementType().ToCsString(fqn)}[{new string(',', type.GetArrayRank() - 1)}]";

            return TypeToCsString(fullname, fqn);
        }

        private static string GenericToCsString(Type type, string fullname, bool fqn)
        {
            var p = fullname.IndexOf('`', StringComparison.Ordinal);
            var def = TypeToCsString(fullname.Substring(0, p), fqn);
            var args = type.GetGenericArguments().Select(x => x.ToCsString(fqn));

            return def + "<" + string.Join(", ", args) + ">";
        }

        private static string TypeToCsString(string fullname, bool fqn)
        {
            if (TypesMap.TryGetValue(fullname, out var typeName))
                return typeName;

            var name = fullname;
            if (!fqn)
            {
                var pos = name.LastIndexOf('.');
                if (pos >= 0) name = name[(pos + 1)..];
            }
            return name.Replace('+', '.');
        }

        private static readonly IDictionary<string, string> TypesMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "System.Int16", "short" },
            { "System.Int32", "int" },
            { "System.Int64", "long" },
            { "System.String", "string" },
            { "System.Object", "object" },
            { "System.Boolean", "bool" },
            { "System.Void", "void" },
            { "System.Char", "char" },
            { "System.Byte", "byte" },
            { "System.UInt16", "ushort" },
            { "System.UInt32", "uint" },
            { "System.UInt64", "ulong" },
            { "System.SByte", "sbyte" },
            { "System.Single", "float" },
            { "System.Double", "double" },
            { "System.Decimal", "decimal" }
        };

        public static T MustBe<T>(this object obj, string name = null)
        {
            if (obj is T t) return t;
            throw new ArgumentException($"Argument '{name ?? nameof(obj)}' is not of type {typeof(T)}.", name ?? nameof(obj));
        }
    }
}
