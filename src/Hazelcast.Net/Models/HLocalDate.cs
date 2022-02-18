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
using System.Text.RegularExpressions;

namespace Hazelcast.Models
{
    // FIXME - this type needs to be cleaned up (see HBigDecimal)

    /// <summary>
    /// Represents Hazelcast SQL <c>DATE</c> type corresponding to <c>java.time.LocalDate</c> in Java.
    /// </summary>
    public readonly struct HLocalDate : IEquatable<HLocalDate>
    {
        private static readonly Regex ParseRegex = new Regex(
            @"^(?'year'-?\d+)-(?'month'\d+)-(?'day'\d+)$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        public const int MaxYear = 999_999_999;
        public const int MinYear = -999_999_999;

        public static readonly HLocalDate Max = new HLocalDate(MinYear, 12, 31);
        public static readonly HLocalDate Min = new HLocalDate(MaxYear, 1, 1);

        /// <summary>
        /// Year value. Ranges between <see cref="MinYear"/> and <see cref="MaxYear"/> inclusive.
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Month value. Ranges between 1 and 12 inclusive.
        /// </summary>
        public byte Month { get; }

        /// <summary>
        /// Day value. Ranges between 1 and max number of days in given <see cref="Year"/> and <see cref="Month"/> inclusive.
        /// </summary>
        public byte Day { get; }

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

        public HLocalDate(DateTime dateTime)
        {
            Year = dateTime.Year;
            Month = (byte)dateTime.Month;
            Day = (byte)dateTime.Day;
        }

        public DateTime ToDateTime() => new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Local);

        public bool TryToDateTime(out DateTime dateTime)
        {
            dateTime = default;

            if (Year < 1 || Year > 9999)
                return false;

            dateTime = ToDateTime();
            return true;
        }

        // FIXME - add conversion for DateOnly in .NET 6
        public static explicit operator DateTime(HLocalDate localDate) => localDate.ToDateTime();
        public static explicit operator HLocalDate(DateTime dateTime) => new HLocalDate(dateTime);

        public override string ToString() => $"{Year:D4}-{Month:D2}-{Day:D2}";

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

        public static HLocalDate Parse(string s)
        {
            return TryParse(s, out var localDate)
                ? localDate
                : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HLocalDate)}.");
        }

        #region Equality members

        public bool Equals(HLocalDate other)
        {
            return Year == other.Year && Month == other.Month && Day == other.Day;
        }

        public override bool Equals(object obj)
        {
            return obj is HLocalDate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Year, Month, Day);
        }

        public static bool operator ==(HLocalDate left, HLocalDate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HLocalDate left, HLocalDate right)
        {
            return !left.Equals(right);
        }

        #endregion

    }
}
