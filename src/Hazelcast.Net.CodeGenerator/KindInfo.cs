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
using System.Diagnostics.CodeAnalysis;

namespace Hazelcast.CodeGenerator;

/// <summary>
/// Represents details about an entry in the FieldKind enumeration.
/// </summary>
public class KindInfo
{
#pragma warning disable CS8618
    private KindInfo()
    { }
#pragma warning restore CS8618

    /// <summary>
    /// Gets the identifier of the <c>FieldKind</c> (the enum value).
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Indicates whether the FieldKind entry represents an array (ArrayOf...).
    /// </summary>
    public bool IsArray { get; init; }

    /// <summary>
    /// Indicates whether the FieldKind entry supports <c>null</c> values, either
    /// because it is a reference type, or because it is a nullable value type.
    /// </summary>
    /// <remarks>
    /// <para>This does not obligatory mean that the full name contains 'Nullable'.
    /// For instance, <c>Date</c> or <c>Time</c> are nullable as well as
    /// <c>NullableInt8</c>.</para>
    /// </remarks>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Indicates whether the FieldKind entry must support <c>null</c> values,
    ///
    /// </summary>
    public bool IsNullableOnly { get; init; }

    /// <summary>
    /// Indicates whether the underlying type of the FieldKind entry is a value type.
    /// </summary>
    /// <remarks>
    /// <para>For instance, this is true for <c>ArrayOfNullableInt8</c> because <c>Int8</c>
    /// is a value type.</para>
    /// </remarks>
    public bool IsValueType { get; init; }

    /// <summary>
    /// Gets the name of the underlying type of the FieldKind entry, i.e. without 'ArrayOf'
    /// and 'Nullable'.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the full name of the FieldKind entry, including 'ArrayOf' and 'Nullable'.
    /// </summary>
    public string FullName { get; init; }

    /// <summary>
    /// Gets the CLR type of the FieldKind entry.
    /// </summary>
    public string ClrType { get; init; }

    /// <summary>
    /// Gets the map from FieldKind entry underlying types to CLR types.
    /// </summary>
    /// <remarks>
    /// <para>Every FieldKind entry must have an entry in this map.</para>
    /// <para>A <c>null</c> value indicates that the entry is to be ignored.</para>
    /// </remarks>
    public static readonly IDictionary<string, string?> ClrTypes = new Dictionary<string, string?>
    {
        { "Char", null },
        { "Portable", null },
        { "NotAvailable", null },

        { "Boolean", "bool" },
        { "Int8", "sbyte" },
        { "Int16", "short" },
        { "Int32", "int" },
        { "Int64", "long" },
        { "Float32", "float" },
        { "Float64", "double" },
        { "Decimal", "HBigDecimal" },
        { "String", "string" },
        { "Time", "HLocalTime" },
        { "Date", "HLocalDate" },
        { "TimeStamp", "HLocalDateTime" },
        { "TimeStampWithTimeZone", "HOffsetDateTime" },
        { "Compact", "object" },
    };

    /// <summary>
    /// Gets the map from CLR value types to a boolean indicating whether they are supported as non-nullable.
    /// </summary>
    public static readonly IDictionary<string, bool> ValueTypes = new Dictionary<string, bool>
    {
        { "bool", true },
        { "sbyte", true },
        { "short", true },
        { "int", true },
        { "long", true },
        { "float", true },
        { "double", true },
        { "HBigDecimal", false },
        { "HLocalTime", false },
        { "HLocalDate", false },
        { "HLocalDateTime", false },
        { "HOffsetDateTime", false }
    };

    /// <summary>
    /// Parses a <see cref="KindInfo" /> full name.
    /// </summary>
    /// <param name="fullName">The full name of the <c>FieldKind</c>.</param>
    /// <param name="id">The identifier of the <c>FieldKind</c>.</param>
    /// <param name="kindInfo">The <see cref="KindInfo"/>.</param>
    /// <returns><c>true</c> if the full name corresponds to a supported <c>FieldKind</c>,
    /// otherwise <c>false</c>.</returns>
    public static bool TryParse(string fullName, int id, [NotNullWhen(true)] out KindInfo? kindInfo)
    {
        var name = fullName;

        var isArray = name.StartsWith("ArrayOf");
        if (isArray) name = name["ArrayOf".Length..];

        var isNullable = name.StartsWith("Nullable");
        if (isNullable) name = name["Nullable".Length..];

        // detect if an entry is added to the enumeration, that we don't support yet
        if (!ClrTypes.TryGetValue(name, out var clrType))
            throw new Exception($"Entry '{name}' missing in the CLR types map.");

        // skip ignored types
        if (clrType is null)
        {
            kindInfo = null;
            return false;
        }

        var isValueType = ValueTypes.TryGetValue(clrType, out var supportsNonNullable);

        // isNullable = can it be null (either a value type that can be null, or a reference type)
        // isNullableOnly = can it *only* be nullable (some value types, all reference types)
        var isNullableOnly = !isValueType || !supportsNonNullable;
        isNullable |= isNullableOnly;

        kindInfo = new KindInfo
        {
            Id = id,
            Name = name,
            FullName = fullName,
            IsArray = isArray,
            IsNullable = isNullable,
            IsNullableOnly = isNullableOnly,
            IsValueType = isValueType,
            ClrType = clrType
        };
        return true;
    }
}