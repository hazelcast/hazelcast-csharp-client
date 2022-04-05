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

#nullable  enable

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents an Hazelcast <c>TIMESTAMP_WITH_TIME_ZONE</c> primitive type value.
    /// </summary>
    /// <remarks>
    /// <para>The <c>TIMESTAMP_WITH_TIME_ZONE</c> primitive type consists of a <c>TIMESTAMP</c>
    /// <see cref="LocalDateTime"/> and a timezone <see cref="Offset"/>.</para>
    /// <para>The offset ranges between <see cref="MinOffset"/> and <see cref="MaxOffset"/> inclusive.
    /// with a 1 second precision, smaller values being rounded and lost during serialization.</para>
    /// </remarks>
    public readonly struct HOffsetDateTime : IEquatable<HOffsetDateTime>
    {
        private static readonly Regex ParseRegex = new Regex(
            @"^(?'local'.+)(Z|(?'offset'(\+|\-)\d+:\d+))$",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        /// <summary>
        /// Initializes a new instance of the <see cref="HOffsetDateTime"/> struct.
        /// </summary>
        /// <param name="localDateTime">The local date and time part.</param>
        /// <param name="offset">The offset part.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is outside of the accepted range.</exception>
        public HOffsetDateTime(HLocalDateTime localDateTime, TimeSpan offset = default)
        {
            if (offset < MinOffset || offset > MaxOffset)
                throw new ArgumentOutOfRangeException(nameof(offset), $"Offset must be between {MinOffset:hh\\:mm} and {MaxOffset:hh\\:mm}.");

            LocalDateTime = localDateTime;
            Offset = offset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HOffsetDateTime"/> struct.
        /// </summary>
        /// <param name="localDateTime">The local date and time part.</param>
        /// <param name="offsetSeconds">The offset part.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offsetSeconds"/> is outside of the accepted range.</exception>
        public HOffsetDateTime(HLocalDateTime localDateTime, int offsetSeconds = 0)
            : this (localDateTime, TimeSpan.FromSeconds(offsetSeconds))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HOffsetDateTime"/> struct.
        /// </summary>
        /// <param name="localDateTime">The local date and time part.</param>
        /// <param name="offset">The offset part.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is outside of the accepted range.</exception>
        public HOffsetDateTime(DateTime localDateTime, TimeSpan offset = default) :
            this(new HLocalDateTime(localDateTime), offset)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HOffsetDateTime"/> struct.
        /// </summary>
        /// <param name="dateTimeOffset">The date, time and offset.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dateTimeOffset"/> offset is outside of the accepted range.</exception>
        public HOffsetDateTime(DateTimeOffset dateTimeOffset) :
            this(new HLocalDateTime(dateTimeOffset.DateTime), dateTimeOffset.Offset)
        { }
        
        /// <summary>
        /// Gets the largest possible value of the offset part.
        /// </summary>
        public static readonly TimeSpan MaxOffset = TimeSpan.FromHours(18);
        
        /// <summary>
        /// Gets the smallest possible value of the offset part.
        /// </summary>
        public static readonly TimeSpan MinOffset = TimeSpan.FromHours(-18);

        /// <summary>
        /// Gets the largest possible value of a <see cref="HOffsetDateTime"/>.
        /// </summary>
        public static readonly HOffsetDateTime Max = new HOffsetDateTime(HLocalDateTime.Max, MinOffset);
        
        /// <summary>
        /// Gets the smallest possible value of a <see cref="HOffsetDateTime"/>.
        /// </summary>
        public static readonly HOffsetDateTime Min = new HOffsetDateTime(HLocalDateTime.Min, MaxOffset);

        /// <summary>
        /// Gets the local date and time part.
        /// </summary>
        public HLocalDateTime LocalDateTime { get; }

        /// <summary>
        /// Gets the offset part.
        /// </summary>
        public TimeSpan Offset { get; }

        /// <summary>
        /// Converts the value of this instance to its <see cref="DateTimeOffset"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="dateTimeOffset">When this method returns, contains the <see cref="DateTimeOffset"/> equivalent
        /// to the value represented by this instance, if the conversion succeeded, or the default value if the conversion failed.</param>
        /// <returns><c>true</c> if the value represented by this instance was converted successfully; otherwise <c>false</c>.</returns>
        public bool TryToDateTimeOffset(out DateTimeOffset dateTimeOffset)
        {
            dateTimeOffset = default;

            if (!LocalDateTime.TryToDateTime(out var dateTime))
                return false;

            dateTimeOffset = new DateTimeOffset(dateTime, Offset);
            return true;
        }

        /// <summary>
        /// Converts the value of this instance to its <see cref="DateTimeOffset"/> equivalent.
        /// </summary>
        /// <returns>The <see cref="DateTimeOffset"/> representation of this instance.</returns>
        public DateTimeOffset ToDateTimeOffset() => new DateTimeOffset(LocalDateTime.ToDateTime().Ticks, Offset);

        /// <summary>
        /// Implements the <see cref="HOffsetDateTime"/> to <see cref="DateTimeOffset"/> conversion.
        /// </summary>
        public static explicit operator DateTimeOffset(HOffsetDateTime value) => value.ToDateTimeOffset();
        
        /// <summary>
        /// Implements the <see cref="DateTimeOffset"/> to <see cref="HOffsetDateTime"/> conversion.
        /// </summary>
        public static explicit operator HOffsetDateTime(DateTimeOffset value) => new HOffsetDateTime(value);

        /// <inheritdoc />
        public override string ToString()
        {
            return Offset switch
            {
                var s when s < TimeSpan.Zero => LocalDateTime + "-" + Offset.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                var s when s > TimeSpan.Zero => LocalDateTime + "+" + Offset.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                _ => LocalDateTime + "Z"
            };
        }

        /// <summary>
        /// Converts the string representation of a date and time with timezone to its <see cref="HOffsetDateTime"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="s">A string containing a date and time with timezone to convert.</param>
        /// <param name="offsetDateTime">When this method returns, contains the <see cref="HOffsetDateTime"/> equivalent
        /// to the date and time with timezone contained in <paramref name="s"/>, if the conversion succeeded, or the
        /// default value if the conversion failed.</param>
        /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Converts the string representation of a date and time with timezone to its <see cref="HOffsetDateTime"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a date and time with timezone to convert.</param>
        /// <returns>A <see cref="HOffsetDateTime"/> equivalent to the date and time with timezone contained in <paramref name="s"/>.</returns>
        /// <exception cref="FormatException">The <paramref name="s"/> string cannot be parsed.</exception>
        public static HOffsetDateTime Parse(string s)
        {
            return TryParse(s, out var offsetDateTime)
                ? offsetDateTime
                : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HOffsetDateTime)}.");
        }

        #region Equality members

        /// <inheritdoc />
        public bool Equals(HOffsetDateTime other)
            => LocalDateTime.Equals(other.LocalDateTime) && Offset.Equals(other.Offset);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is HOffsetDateTime other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(LocalDateTime, Offset);

        /// <summary>Implements the == operator.</summary>
        public static bool operator ==(HOffsetDateTime left, HOffsetDateTime right) => left.Equals(right);

        /// <summary>Implements the != operator.</summary>
        public static bool operator !=(HOffsetDateTime left, HOffsetDateTime right) => !left.Equals(right);

        #endregion
    }
}
