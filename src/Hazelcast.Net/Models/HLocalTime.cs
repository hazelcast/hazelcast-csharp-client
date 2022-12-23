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

#nullable  enable

using System;
using System.Text.RegularExpressions;

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents an Hazelcast <c>TIME</c> primitive type value.
    /// </summary>
    /// <remarks>
    /// <para>The <c>TIME</c> primitive type consists in hours, minutes, seconds
    /// and nanoseconds, with nanosecond precision and one-day range.</para>
    /// </remarks>
    public readonly struct HLocalTime : IEquatable<HLocalTime>
    {
        private static readonly Regex ParseRegex = new Regex(
            @"^(?'hour'\d+):(?'minute'\d+):(?'second'\d+)(\.(?'nano'\d+))?$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalTime"/> struct.
        /// </summary>
        /// <param name="hour">The hour value.</param>
        /// <param name="minute">The minute value.</param>
        /// <param name="second">The second value.</param>
        /// <param name="nanosecond">The nanosecond value.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="hour"/>, <paramref name="minute"/>, <paramref name="second"/>,
        /// and/or <paramref name="nanosecond"/> is out of range.</exception>
        public HLocalTime(byte hour, byte minute, byte second, int nanosecond)
        {
            if (hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour), @"Hour must be between 0 and 23.");
            if (minute > 59)
                throw new ArgumentOutOfRangeException(nameof(minute), @"Minute must be between 0 and 59.");
            if (second > 59)
                throw new ArgumentOutOfRangeException(nameof(second), @"Second must be between 0 and 59.");
            if (nanosecond < 0 || nanosecond > 999_999_999)
                throw new ArgumentOutOfRangeException(nameof(nanosecond), @"Nanosecond must be between 0 and 999999999.");

            Hour = hour;
            Minute = minute;
            Second = second;
            Nanosecond = nanosecond;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalTime"/> struct.
        /// </summary>
        /// <param name="dateTime">The time.</param>
        /// <remarks>
        /// <para>The date part of the <paramref name="dateTime"/> is ignored.</para>
        /// </remarks>
        public HLocalTime(DateTime dateTime)
            : this((byte) dateTime.Hour, (byte) dateTime.Minute, (byte) dateTime.Second, dateTime.Millisecond * 1000)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalTime"/> struct.
        /// </summary>
        /// <param name="timeSpan">The time.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeSpan"/> is out of range.</exception>

        public HLocalTime(TimeSpan timeSpan)
            : this((byte) timeSpan.Hours, (byte) timeSpan.Minutes, (byte) timeSpan.Seconds, timeSpan.Milliseconds * 1000)
        { }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalTime"/> struct.
        /// </summary>
        /// <param name="timeOnly">The time.</param>
        public HLocalTime(TimeOnly timeOnly)
            : this((byte) timeOnly.Hour, (byte) timeOnly.Minute, (byte) timeOnly.Second, timeOnly.Millisecond * 1000)
        { }
#endif

        /// <summary>
        /// Gets the smallest possible value of a <see cref="HLocalTime"/>.
        /// </summary>
        public static readonly HLocalTime Min = new(0, 0, 0, 0);

        /// <summary>
        /// Gets the largest possible value of a <see cref="HLocalTime"/>.
        /// </summary>
        public static readonly HLocalTime Max = new(23, 59, 59, 999_999_999);
        
        /// <summary>
        /// Gets a value that represents midnight, i.e. "00:00:00".
        /// </summary>
        public static readonly HLocalTime Midnight = Min;
        
        /// <summary>
        /// Gets a value that represents noon, i.e. "12:00:00".
        /// </summary>
        public static readonly HLocalTime Noon = new(12, 0, 0, 0);

        /// <summary>
        /// Gets the hour value.
        /// </summary>
        public byte Hour { get; }

        /// <summary>
        /// Gets the minute value.
        /// </summary>
        public byte Minute { get; }

        /// <summary>
        /// Gets the second value.
        /// </summary>
        public byte Second { get; }

        /// <summary>
        /// Gets the nanosecond value.
        /// </summary>
        public int Nanosecond { get; }
        
        /// <summary>
        /// Converts the value of this <see cref="HLocalTime"/> to its <see cref="TimeSpan"/> equivalent.
        /// </summary>
        /// <returns>The <see cref="TimeSpan"/> representation of this instance.</returns>
        public TimeSpan ToTimeSpan() => new(0, Hour, Minute, Second, Nanosecond / 1000);

#if NET6_0_OR_GREATER
        public TimeOnly ToTimeOnly() => new TimeOnly(Hour, Minute, Second, Nanosecond / 1000);
#endif

        /// <summary>
        /// Implements the <see cref="HLocalTime"/> to <see cref="TimeSpan"/> conversion.
        /// </summary>
        public static explicit operator TimeSpan(HLocalTime localTime) => localTime.ToTimeSpan();
        
        /// <summary>
        /// Implements the <see cref="TimeSpan"/> to <see cref="HLocalTime"/> conversion.
        /// </summary>
        public static explicit operator HLocalTime(TimeSpan timeSpan) => new(timeSpan);
        
        /// <summary>
        /// Implements the <see cref="DateTime"/> to <see cref="HLocalTime"/> conversion.
        /// </summary>
        public static explicit operator HLocalTime(DateTime dateTime) => new(dateTime);

#if NET6_0_OR_GREATER
        /// <summary>
        /// Implements the <see cref="HLocalTime"/> to <see cref="TimeOnly"/> conversion.
        /// </summary>
        public static explicit operator TimeOnly(HLocalTime localTime) => localTime.ToTimeOnly();

        /// <summary>
        /// Implements the <see cref="TimeOnly"/> to <see cref="HLocalTime"/> conversion.
        /// </summary>
        public static explicit operator HLocalTime(TimeOnly timeOnly) => new HLocalTime(timeOnly);
#endif

        /// <inheritdoc />
        public override string ToString() => Nanosecond == 0
            ? $"{Hour:D2}:{Minute:D2}:{Second:D2}"
            : $"{Hour:D2}:{Minute:D2}:{Second:D2}.{Nanosecond:D9}";

        /// <summary>
        /// Converts the string representation of a time to its <see cref="HLocalTime"/> representation.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="s">A string containing a time to convert.</param>
        /// <param name="localTime">When this method returns, contains the <see cref="HLocalTime"/> equivalent of
        /// the time in <paramref name="s"/>, if the conversion succeeded, or zero if the conversion failed.</param>
        /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string s, out HLocalTime localTime)
        {
            localTime = default;

            var nano = 0;
            var match = ParseRegex.Match(s);
            if (!match.Success ||
                !byte.TryParse(match.Groups["hour"].Value, out var hour) ||
                !byte.TryParse(match.Groups["minute"].Value, out var minute) ||
                !byte.TryParse(match.Groups["second"].Value, out var second) ||
                (match.Groups["nano"].Success && !int.TryParse(match.Groups["nano"].Value, out nano))
            )
            {
                return false;
            }

            localTime = new HLocalTime(hour, minute, second, nano);
            return true;
        }

        /// <summary>
        /// Converts the string representation of a time to its <see cref="HLocalTime"/> representation.
        /// </summary>
        /// <param name="s">A string containing a time to convert.</param>
        /// <returns>A <see cref="HLocalTime"/> equivalent to the time contained in <paramref name="s"/>.</returns>
        /// <exception cref="FormatException">The <paramref name="s"/> string cannot be parsed.</exception>
        public static HLocalTime Parse(string s)
        {
            return TryParse(s, out var localTime)
                ? localTime
                : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HLocalTime)}.");
        }

        #region Equality members

        /// <inheritdoc />
        public bool Equals(HLocalTime other)
            =>  Hour == other.Hour && Minute == other.Minute && Second == other.Second && Nanosecond == other.Nanosecond;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is HLocalTime other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Hour, Minute, Second, Nanosecond);

        /// <summary>Implements the == operator.</summary>
        public static bool operator ==(HLocalTime left, HLocalTime right) => left.Equals(right);

        /// <summary>Implements the != operator.</summary>
        public static bool operator !=(HLocalTime left, HLocalTime right) => !left.Equals(right);

        #endregion
    }
}
