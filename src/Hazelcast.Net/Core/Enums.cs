// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Reflection;
using Hazelcast.Exceptions;

namespace Hazelcast.Core;

/// <summary>
/// Parses enumerations.
/// </summary>
internal static class Enums
{
    // static cache for values
    private static readonly ConcurrentDictionary<Type, (Dictionary<string, string>, Dictionary<string, string>)> Cache = new();

    /// <summary>
    /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
    /// </summary>
    /// <typeparam name="T">An enumeration type.</typeparam>
    /// <param name="value">A string containing the name or value to convert.</param>
    /// <returns>An object of type <typeparamref name="T"/> whose value is represented by <paramref name="value"/>.</returns>
    public static T Parse<T>(string value) => (T) Enum.Parse(typeof(T), value);

    private static (Dictionary<string, string>, Dictionary<string, string>) GetCaches(Type enumType)
    {
        return Cache.GetOrAdd(enumType, type =>
        {
            var dotnetToJava = new Dictionary<string, string>();
            var javaToDotnet = new Dictionary<string, string>();
            foreach (var fieldInfo in enumType.GetFields().Where(x => x.FieldType.IsEnum))
            {
                var dotnetName = fieldInfo.Name;
                var javaNameAttribute = fieldInfo.GetCustomAttribute<JavaNameAttribute>();
                var javaName = javaNameAttribute == null ? dotnetName : javaNameAttribute.Name;
                dotnetToJava.Add(dotnetName, javaName);
                javaToDotnet.Add(javaName, dotnetName);
            }
            return (dotnetToJava, javaToDotnet);
        });
    }

    /// <summary>
    /// Converts the string representation of the Java name of one or more enumerated constants to an equivalent enumerated object.
    /// </summary>
    /// <typeparam name="T">An enumeration type.</typeparam>
    /// <param name="javaValue">A string containing the Java name to convert.</param>
    /// <returns>An object of type <typeparamref name="T"/> whose value is represented by <paramref name="javaValue"/>.</returns>
    public static T ParseJava<T>(string javaValue)
    {
        if (string.IsNullOrWhiteSpace(javaValue)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(javaValue));

        var (_, javaToDotnet) = GetCaches(typeof(T));
        return javaToDotnet.TryGetValue(javaValue, out var dotnetValue)
            ? (T) Enum.Parse(typeof(T), dotnetValue)
            : throw new ArgumentException("Value is not one of the named constants defined for the enumeration.", nameof(javaValue));
    }

    /// <summary>
    /// Converts an enum value to its equivalent Java string representation.
    /// </summary>
    /// <typeparam name="T">An enumeration type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>The Java string representation of the enum value.</returns>
    public static string ToJavaString<T>(this T value) where T : Enum
    {
        var (dotnetToJava, _) = GetCaches(typeof(T));
        var dotnetValue = value.ToString();
        return dotnetToJava.TryGetValue(dotnetValue, out var javaValue)
            ? javaValue
            : dotnetValue;
    }

    /// <summary>
    /// Specifies the name that Java uses for this value.
    /// </summary>
    /// <remarks>
    /// <para>The Java name is used when the enum value is serialized as a string.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    internal class JavaNameAttribute : Attribute
    {
        /// <summary>
        /// Specifies the name that Java uses for this value.
        /// </summary>
        /// <param name="name">The name that Java uses for this value.</param>
        /// <remarks>
        /// <para>The Java name is used when the enum value is serialized as a string.</para>
        /// </remarks>
        public JavaNameAttribute(string name)
        {
            Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
        }

        /// <summary>
        /// Gets the name that Java uses for the value.
        /// </summary>
        public string Name { get; }
    }
}
