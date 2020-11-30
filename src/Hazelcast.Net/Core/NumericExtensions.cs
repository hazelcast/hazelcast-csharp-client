// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    /// Provides extension methods to the numeric value type (<see cref="long"/>, <see cref="int"/>...).
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// Converts this value to an equivalent <see cref="int"/> clamped value.
        /// </summary>
        /// <param name="value">This value.</param>
        /// <returns>The <see cref="int"/> clamped value equivalent to this <paramref name="value"/>.</returns>
        /// <remarks>
        /// <para>Contrary to <see cref="Convert.ToInt32(long)"/> which throws if the value is outside
        /// the <see cref="int"/> min-max values, this method clamps the returned value with those min-max
        /// values.</para>
        /// </remarks>
        public static int ClampToInt32(this long value)
        {
#if NETSTANDARD2_0
            if (value > int.MaxValue) return int.MaxValue;
            if (value < int.MinValue) return int.MinValue;
            return (int) value;
#else
            return (int) Math.Clamp(value, int.MinValue, int.MaxValue);
#endif
        }

        /// <summary>
        /// Replaces this value by a new value if it is negative.
        /// </summary>
        /// <param name="value">This value.</param>
        /// <param name="newValue">A new value.</param>
        /// <returns>The new value, if this value is negative; otherwise, this value.</returns>
        public static long NegativeAs(this long value, long newValue)
            => value < 0 ? newValue : value;

        /// <summary>
        /// Replaces this value by a new value if it is negative.
        /// </summary>
        /// <param name="value">This value.</param>
        /// <param name="newValue">A new value.</param>
        /// <returns>The new value, if this value is negative; otherwise, this value.</returns>
        public static int NegativeAs(this int value, int newValue)
            => value < 0 ? newValue : value;

        /// <summary>
        /// Replaces this value by a new value if it is zero.
        /// </summary>
        /// <param name="value">This value.</param>
        /// <param name="newValue">A new value.</param>
        /// <returns>The new value, if this value is zero; otherwise, this value.</returns>
        public static long ZeroAs(this long value, long newValue)
            => value == 0 ? newValue : value;

        /// <summary>
        /// Replaces this value by a new value if it is zero.
        /// </summary>
        /// <param name="value">This value.</param>
        /// <param name="newValue">A new value.</param>
        /// <returns>The new value, if this value is zero; otherwise, this value.</returns>
        public static int ZeroAs(this int value, int newValue)
            => value == 0 ? newValue : value;
    }
}
