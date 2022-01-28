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
using System.Linq;
using System.Text;

namespace Hazelcast.Serialization.Compact
{
    // FIXME: add WriteBigDecimal(string name, BigDecimal value)?
    // TODO: consider introducing TimeOnly and DateOnly support with .NET 6

    // FIXME: what shall we do if we write a value that is too big? throw, or?

    // FIXME
    // we don't support unsigned integers and that includes 'byte'
    // some tests should validate what can be done with 'byte' etc

    public static class CompactWriterExtensions
    {
        private static ICompactWriter SafeWriter(ICompactWriter writer)
            => writer ?? throw new ArgumentNullException(nameof(writer));

        /// <summary>Writes a <see cref="byte"/> field.</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        public static void WriteByte(this ICompactWriter writer, string name, byte value)
            => SafeWriter(writer).WriteSignedByte(name, (sbyte)value);

        /// <summary>Writes a <see cref="char"/> field.</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        public static void WriteChar(this ICompactWriter writer, string name, char value)
            => SafeWriter(writer).WriteShort(name, (short)value);

        /// <summary>Writes an array-of-<see cref="byte"/> field.</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        public static void WriteBytes(this ICompactWriter writer, string name, byte[] value)
            => SafeWriter(writer).WriteSignedBytes(name, value.Cast<sbyte>().ToArray());

        /// <summary>Writes a array-of-<see cref="char"/> field.</summary>
        /// <param name="writer">The compact writer.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        public static void WriteChars(this ICompactWriter writer, string name, char[] value)
            => SafeWriter(writer).WriteShorts(name, value.Cast<short>().ToArray());
    }
}
