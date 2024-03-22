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
using System.Text.RegularExpressions;

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents an Hazelcast <c>DATE</c> primitive type value.
    /// </summary>
    /// <remarks>
    /// <para>The <c>DATE</c> primitive type consists in a day, month and year, with
    /// year being comprised between <see cref="MinYear"/> and <see cref="MaxYear"/>
    /// inclusive.</para>
    /// </remarks>
    public readonly struct HLocalDate : IEquatable<HLocalDate>
    {
        private static readonly Regex ParseRegex = new(
            @"^(?'year'-?\d+)-(?'month'\d+)-(?'day'\d+)$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );
        
        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalDate"/> struct.
        /// </summary>
        /// <param name="year">The year value.</param>
        /// <param name="month">The month value.</param>
        /// <param name="day">The day value.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="year"/>, <paramref name="month"/> and/or <paramref name="day"/> are out-of-range.</exception>
        public HLocalDate(int year, byte month, byte day)
        {
            if (year < MinYear || year > MaxYear)
                throw new ArgumentOutOfRangeException(nameof(year), $@"Year must be between {MinYear} and {MaxYear}.");
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), @"Month must be between 1 and 12.");
            if (day < 1 || day > 31 || day > DateTime.DaysInMonth(year % 4 + 4, month))
                throw new ArgumentOutOfRangeException(nameof(day), @"Day must be between 1 and number of days in provided month.");

            Year = year;
            Month = month;
            Day = day;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalDate"/> struct.
        /// </summary>
        /// <param name="dateTime">The date.</param>
        public HLocalDate(DateTime dateTime)
        {
            Year = dateTime.Year;
            Month = (byte)dateTime.Month;
            Day = (byte)dateTime.Day;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="HLocalDate"/> struct.
        /// </summary>
        /// <param name="dateOnly">The date.</param>
        public HLocalDate(DateOnly dateOnly)
            : this(dateOnly.Year, (byte) dateOnly.Month, (byte) dateOnly.Day)
        { }
#endif

        /// <summary>
        /// Gets the largest possible value of the year value.
        /// </summary>
        public const int MaxYear = 999_999_999;
        
        /// <summary>
        /// Gets the smallest possible value of the year value.
        /// </summary>
        public const int MinYear = -999_999_999;

        /// <summary>
        /// Gets the largest possible value of a <see cref="HLocalDate"/>.
        /// </summary>
        public static readonly HLocalDate Max = new(MinYear, 12, 31);
        
        /// <summary>
        /// Gets the smallest possible value of a <see cref="HLocalDate"/>.
        /// </summary>
        public static readonly HLocalDate Min = new(MaxYear, 1, 1);

        /// <summary>
        /// Gets the year value.
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Gets the month value.
        /// </summary>
        public byte Month { get; }

        /// <summary>
        /// Gets the day value.
        /// </summary>
        public byte Day { get; }
        
        /// <summary>
        /// Converts the value of this <see cref="HLocalDate"/> to its <see cref="DateTime"/> equivalent.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> representation of this instance.</returns>
        public DateTime ToDateTime() => new(Year, Month, Day, 0, 0, 0, DateTimeKind.Local);

        /// <summary>
        /// Converts the value of this <see cref="HLocalDate"/> to its <see cref="DateTime"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="dateTime">When this method returns, contains the <see cref="DateTime"/> equivalent
        /// to the value represented by this instance, if the conversion succeeded, or the default value if the conversion failed.</param>
        /// <returns><c>true</c> if the value represented by this instance was converted successfully; otherwise <c>false</c>.</returns>
        public bool TryToDateTime(out DateTime dateTime)
        {
            dateTime = default;

            if (Year < 1 || Year > 9999)
                return false;

            dateTime = ToDateTime();
            return true;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Converts the value of this <see cref="HLocalDate"/> to its <see cref="DateOnly"/> equivalent.
        /// </summary>
        /// <returns>The <see cref="DateOnly"/> representation of this instance.</returns>
        public DateOnly ToDateOnly() => new(Year, Month, Day);

        /// <summary>
        /// Converts the value of this <see cref="HLocalDate"/> to its <see cref="DateOnly"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="dateOnly">When this method returns, contains the <see cref="DateOnly"/> equivalent
        /// to the value represented by this instance, if the conversion succeeded, or the default value if the conversion failed.</param>
        /// <returns><c>true</c> if the value represented by this instance was converted successfully; otherwise <c>false</c>.</returns>
        public bool TryToDateOnly(out DateOnly dateOnly)
        {
            dateOnly = default;

            if (Year is < 1 or > 9999)
                return false;

            dateOnly = ToDateOnly();
            return true;
        }
#endif

        /// <summary>
        /// Implements the <see cref="HLocalDate"/> to <see cref="DateTime"/> conversion.
        /// </summary>
        public static explicit operator DateTime(HLocalDate localDate) => localDate.ToDateTime();
        
        /// <summary>
        /// Implements the <see cref="DateTime"/> to <see cref="HLocalDate"/> conversion.
        /// </summary>
        public static explicit operator HLocalDate(DateTime dateTime) => new(dateTime);

#if NET6_0_OR_GREATER
        /// <summary>
        /// Implements the <see cref="HLocalDate"/> to <see cref="DateOnly"/> conversion.
        /// </summary>
        public static explicit operator DateOnly(HLocalDate localDate) => localDate.ToDateOnly();
        
        /// <summary>
        /// Implements the <see cref="DateOnly"/> to <see cref="HLocalDate"/> conversion.
        /// </summary>
        public static explicit operator HLocalDate(DateOnly dateOnly) => new(dateOnly);
#endif
        
        /// <inheritdoc />
        public override string ToString() => $"{Year:D4}-{Month:D2}-{Day:D2}";

        /// <summary>
        /// Converts the string representation of a date to its <see cref="HLocalDate"/> representation.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="s">A string containing a date to convert.</param>
        /// <param name="localDate">When this method returns, contains the <see cref="HLocalDate"/> equivalent of
        /// the date in <paramref name="s"/>, if the conversion succeeded, or zero if the conversion failed.</param>
        /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string s, out HLocalDate localDate)
        {
            localDate = default;

            var match = ParseRegex.Match(s);
            if (!match.Success ||
                !int.TryParse(match.Groups["year"].Value, out var year) ||
                !byte.TryParse(match.Groups["month"].Value, out var month) ||
                !byte.TryParse(match.Groups["day"].Value, out var day)
            )
            {
                return false;
            }

            localDate = new HLocalDate(year, month, day);
            return true;
        }

        /// <summary>
        /// Converts the string representation of a date to its <see cref="HLocalDate"/> representation.
        /// </summary>
        /// <param name="s">A string containing a date to convert.</param>
        /// <returns>A <see cref="HLocalDate"/> equivalent to the date contained in <paramref name="s"/>.</returns>
        /// <exception cref="FormatException">The <paramref name="s"/> string cannot be parsed.</exception>
        public static HLocalDate Parse(string s)
        {
            return TryParse(s, out var localDate)
                ? localDate
                : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HLocalDate)}.");
        }

        #region Equality members

        /// <inheritdoc />
        public bool Equals(HLocalDate other)
            => Year == other.Year && Month == other.Month && Day == other.Day;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is HLocalDate other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Year, Month, Day);

        /// <summary>Implements the == operator.</summary>
        public static bool operator ==(HLocalDate left, HLocalDate right) => left.Equals(right);

        /// <summary>Implements the != operator.</summary>
        public static bool operator !=(HLocalDate left, HLocalDate right) => !left.Equals(right);

        #endregion
    }
}
