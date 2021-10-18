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
using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the system clock.
    /// </summary>
    internal static class Clock
    {
        // unix epoch is 00:00:00 UTC on January 1st, 1970
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static int _initialized;
        private static long _offsetMilliseconds;
        private static IClockSource _clockSource = new SystemClock();

        /// <summary>
        /// (internal for tests only)
        /// Overrides the default static implementation with a temporary alternate implementation.
        /// </summary>
        /// <param name="clockSource">The alternate <see cref="IClockSource"/> implementation.</param>
        /// <returns>An object that must be disposed in order to restore the original static implementation.</returns>
        internal static IDisposable Override(IClockSource clockSource)
        {
            var saved = _clockSource;
            _clockSource = clockSource;
            return new DisposeAction(() => _clockSource = saved);
        }

        /// <summary>
        /// (for tests only) Gets or sets the clock offset.
        /// </summary>
        internal static long Offset
        {
            get => _offsetMilliseconds;
            set => _offsetMilliseconds = value;
        }

        /// <summary>
        /// Initializes the clock.
        /// </summary>
        /// <param name="options">Clock options.</param>
        internal static void Initialize(ClockOptions options)
        {
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 1)
            {
                //throw new InvalidOperationException("The clock has already been initialized.");
                if (options.OffsetMilliseconds != _offsetMilliseconds)
                    throw new InvalidOperationException("The clock has already been initialized with a different offset.");
                return;
            }
            _offsetMilliseconds = options.OffsetMilliseconds;
        }

        /// <summary>
        /// (internal for tests only)
        /// Resets the clock.
        /// </summary>
        internal static void Reset()
        {
            _offsetMilliseconds = 0;
            _initialized = 0;
        }

        /// <summary>
        /// Gets the epoch time in milliseconds, i.e. the number of milliseconds that have elapsed since the epoch (00:00:00 UTC on January 1st, 1970).
        /// </summary>
        /// <remarks>The epoch time in milliseconds.</remarks>
        public static long Milliseconds
            => ToEpoch(DateTime.UtcNow);

        public static DateTime Now
            => _clockSource.Now;

        /// <summary>
        /// Gets a number (-1) representing 'never'.
        /// </summary>
        public static long Never
            => -1L;

        /// <summary>
        /// Gets the origin date and time.
        /// </summary>
        public static DateTime Origin
            => Jan1St1970;

        /// <summary>
        /// Gets the UTC <see cref="DateTime"/> corresponding to an epoch time.
        /// </summary>
        /// <param name="epochMilliseconds">The epoch time in milliseconds.</param>
        /// <returns>The corresponding UTC <see cref="DateTime"/>.</returns>
        public static DateTime ToDateTime(long epochMilliseconds)
            => Jan1St1970.AddMilliseconds(epochMilliseconds - _offsetMilliseconds);

        /// <summary>
        /// Gets the epoch time in milliseconds corresponding to an UTC <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/>.</param>
        /// <returns>The epoch time in milliseconds corresponding to the <see cref="DateTime"/>.</returns>
        public static long ToEpoch(DateTime dateTime)
            => (long) (dateTime - Jan1St1970).TotalMilliseconds + _offsetMilliseconds;
    }
}
