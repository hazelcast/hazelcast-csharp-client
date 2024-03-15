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

#nullable enable

#nullable enable

using System;
using System.Globalization;
using System.Numerics;

namespace Hazelcast.Models
{
    // NOTE:
    // this is a simplified version of BigDecimal, we may consider enhancing it in the future.
    // there *are* more complete versions out there that users can exist at little cost, since
    // they all ultimately are structs combining a BigInteger unscaled value with an int scale.

    /// <summary>
    /// Represents an Hazelcast <c>DECIMAL</c> primitive type value.
    /// </summary>
    /// <remarks>
    /// <para>The <c>DECIMAL</c> primitive type consists of a random precision <see cref="BigInteger"/>
    /// <see cref="UnscaledValue"/> and a <see cref="int"/> <see cref="Scale"/> which indicates the
    /// number of digits of <see cref="UnscaledValue"/> that should be to the right of the decimal
    /// point.</para>
    /// <para>The actual value is therefore <see cref="UnscaledValue"/>*(10^-<see cref="Scale"/>).</para>
    /// <para>Different combinations of (<c>UnscaledValue</c>, <c>Scale</c>) may represent the same
    /// <see cref="HBigDecimal"/> number. Use the <see cref="Normalize"/> method to get the unique
    /// normalized representation of the number.</para>
    /// <para>Corresponds to Java <c>java.math.BigDecimal</c>.</para>
    /// </remarks>
    public readonly struct HBigDecimal : IEquatable<HBigDecimal>
    {
        private static readonly NumberFormatInfo NoSignFormat = new NumberFormatInfo
        {
            NegativeSign = "",
            PositiveSign = ""
        };

        private static readonly BigInteger MaxDecimal = new BigInteger(decimal.MaxValue);
        private static readonly BigInteger BigInteger10 = new BigInteger(10);

        /// <summary>
        /// Initializes a new instance of the <see cref="HBigDecimal"/> struct.
        /// </summary>
        /// <param name="unscaledValue">The unscaled component of the number.</param>
        /// <param name="scale">The scale component of the number.</param>
        public HBigDecimal(BigInteger unscaledValue, int scale = 0)
        {
            if (scale < 0) throw new ArgumentOutOfRangeException(nameof(scale), "Value cannot be negative.");

            UnscaledValue = unscaledValue;
            Scale = scale;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HBigDecimal"/> struct.
        /// </summary>
        /// <param name="value">The value of the number.</param>
        public HBigDecimal(int value)
            : this(new BigInteger(value))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HBigDecimal"/> struct.
        /// </summary>
        /// <param name="value">The value of the number.</param>
        public HBigDecimal(decimal value)
        {
            if (value == 0M)
            {
                UnscaledValue = 0;
                Scale = 0;
                return;
            }

            const int signMask = unchecked((int)0x80000000);
            const int scaleMask = 0x00FF0000;

            var valueBytes = decimal.GetBits(value);

            var unscaledValue = (new BigInteger(((ulong)(uint)valueBytes[2] << 32) | (uint)valueBytes[1]) << 32) | (uint)valueBytes[0];
            if ((valueBytes[3] & signMask) != 0) unscaledValue = -unscaledValue;

            var scale = (valueBytes[3] & scaleMask) >> 16;

            UnscaledValue = unscaledValue;
            Scale = scale;
        }

        /// <summary>
        /// Gets a value that represents the number 0 (zero).
        /// </summary>
        public static readonly HBigDecimal Zero = new HBigDecimal(BigInteger.Zero);

        /// <summary>
        /// Gets a value that represents the number 1 (one).
        /// </summary>
        public static readonly HBigDecimal One = new HBigDecimal(BigInteger.One);

        /// <summary>
        /// Gets a value that represents the number -1 (minus one).
        /// </summary>
        public static readonly HBigDecimal MinusOne = new HBigDecimal(BigInteger.MinusOne);

        /// <summary>
        /// Gets a value that represents the number 10 (ten).
        /// </summary>
        public static readonly HBigDecimal Ten = new HBigDecimal(10);

        /// <summary>
        /// Gets the unscaled value part of the number.
        /// </summary>
        public BigInteger UnscaledValue { get; }

        /// <summary>
        /// Gets the scale part of the number.
        /// </summary>
        public int Scale { get; }

        /// <summary>
        /// Implements the <see cref="HBigDecimal"/> to <see cref="decimal"/> conversion.
        /// </summary>
        /// <exception cref="OverflowException">The <paramref name="value"/> instance represents a number
        /// that is less than <see cref="decimal.MinValue"/> or greater than <see cref="decimal.MaxValue"/>.</exception>
        public static explicit operator decimal(HBigDecimal value) => value.ToDecimal();

        /// <summary>
        /// Implements the <see cref="decimal"/> to <see cref="HBigDecimal"/> conversion.
        /// </summary>
        public static explicit operator HBigDecimal(decimal value) => new HBigDecimal(value);

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent <see cref="decimal"/> representation.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="value">When this method returns, contains the <see cref="decimal"/> equivalent of
        /// the number represented by this instance, if the conversion succeeded, or zero if the conversion
        /// failed.</param>
        /// <returns><c>true</c> if the number represented by this instance was converted successfully; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>A <see cref="HBigDecimal"/> value can be converted to <see cref="decimal"/> if it is greater than or equal to
        /// <see cref="decimal.MinValue"/> and less than or equal to <see cref="decimal.MaxValue"/>.</para>
        /// </remarks>
        public bool TryToDecimal(out decimal value)
        {
            value = default;

            var normalized = Normalize();
            var (unscaled, scale) = (normalized.UnscaledValue, normalized.Scale);

            var divisor = BigInteger.Pow(BigInteger10, scale);
            var remainder = BigInteger.Remainder(unscaled, divisor);
            var scaled = BigInteger.Divide(unscaled, divisor);

            if (scaled > MaxDecimal)
                return false;

            value = (decimal)scaled + (decimal)remainder / (decimal)divisor;
            return true;
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent <see cref="decimal"/> representation.
        /// </summary>
        /// <returns>The <see cref="decimal"/> representation of this instance.</returns>
        /// <exception cref="OverflowException">This instance represents a number that is less than <see cref="decimal.MinValue"/>
        /// or greater than <see cref="decimal.MaxValue"/>.</exception>
        public decimal ToDecimal() => TryToDecimal(out var value)
            ? value
            : throw new OverflowException($"Value was either too large or too small for {nameof(Decimal)}.");

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent <see cref="string"/> representation.
        /// </summary>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information.</param>
        /// <returns>The string representation of the value of this instance as specified by <paramref name="cultureInfo"/>.</returns>
        public string ToString(CultureInfo cultureInfo)
        {
            if (cultureInfo == null) throw new ArgumentNullException(nameof(cultureInfo));

            var separator = cultureInfo.NumberFormat.NumberDecimalSeparator;
            var unscaledString = UnscaledValue.ToString("G", NoSignFormat) ?? "0";
            var unsignedString = Scale switch
            {
                var scale when scale < 0 => $"{unscaledString}{new string('0', -scale)}",
                var scale when scale > 0 && scale < unscaledString.Length => $"{unscaledString[..^scale]}.{unscaledString[^scale..]}",
                var scale when scale >= unscaledString.Length => $"0{separator}{new string('0', scale - unscaledString.Length)}{unscaledString}",
                _ => unscaledString
            };

            return (UnscaledValue.Sign < 0 ? '-' : (char?)null) + unsignedString;
        }

        /// <inheritdoc />
        public override string ToString() => ToString(CultureInfo.CurrentCulture);

        /// <summary>
        /// Converts the string representation of a number to its <see cref="HBigDecimal"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information about <paramref name="s"/>.</param>
        /// <param name="bigDecimal">When this method returns, contains the <see cref="HBigDecimal"/> equivalent of
        /// the number contained in <paramref name="s"/>, if the conversion succeeded, or zero if the conversion failed.</param>
        /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string? s, CultureInfo cultureInfo, out HBigDecimal bigDecimal)
        {
            // s can be null and we'll return false
            if (cultureInfo == null) throw new ArgumentNullException(nameof(cultureInfo));

            bigDecimal = default;
            if (s == null)
                return false;

            var separator = cultureInfo.NumberFormat.NumberDecimalSeparator;
            if (!BigInteger.TryParse(s.Replace(separator, "", StringComparison.Ordinal), NumberStyles.Integer, cultureInfo, out var unscaled))
                return false;

            var scale = 0;
            if (s.IndexOf(separator, StringComparison.Ordinal) is var separatorIndex && separatorIndex >= 0)
                scale = s.Length - separatorIndex - 1;

            if (separatorIndex != s.LastIndexOf(separator, StringComparison.Ordinal))
                return false;

            bigDecimal = new HBigDecimal(unscaled, scale);
            return true;
        }

        /// <summary>
        /// Converts the string representation of a number to its <see cref="HBigDecimal"/> equivalent.
        /// A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="bigDecimal">When this method returns, contains the <see cref="HBigDecimal"/> equivalent of
        /// the number contained in <paramref name="s"/>, if the conversion succeeded, or zero if the conversion failed.</param>
        /// <returns><c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string s, out HBigDecimal bigDecimal) => TryParse(s, CultureInfo.CurrentCulture, out bigDecimal);

        /// <summary>
        /// Converts the string representation of a number to its <see cref="HBigDecimal"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information about <paramref name="s"/>.</param>
        /// <returns>A <see cref="HBigDecimal"/> equivalent to the number contained in <paramref name="s"/>.</returns>
        public static HBigDecimal Parse(string s, CultureInfo cultureInfo) => TryParse(s, cultureInfo, out var bigDecimal)
            ? bigDecimal
            : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HBigDecimal)}.");

        /// <summary>
        /// Converts the string representation of a number to its <see cref="HBigDecimal"/> equivalent.
        /// </summary>
        /// <param name="s">A string containing a number to convert.</param>
        /// <returns>A <see cref="HBigDecimal"/> equivalent to the number contained in <paramref name="s"/>.</returns>
        public static HBigDecimal Parse(string s) => Parse(s, CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns equivalent <see cref="HBigDecimal"/> value but with <see cref="Scale"/> guaranteed to be non-negative.
        /// </summary>
        public HBigDecimal Normalize()
        {
            var unscaled = UnscaledValue;
            var scale = Scale;

            static bool CanNormalize(BigInteger u, int s) => (u <= -10 || u >= 10) && s > 0;

            if (!CanNormalize(unscaled, scale)) return this;

            while (CanNormalize(unscaled, scale))
            {
                var divided = BigInteger.DivRem(unscaled, BigInteger10, out var remainder);
                if (remainder != 0) return new HBigDecimal(unscaled, scale);
                unscaled = divided;
                scale -= 1;
            }

            return new HBigDecimal(unscaled, scale);
        }

        #region Equality members

        /// <inheritdoc />
        public bool Equals(HBigDecimal other)
        {
            var (thisNormalized, otherNormalized) = (Normalize(), other.Normalize());
            return (thisNormalized.UnscaledValue, thisNormalized.Scale) == (otherNormalized.UnscaledValue, otherNormalized.Scale);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is HBigDecimal other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var normalized = Normalize();
            return HashCode.Combine(normalized.UnscaledValue, normalized.Scale);
        }

        /// <summary>Implements the == operator.</summary>
        public static bool operator ==(HBigDecimal left, HBigDecimal right) => left.Equals(right);

        /// <summary>Implements the != operator.</summary>
        public static bool operator !=(HBigDecimal left, HBigDecimal right) => !left.Equals(right);

        #endregion
    }
}
