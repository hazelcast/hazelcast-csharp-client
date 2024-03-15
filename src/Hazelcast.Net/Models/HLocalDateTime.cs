// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents an Hazelcast SQL <c>TIMESTAMP</c> primitive type value.
    /// </summary>
    /// <remarks>
    /// <para>The <c>TIMESTAMP</c> primitive type consists of a <c>DATE</c> <see cref="Date"/>
    /// and a <c>TIME</c> <see cref="Time"/>.</para>
    /// </remarks>
    public readonly struct HLocalDateTime : IEquatable<HLocalDateTime>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalDateTime"/> struct.
        /// </summary>
        /// <param name="date">The date value.</param>
        /// <param name="time">The time value.</param>
        public HLocalDateTime(HLocalDate date, HLocalTime time)
        {
            Date = date;
            Time = time;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalDateTime"/> struct.
        /// </summary>
        /// <param name="year">The year value.</param>
        /// <param name="month">The month value.</param>
        /// <param name="day">The day value.</param>
        /// <param name="hour">The hour value.</param>
        /// <param name="minute">The minute value.</param>
        /// <param name="second">The second value.</param>
        /// <param name="nanosecond">The nanosecond value.</param>
        public HLocalDateTime(int year, byte month, byte day, byte hour, byte minute, byte second, int nanosecond)
        {
            Date = new HLocalDate(year, month, day);
            Time = new HLocalTime(hour, minute, second, nanosecond);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalDateTime"/> struct.
        /// </summary>
        /// <param name="year">The year value.</param>
        /// <param name="month">The month value.</param>
        /// <param name="day">The day value.</param>
        public HLocalDateTime(int year, byte month, byte day)
            : this(new HLocalDate(year, month, day), default)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalDateTime"/> struct.
        /// </summary>
        /// <param name="dateTime">The date and time value.</param>
        public HLocalDateTime(DateTime dateTime)
            : this(new HLocalDate(dateTime), new HLocalTime(dateTime))
        { }

        /// <summary>
        /// Gets the smallest possible value of a <see cref="HLocalDateTime"/>.
        /// </summary>
        public static readonly HLocalDateTime Min = new HLocalDateTime(HLocalDate.Min, HLocalTime.Min);
        
        /// <summary>
        /// Gets the largest possible value of a <see cref="HLocalDateTime"/>.
        /// </summary>
        public static readonly HLocalDateTime Max = new HLocalDateTime(HLocalDate.Max, HLocalTime.Max);

        /// <summary>
        /// Gets the date value.
        /// </summary>
        public HLocalDate Date { get; }

        /// <summary>
        /// Gets the time value.
        /// </summary>
        public HLocalTime Time { get; }

        /// <summary>
        /// Gets the year value.
        /// </summary>
        public int Year => Date.Year;

        /// <summary>
        /// Gets the month value.
        /// </summary>
        public byte Month => Date.Month;

        /// <summary>
        /// Gets the day value.
        /// </summary>
        public byte Day => Date.Day;

        /// <summary>
        /// Gets the hour value.
        /// </summary>
        public byte Hour => Time.Hour;

        /// <summary>
        /// Gets the minute value.
        /// </summary>
        public byte Minute => Time.Minute;

        /// <summary>
        /// Gets the second value.
        /// </summary>
        public byte Second => Time.Second;

        /// <summary>
        /// Gets the nanosecond value.
        /// </summary>
        public int Nanosecond => Time.Nanosecond;

        /// <summary>
        /// Offsets the value of this <see cref="HLocalDateTime"/> as an <see cref="HOffsetDateTime"/>.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The <see cref="HOffsetDateTime"/>.</returns>
        internal HOffsetDateTime Offset(TimeSpan offset) => new HOffsetDateTime(this, offset);

        /// <summary>
        /// Offsets the value of this <see cref="HLocalDateTime"/> as an <see cref="HOffsetDateTime"/>.
        /// </summary>
        /// <param name="offsetSeconds">The offset.</param>
        /// <returns>The <see cref="HOffsetDateTime"/>.</returns>
        internal HOffsetDateTime Offset(int offsetSeconds = 0) => new HOffsetDateTime(this, offsetSeconds);
        
        /// <summary>
        /// Converts the value of this <see cref="HLocalDateTime"/> to its <see cref="DateTime"/> equivalent.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> representation of this instance.</returns>
        public DateTime ToDateTime() => Date.ToDateTime() + Time.ToTimeSpan();

        /// <summary>
        /// Converts the value of this <see cref="HLocalDateTime"/> to its <see cref="DateTime"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="dateTime">When this method returns, contains the <see cref="DateTime"/> equivalent
        /// to the value represented by this instance, if the conversion succeeded, or the default value if the conversion failed.</param>
        /// <returns><c>true</c> if the value represented by this instance was converted successfully; otherwise <c>false</c>.</returns>
        public bool TryToDateTime(out DateTime dateTime)
        {
            if (!Date.TryToDateTime(out dateTime))
                return false;

            dateTime += Time.ToTimeSpan();
            return true;
        }

        /// <summary>
        /// Implements the <see cref="HLocalDateTime"/> to <see cref="DateTime"/> conversion.
        /// </summary>
        public static explicit operator DateTime(HLocalDateTime localDateTime) => localDateTime.ToDateTime();
        
        /// <summary>
        /// Implements the <see cref="DateTime"/> to <see cref="HLocalDateTime"/> conversion.
        /// </summary>
        public static explicit operator HLocalDateTime(DateTime dateTime) => new HLocalDateTime(dateTime);

        /// <inheritdoc />
        public override string ToString() => Date + "T" + Time;

        /// <summary>
        /// Converts the string representation of a date and time to its <see cref="HLocalDateTime"/> representation.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="s">A string containing a date and time to convert.</param>
        /// <param name="localDateTime">When this method returns, contains the <see cref="HLocalDateTime"/> equivalent of
        /// the date and time in <paramref name="s"/>, if the conversion succeeded, or zero if the conversion failed.</param>
        /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string? s, out HLocalDateTime localDateTime)
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

        /// <summary>
        /// Converts the string representation of a date and time to its <see cref="HLocalDateTime"/> representation.
        /// </summary>
        /// <param name="s">A string containing a date and time to convert.</param>
        /// <returns>A <see cref="HLocalDateTime"/> equivalent to the date and time time contained in <paramref name="s"/>.</returns>
        /// <exception cref="FormatException">The <paramref name="s"/> string cannot be parsed.</exception>
        public static HLocalDateTime Parse(string s)
        {
            return TryParse(s, out var localDateTime)
                ? localDateTime
                : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HLocalDateTime)}.");
        }

        #region Equality members

        /// <inheritdoc />
        public bool Equals(HLocalDateTime other)
            => Date.Equals(other.Date) && Time.Equals(other.Time);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is HLocalDateTime other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Date, Time);

        /// <summary>Implements the == operator.</summary>
        public static bool operator ==(HLocalDateTime left, HLocalDateTime right) => left.Equals(right);

        /// <summary>Implements the != operator.</summary>
        public static bool operator !=(HLocalDateTime left, HLocalDateTime right) => !left.Equals(right);

        #endregion
    }
}
