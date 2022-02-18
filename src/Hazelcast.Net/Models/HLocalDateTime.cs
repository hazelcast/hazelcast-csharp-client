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

namespace Hazelcast.Models
{
    // FIXME - this type needs to be cleaned up (see HBigDecimal)

    /// <summary>
    /// Represents Hazelcast SQL <c>TIMESTAMP</c> type corresponding to <c>java.time.LocalDateTime</c> in Java.
    /// </summary>
    public readonly struct HLocalDateTime : IEquatable<HLocalDateTime>
    {
        public static readonly HLocalDateTime Min = new HLocalDateTime(HLocalDate.Min, HLocalTime.Min);
        public static readonly HLocalDateTime Max = new HLocalDateTime(HLocalDate.Max, HLocalTime.Max);

        /// <summary>
        /// Date part represented as <see cref="HLocalDate"/>.
        /// </summary>
        public HLocalDate Date { get; }

        /// <summary>
        /// Time part represented as <see cref="HLocalTime"/>.
        /// </summary>
        public HLocalTime Time { get; }

        /// <inheritdoc cref="HLocalDate.Year"/>
        public int Year => Date.Year;

        /// <inheritdoc cref="HLocalDate.Month"/>
        public byte Month => Date.Month;

        /// <inheritdoc cref="HLocalDate.Day"/>
        public byte Day => Date.Day;

        /// <inheritdoc cref="HLocalTime.Hour"/>
        public byte Hour => Time.Hour;

        /// <inheritdoc cref="HLocalTime.Minute"/>
        public byte Minute => Time.Minute;

        /// <inheritdoc cref="HLocalTime.Second"/>
        public byte Second => Time.Second;

        /// <inheritdoc cref="HLocalTime.Nanosecond"/>
        public int Nanosecond => Time.Nanosecond;

        public HLocalDateTime(HLocalDate date, HLocalTime time)
        {
            Date = date;
            Time = time;
        }

        public HLocalDateTime(int year, byte month, byte day, byte hour, byte minute, byte second, int nanosecond)
        {
            Date = new HLocalDate(year, month, day);
            Time = new HLocalTime(hour, minute, second, nanosecond);
        }

        public HLocalDateTime(int year, byte month, byte day)
        {
            Date = new HLocalDate(year, month, day);
            Time = default;
        }

        public HLocalDateTime(DateTime dateTime)
        {
            Date = new HLocalDate(dateTime);
            Time = new HLocalTime(dateTime);
        }

        public DateTime ToDateTime() => Date.ToDateTime() + Time.ToTimeSpan();

        public bool TryToDateTime(out DateTime dateTime)
        {
            if (!Date.TryToDateTime(out dateTime))
                return false;

            dateTime += Time.ToTimeSpan();
            return true;
        }

        public static explicit operator DateTime(HLocalDateTime localDateTime) => localDateTime.ToDateTime();
        public static explicit operator HLocalDateTime(DateTime dateTime) => new HLocalDateTime(dateTime);

        public override string ToString() => Date + "T" + Time;

        public static bool TryParse(string s, out HLocalDateTime localDateTime)
        {
            localDateTime = default;
            if (s == null) 
                return false;

            var parts = s.Split(new[] { 'T', 't' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return false;

            if (!HLocalDate.TryParse(parts[0], out var localDate) || !HLocalTime.TryParse(parts[1], out var localTime))
                return false;

            localDateTime = new HLocalDateTime(localDate, localTime);
            return true;
        }

        public static HLocalDateTime Parse(string s)
        {
            return TryParse(s, out var localDateTime)
                ? localDateTime
                : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HLocalDateTime)}.");
        }

        #region Equality members

        public bool Equals(HLocalDateTime other)
        {
            return Date.Equals(other.Date) && Time.Equals(other.Time);
        }

        public override bool Equals(object obj)
        {
            return obj is HLocalDateTime other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Date, Time);
        }

        public static bool operator ==(HLocalDateTime left, HLocalDateTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HLocalDateTime left, HLocalDateTime right)
        {
            return !left.Equals(right);
        }

        #endregion

    }
}
