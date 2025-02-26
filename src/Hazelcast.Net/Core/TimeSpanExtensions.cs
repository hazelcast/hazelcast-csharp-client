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

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="TimeSpan"/> struct.
    /// </summary>
    internal static class TimeSpanExtensions
    {
        /// <summary>
        /// The constant -1ms value.
        /// </summary>
        public static TimeSpan MinusOneMillisecond { get; } = TimeSpan.FromMilliseconds(-1);

        /// <summary>
        /// Gets the value of the current <see cref="TimeSpan"/> structure expressed in whole milliseconds.
        /// </summary>
        /// <param name="timespan">The <see cref="TimeSpan"/>.</param>
        /// <param name="roundToZero">Whether it is OK to round a value to zero.</param>
        /// <returns>The value of the <paramref name="timespan"/> structure expressed in whole milliseconds.</returns>
        /// <remarks>
        /// <para>If the rounded value is zero, but the non-rounded value is not zero (for instance,
        /// if <paramref name="timespan"/> is 0.666ms), the returned value depends on <paramref name="roundToZero"/>.
        /// If it is <c>true</c> then 0 is returned; otherwise, 1 (or -1) is returned, assuming that 0 might have some
        /// special meaning that should be avoided.</para>
        /// <para>If the value is negative, the returned value is -1.</para>
        /// </remarks>
        public static long RoundedMilliseconds(this TimeSpan timespan, bool roundToZero = true)
        {
            var milliseconds = timespan.TotalMilliseconds;

            if (milliseconds == 0) return 0;

            var rounded = (long) milliseconds;
            if (rounded != 0) return rounded > 0 ? rounded : -1;
            if (roundToZero) return 0;
            return milliseconds > 0 ? 1 : -1;
        }
    }
}
