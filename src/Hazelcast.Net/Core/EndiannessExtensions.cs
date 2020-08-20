﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="Endianness"/> enumeration.
    /// </summary>
    public static class EndiannessExtensions
    {
        /// <summary>
        /// Gets the native endianness of the computer architecture where the code is executing.
        /// </summary>
        public static Endianness NativeEndianness => BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;

        /// <summary>
        /// Determines whether this endianness is 'big-endian'.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>true if this endianness is 'big-endian'; otherwise false.</returns>
        public static bool IsBigEndian(this Endianness endianness) => endianness == Endianness.BigEndian;

        /// <summary>
        /// Determines whether this endianness is 'little-endian'.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>true if this endianness is 'little-endian'; otherwise false.</returns>
        public static bool IsLittleEndian(this Endianness endianness) => endianness == Endianness.LittleEndian;

        /// <summary>
        /// Resolves an endianness.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <param name="defaultValue">An optional default value.</param>
        /// <returns>The <paramref name="endianness"/> if it is specified, else the <paramref name="defaultValue"/>.</returns>
        public static Endianness Resolve(this Endianness endianness, Endianness defaultValue = Endianness.BigEndian)
            => endianness switch
               {
                   Endianness.Unspecified => defaultValue,
                   Endianness.Native => NativeEndianness,
                   Endianness.LittleEndian => endianness,
                   Endianness.BigEndian => endianness,
                   _ => throw new NotSupportedException()
               };
    }
}
