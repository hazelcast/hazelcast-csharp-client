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

using System;
using System.Text.RegularExpressions;

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents Hazelcast SQL <c>TIME</c> type corresponding to <c>java.time.LocalTime</c> in Java.
    /// </summary>
    public readonly struct HLocalTime : IEquatable<HLocalTime>
    {
        private static readonly Regex ParseRegex = new Regex(
            @"^(?'hour'\d+):(?'minute'\d+):(?'second'\d+)(\.(?'nano'\d+))?$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        public static readonly HLocalTime Min = new HLocalTime(0, 0, 0, 0);
        public static readonly HLocalTime Max = new HLocalTime(23, 59, 59, 999_999_999);
        public static readonly HLocalTime Midnight = Min;
        public static readonly HLocalTime Noon = new HLocalTime(12, 0, 0, 0);

        /// <summary>
        /// Hour value. Ranges between 0 and 23 inclusive.
        /// </summary>
        public byte Hour { get; }

        /// <summary>
        /// Minute value. Ranges between 0 and 59 inclusive.
        /// </summary>
        public byte Minute { get; }

        /// <summary>
        /// Second value. Ranges between 0 and 59 inclusive.
        /// </summary>
        public byte Second { get; }

        /// <summary>
        /// Nanosecond value. Ranges between 0 and 999999999 inclusive.
        /// </summary>
        public int Nanosecond { get; }

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

        public HLocalTime(DateTime dateTime)
        {
            Hour = (byte)dateTime.Hour;
            Minute = (byte)dateTime.Minute;
            Second = (byte)dateTime.Second;
            Nanosecond = dateTime.Millisecond * 1000;
        }

        public HLocalTime(TimeSpan timeSpan)
        {
            Hour = (byte)timeSpan.Hours;
            Minute = (byte)timeSpan.Minutes;
            Second = (byte)timeSpan.Seconds;
            Nanosecond = timeSpan.Milliseconds * 1000;
        }

        public TimeSpan ToTimeSpan() => new TimeSpan(0, Hour, Minute, Second, Nanosecond / 1000);

        public static explicit operator TimeSpan(HLocalTime localTime) => localTime.ToTimeSpan();
        public static explicit operator HLocalTime(TimeSpan timeSpan) => new HLocalTime(timeSpan);
        public static explicit operator HLocalTime(DateTime dateTime) => new HLocalTime(dateTime);

        public override string ToString() => Nanosecond == 0
            ? $"{Hour:D2}:{Minute:D2}:{Second:D2}"
            : $"{Hour:D2}:{Minute:D2}:{Second:D2}.{Nanosecond:D9}";

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

        public static HLocalTime Parse(string s)
        {
            return TryParse(s, out var localTime)
                ? localTime
                : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HLocalTime)}.");
        }

        #region Equality members

        public bool Equals(HLocalTime other)
        {
            return Hour == other.Hour && Minute == other.Minute && Second == other.Second && Nanosecond == other.Nanosecond;
        }

        public override bool Equals(object obj)
        {
            return obj is HLocalTime other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hour, Minute, Second, Nanosecond);
        }

        public static bool operator ==(HLocalTime left, HLocalTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HLocalTime left, HLocalTime right)
        {
            return !left.Equals(right);
        }

        #endregion

    }
}
