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

namespace Hazelcast.Sql
{
    /// <summary>
    /// Represents Hazelcast SQL <c>TIMESTAMP_WITH_TIME_ZONE</c> type corresponding to <c>java.time.OffsetDateTime</c> in Java.
    /// </summary>
    public readonly struct HOffsetDateTime : IEquatable<HOffsetDateTime>
    {
        private static readonly Regex ParseRegex = new Regex(
            @"^(?'local'.+)(Z|(?'offset'(\+|\-)\d+:\d+))$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        public static readonly TimeSpan MaxOffset = TimeSpan.FromHours(18);
        public static readonly TimeSpan MinOffset = TimeSpan.FromHours(-18);

        public static readonly HOffsetDateTime Max = new HOffsetDateTime(HLocalDateTime.Max, MinOffset);
        public static readonly HOffsetDateTime Min = new HOffsetDateTime(HLocalDateTime.Min, MaxOffset);

        /// <summary>
        /// Local part represented as <see cref="HLocalDateTime"/>.
        /// </summary>
        public HLocalDateTime LocalDateTime { get; }

        /// <summary>
        /// Offset value.
        /// Ranges between <see cref="MinOffset"/> and <see cref="MaxOffset"/> inclusive.
        /// Precision is 1 second, smaller values will be rounded and lost during serialization.
        /// </summary>
        public TimeSpan Offset { get; }

        public HOffsetDateTime(HLocalDateTime localDateTime, TimeSpan offset = default)
        {
            if (offset < MinOffset || offset > MaxOffset)
                throw new ArgumentOutOfRangeException(nameof(offset), $@"Offset must be between {MinOffset:hh\:mm} and {MaxOffset:hh\:mm}.");

            LocalDateTime = localDateTime;
            Offset = offset;
        }

        public HOffsetDateTime(DateTime localDateTime, TimeSpan offset = default) :
            this(new HLocalDateTime(localDateTime), offset)
        { }

        public HOffsetDateTime(DateTimeOffset dateTimeOffset) :
            this(new HLocalDateTime(dateTimeOffset.DateTime), dateTimeOffset.Offset)
        { }

        public bool TryToDateTimeOffset(out DateTimeOffset dateTimeOffset)
        {
            dateTimeOffset = default;

            if (!LocalDateTime.TryToDateTime(out var dateTime))
                return false;

            dateTimeOffset = new DateTimeOffset(dateTime, Offset);
            return true;
        }

        public DateTimeOffset ToDateTimeOffset() => new DateTimeOffset(LocalDateTime.ToDateTime().Ticks, Offset);

        public static explicit operator DateTimeOffset(HOffsetDateTime offsetDateTime) => offsetDateTime.ToDateTimeOffset();
        public static explicit operator HOffsetDateTime(DateTimeOffset dateTimeOffset) => new HOffsetDateTime(dateTimeOffset);

        public override string ToString()
        {
            return Offset switch
            {
                var s when s < TimeSpan.Zero => LocalDateTime + "-" + Offset.ToString(@"hh\:mm"),
                var s when s > TimeSpan.Zero => LocalDateTime + "+" + Offset.ToString(@"hh\:mm"),
                _ => LocalDateTime + "Z"
            };
        }

        public static bool TryParse(string s, out HOffsetDateTime offsetDateTime)
        {
            offsetDateTime = default;

            var offset = TimeSpan.Zero;
            var match = ParseRegex.Match(s);
            if (!match.Success ||
                !HLocalDateTime.TryParse(match.Groups["local"].Value, out var localDateTime) ||
                (match.Groups["offset"].Success && !TimeSpan.TryParse(match.Groups["offset"].Value.TrimStart('+'), out offset)))
            {
                return false;
            }

            offsetDateTime = new HOffsetDateTime(localDateTime, offset);
            return true;
        }

        public static HOffsetDateTime Parse(string s)
        {
            return TryParse(s, out var offsetDateTime)
                ? offsetDateTime
                : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HOffsetDateTime)}.");
        }

        #region Equality members

        public bool Equals(HOffsetDateTime other)
        {
            return LocalDateTime.Equals(other.LocalDateTime) && Offset.Equals(other.Offset);
        }

        public override bool Equals(object obj)
        {
            return obj is HOffsetDateTime other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LocalDateTime, Offset);
        }

        public static bool operator ==(HOffsetDateTime left, HOffsetDateTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HOffsetDateTime left, HOffsetDateTime right)
        {
            return !left.Equals(right);
        }

        #endregion

    }
}
