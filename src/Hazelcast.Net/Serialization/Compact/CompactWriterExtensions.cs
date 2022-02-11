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

// FIXME - eventually remove ReSharper disable when all methods are tested
// ReSharper disable UnusedMember.Global

using System;
using System.Runtime.CompilerServices;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Provides extension methods for the <see cref="ICompactWriter"/> interface.
    /// </summary>
    /// <remarks>
    /// <para>This class provides extension methods with names that are closer to C# names (for instance,
    /// <c>DateTime</c> vs <c>TimeStamp</c>. It also provides extension methods that support types not
    /// natively supported by the <see cref="ICompactWriter"/> interface such as <c>byte</c> or <c>char</c>.</para>
    /// </remarks>
    public static class CompactWriterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ICompactWriter SafeWriter(ICompactWriter writer)
            => writer ?? throw new ArgumentNullException(nameof(writer));

        /// <summary>Writes a <see cref="FieldKind.TimeStampRef"/> field (alias for <see cref="ICompactWriter.WriteTimeStampRef"/>).</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeStampRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDateTimeRef(this ICompactWriter writer, string name, DateTime? value)
            => SafeWriter(writer).WriteTimeStampRef(name, value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeStampRef"/> field (alias for <see cref="ICompactWriter.WriteArrayOfTimeStampRef"/>).</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeStampRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDateTimeRefs(this ICompactWriter writer, string name, DateTime?[]? value)
            => SafeWriter(writer).WriteArrayOfTimeStampRef(name, value);

        /// <summary>Writes a <see cref="FieldKind.TimeStampWithTimeZoneRef"/> field (alias for <see cref="ICompactWriter.WriteTimeStampWithTimeZoneRef"/>).</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeStampWithTimeZoneRef"/> primitive type.
        /// The range of this primitive type is different from the range of <see cref="DateTimeOffset"/>.
        /// Refer to the primitive type documentation for details.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDateTimeOffsetRef(this ICompactWriter writer, string name, DateTimeOffset? value)
            => SafeWriter(writer).WriteTimeStampWithTimeZoneRef(name, value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeStampWithTimeZoneRef"/> field (alias for <see cref="ICompactWriter.WriteArrayOfTimeStampWithTimeZoneRef"/>).</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfTimeStampWithTimeZoneRef"/> primitive type.
        /// The range of this primitive type is different from the range of <see cref="DateTimeOffset"/>.
        /// Refer to the primitive type documentation for details.</para>
        /// </remarks>
        public static void WriteDateTimeOffsetRefs(this ICompactWriter writer, string name, DateTimeOffset?[]? value)
            => SafeWriter(writer).WriteArrayOfTimeStampWithTimeZoneRef(name, value);

        /// <summary>Writes a <see cref="FieldKind.Int8"/> field.</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        public static void WriteByte(this ICompactWriter writer, string name, byte value)
            => SafeWriter(writer).WriteInt8(name, (sbyte)value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt8"/> field.</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        public static void WriteBytes(this ICompactWriter writer, string name, byte[] value)
            => SafeWriter(writer).WriteArrayOfInt8(name, Array.ConvertAll(value, x => (sbyte)x));

        /// <summary>Writes a <see cref="FieldKind.Int16"/> field.</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        public static void WriteChar(this ICompactWriter writer, string name, char value)
            => SafeWriter(writer).WriteInt16(name, (short)value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt16"/> field.</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        public static void WriteChars(this ICompactWriter writer, string name, char[] value)
            => SafeWriter(writer).WriteArrayOfInt16(name, Array.ConvertAll(value, x => (short)x));
    }
}
