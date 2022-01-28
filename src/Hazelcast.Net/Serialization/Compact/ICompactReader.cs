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

#nullable enable

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Defines methods for reading fields from a compact-serialized blob.
    /// </summary>
    public interface ICompactReader
    {
        // for types that support both a nullable and a non-nullable version, we define
        // the two methods, thus avoiding allocating an extra nullable struct and/or
        // boxing when it is not necessary.

        /// <summary>Reads a <see cref="FieldKind.Boolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool ReadBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.BooleanRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// /// <returns>The value of the field.</returns>
        bool? ReadBooleanRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool[]? ReadBooleans(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfBooleanRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool?[]? ReadBooleanRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte ReadSignedByte(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger8Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte? ReadSignedByteRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte[]? ReadSignedBytes(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger8Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte?[]? ReadSignedByteRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short ReadShort(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger16Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short? ReadShortRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short[]? ReadShorts(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger16Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short?[]? ReadShortRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int ReadInt(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int? ReadIntRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int[]? ReadInts(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int?[]? ReadIntRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long ReadLong(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long? ReadLongRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long[]? ReadLongs(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long?[]? ReadLongRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.Float"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float ReadFloat(string name);

        /// <summary>Reads a <see cref="FieldKind.FloatRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float? ReadFloatRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfFloat"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float[]? ReadFloats(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfFloatRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float?[]? ReadFloatRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.Double"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double ReadDouble(string name);

        /// <summary>Reads a <see cref="FieldKind.DoubleRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double? ReadDoubleRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDouble"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double[]? ReadDoubles(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDoubleRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double?[]? ReadDoubleRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.String"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        string? ReadString(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfString"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        string?[]? ReadStrings(string name);

        /// <summary>Reads a <see cref="FieldKind.DecimalRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        decimal? ReadDecimalRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDecimalRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        decimal?[]? ReadDecimalRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.TimeRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeSpan? ReadTimeRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTimeRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeSpan?[]? ReadTimeSpanRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.DateRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime? ReadDateTimeRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDateRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime?[]? ReadDateTimeRefs(string name);

        // FIXME - reader and writer, refactor date/time support
        // FIXME - reader and writer, refactor BigDecimal support
        // FIXME - reader and writer, WriteCompact vs WriteObject?
        // FIXME - reader and writer, WriteTimeRef the *ref* is important

        /// <summary>Reads a <see cref="FieldKind.TimeStampWithOffsetRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTimeOffset? ReadDateTimeOffsetRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTimeStampWithOffsetRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTimeOffset?[]? ReadDateTimeOffsetRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.Object"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        object? ReadObject(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfObject"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        object?[]? ReadObjects(string name);
    }
}
