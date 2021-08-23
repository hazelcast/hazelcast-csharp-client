using System;
using System.Globalization;
using System.Numerics;

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents Hazelcast SQL <c>DECIMAL</c> type corresponding to <c>java.math.BigDecimal</c> in Java.
    /// </summary>
    public readonly struct HBigDecimal: IEquatable<HBigDecimal>
    {
        private static readonly NumberFormatInfo NoSignFormat = new NumberFormatInfo
        {
            NegativeSign = "",
            PositiveSign = ""
        };

        private static readonly BigInteger MaxDecimal = new BigInteger(decimal.MaxValue);

        public static readonly HBigDecimal Zero = new HBigDecimal(BigInteger.Zero);
        public static readonly HBigDecimal One = new HBigDecimal(BigInteger.One);
        public static readonly HBigDecimal MinusOne = new HBigDecimal(BigInteger.MinusOne);
        public static readonly HBigDecimal Ten = new HBigDecimal(10);

        /// <summary>
        /// Unscaled part of number.
        /// Final value is calculated as <see cref="UnscaledValue"/>*(10^-<see cref="Scale"/>)
        /// </summary>
        public BigInteger UnscaledValue { get; }

        /// <summary>
        /// Scale to apply to <see cref="UnscaledValue"/> to get represented number.
        /// Final value is calculated as <see cref="UnscaledValue"/>*(10^-<see cref="Scale"/>)
        /// </summary>
        public int Scale { get; }

        public HBigDecimal(BigInteger unscaledValue, int scale = 0)
        {
            UnscaledValue = unscaledValue;
            Scale = scale;
        }

        public HBigDecimal(int value)
        {
            UnscaledValue = new BigInteger(value);
            Scale = 0;
        }

        public HBigDecimal(decimal value): this()
        {
            if (value == 0M) return;

            const int signMask = unchecked((int)0x80000000);
            const int scaleMask = 0x00FF0000;

            var valueBytes = decimal.GetBits(value);

            var unscaledValue = (new BigInteger(((ulong)(uint)valueBytes[2] << 32) | (uint)valueBytes[1]) << 32) | (uint)valueBytes[0];
            if ((valueBytes[3] & signMask) != 0) unscaledValue = -unscaledValue;

            var scale = (valueBytes[3] & scaleMask) >> 16;

            UnscaledValue = unscaledValue;
            Scale = scale;
        }

        public static explicit operator decimal(HBigDecimal bigDecimal) => bigDecimal.ToDecimal();
        public static explicit operator HBigDecimal(decimal @decimal) => new HBigDecimal(@decimal);

        public bool TryToDecimal(out decimal value)
        {
            value = default;

            var normalized = Normalize();
            var (unscaled, scale) = (normalized.UnscaledValue, normalized.Scale);

            var divisor = Pow10(scale);
            var remainder = BigInteger.Remainder(unscaled, divisor);
            var scaled = BigInteger.Divide(unscaled, divisor);

            if (scaled > MaxDecimal)
                return false;

            value = (decimal)scaled + (decimal)remainder / (decimal)divisor;
            return true;
        }

        public decimal ToDecimal() => TryToDecimal(out var value)
            ? value
            : throw new OverflowException($"Value was either too large or too small for {nameof(Decimal)}.");

        public string ToString(CultureInfo cultureInfo)
        {
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

        public override string ToString() => ToString(CultureInfo.CurrentCulture);

        public static bool TryParse(string s, CultureInfo cultureInfo, out HBigDecimal bigDecimal)
        {
            bigDecimal = default;
            var separator = cultureInfo.NumberFormat.NumberDecimalSeparator;

            if (!BigInteger.TryParse(s.Replace(separator, ""), NumberStyles.Integer, cultureInfo, out var unscaled))
                return false;

            var scale = 0;
            if (s.IndexOf(separator, StringComparison.Ordinal) is var separatorIndex && separatorIndex >= 0)
                scale = s.Length - separatorIndex - 1;

            if (separatorIndex != s.LastIndexOf(separator, StringComparison.Ordinal))
                return false;

            bigDecimal = new HBigDecimal(unscaled, scale);
            return true;
        }

        public static bool TryParse(string s, out HBigDecimal bigDecimal) => TryParse(s, CultureInfo.CurrentCulture, out bigDecimal);

        public static HBigDecimal Parse(string s, CultureInfo cultureInfo) => TryParse(s, cultureInfo, out var bigDecimal)
            ? bigDecimal
            : throw new FormatException($"Failed to parse \"{s}\" as {nameof(HBigDecimal)}.");

        public static HBigDecimal Parse(string s) => Parse(s, CultureInfo.CurrentCulture);

        private BigInteger Pow10(int power) => BigInteger.Pow(new BigInteger(10), power);

        /// <summary>
        /// Returns equivalent <see cref="HBigDecimal"/> value but with <see cref="Scale"/> guaranteed to be non-negative.
        /// </summary>
        private HBigDecimal Normalize() => Scale >= 0 ? this : new HBigDecimal(UnscaledValue * Pow10(-Scale));

        #region Equality members

        public bool Equals(HBigDecimal other)
        {
            var (thisNormalized, otherNormalized) = (Normalize(), other.Normalize());
            return thisNormalized.UnscaledValue.Equals(otherNormalized.UnscaledValue) && thisNormalized.Scale == otherNormalized.Scale;
        }

        public override bool Equals(object obj)
        {
            return obj is HBigDecimal other && Equals(other);
        }

        public override int GetHashCode()
        {
            var normalized = Normalize();
            return HashCode.Combine(normalized.UnscaledValue, normalized.Scale);
        }

        public static bool operator ==(HBigDecimal left, HBigDecimal right) => left.Equals(right);

        public static bool operator !=(HBigDecimal left, HBigDecimal right) => !left.Equals(right);

        #endregion

    }
}
