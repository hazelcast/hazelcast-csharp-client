using System;
using System.Globalization;
using Hazelcast.Sql;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    public class SqlDataTypesTests
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        [Test]
        [TestCase("0")]
        [TestCase("1")]
        [TestCase("-1")]
        [TestCase("0.1")]
        [TestCase("-0.1")]
        [TestCase("10")]
        [TestCase("-10")]
        [TestCase("0.1234567890123456789012345678")]
        [TestCase("12345678901234567890123456789")]
        [TestCase("79228162514264337593543950335")] // decimal.MaxValue
        [TestCase("-79228162514264337593543950335")] // decimal.MinValue
        [TestCase("0.0000000000000000000000000001")] // decimal.Epsilon
        [TestCase("-0.0000000000000000000000000001")] // -decimal.Epsilon
        [TestCase("10000000000000000000000000000")] // 1 / decimal.Epsilon
        [TestCase("-10000000000000000000000000000")] // -1 / decimal.Epsilon
        [TestCase("79228162514264337593543950336")] // too large
        [TestCase("0.00000000000000000000000000001")] // too small
        public void BigDecimal(string stringValue)
        {
            var bigDecimalValue = HBigDecimal.Parse(stringValue, Culture);

            if (decimal.TryParse(stringValue, NumberStyles.Float, Culture, out var decimalValue)
                && decimalValue.ToString(Culture) == stringValue) // parsed and no value loss
            {
                Assert.AreEqual(decimalValue, (decimal)bigDecimalValue);
                Assert.AreEqual((HBigDecimal)decimalValue, bigDecimalValue);
                Assert.AreEqual(decimalValue.ToString(Culture), bigDecimalValue.ToString(Culture));
            }
            else
            {
                Assert.Throws<OverflowException>(() => _ = (decimal)bigDecimalValue);
                Assert.AreEqual(stringValue, bigDecimalValue.ToString(Culture));
            }
        }

        [TestCase("2020-07-15")]
        [TestCase("0001-01-01")] // DateTime.MinValue
        [TestCase("9999-12-31")] // DateTime.MaxValue
        [TestCase("12345-11-22")] // too large
        [TestCase("0000-01-01")] // too small
        [TestCase("-12345-11-22")] // negative
        public void LocalDate(string stringValue)
        {
            var localDateValue = HLocalDate.Parse(stringValue);

            if (DateTime.TryParse(stringValue, Culture, DateTimeStyles.None, out var dateTimeValue))
            {
                Assert.AreEqual(dateTimeValue, (DateTime)localDateValue);
                Assert.AreEqual((HLocalDate)dateTimeValue, localDateValue);
                Assert.AreEqual(dateTimeValue.ToString("yyyy-MM-dd"), localDateValue.ToString());
            }
            else
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = (DateTime)localDateValue);
                Assert.AreEqual(stringValue, localDateValue.ToString());
            }
        }

        [TestCase("12:34:56")]
        [TestCase("00:00:00")]
        [TestCase("23:59:59")]
        public void LocalTime(string stringValue)
        {
            var localTimeValue = HLocalTime.Parse(stringValue);
            var timeSpanValue = TimeSpan.Parse(stringValue, Culture);

            Assert.AreEqual(timeSpanValue, (TimeSpan)localTimeValue);
            Assert.AreEqual((HLocalTime)timeSpanValue, localTimeValue);
            Assert.AreEqual(timeSpanValue.ToString(@"hh\:mm\:ss"), localTimeValue.ToString());
        }

        [TestCase("2020-07-15T07:11:42")]
        [TestCase("0001-01-01T00:00:00")] // DateTime.MinValue
        [TestCase("9999-12-31T23:59:59")] // DateTime.MaxValue
        [TestCase("12345-11-22T11:22:33")] // too large
        [TestCase("0000-01-01T11:22:33")] // too small
        [TestCase("-12345-11-22T11:22:33")] // negative
        public void LocalDateTime(string stringValue)
        {
            var localDateTimeValue = HLocalDateTime.Parse(stringValue);

            if (DateTime.TryParse(stringValue, Culture, DateTimeStyles.None, out var dateTimeValue))
            {
                Assert.AreEqual(dateTimeValue, (DateTime)localDateTimeValue);
                Assert.AreEqual((HLocalDateTime)dateTimeValue, localDateTimeValue);
                Assert.AreEqual(dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss"), localDateTimeValue.ToString());
            }
            else
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = (DateTime)localDateTimeValue);
                Assert.AreEqual(stringValue, localDateTimeValue.ToString());
            }
        }

        [TestCase("2020-07-15T07:11:42Z")]
        [TestCase("2020-07-15T07:11:42+11:22")]
        [TestCase("0001-01-01T00:00:00Z")] // DateTimeOffset.MinValue
        [TestCase("0001-01-01T00:00:00-18:00")] // DateTimeOffset.MinValue + min offset
        [TestCase("9999-12-31T23:59:59Z")] // DateTimeOffset.MaxValue
        [TestCase("9999-12-31T23:59:59+18:00")] // DateTimeOffset.MaxValue + max offset
        [TestCase("12345-11-22T11:22:33Z")] // too large
        [TestCase("12345-11-22T11:22:33+12:34")] // too large with offset
        [TestCase("0000-01-01T11:22:33Z")] // too small
        [TestCase("0000-01-01T11:22:33-12:34")] // too small with offset
        [TestCase("-12345-11-22T11:22:33Z")] // negative
        [TestCase("-12345-11-22T11:22:33-12:34")] // negative with offset
        public void OffsetDateTime(string stringValue)
        {
            var offsetDateTimeValue = HOffsetDateTime.Parse(stringValue);

            if (DateTimeOffset.TryParse(stringValue, Culture, DateTimeStyles.None, out var dateTimeOffsetValue))
            {
                Assert.AreEqual(dateTimeOffsetValue, (DateTimeOffset)offsetDateTimeValue);
                Assert.AreEqual((HOffsetDateTime)dateTimeOffsetValue, offsetDateTimeValue);
                Assert.AreEqual(
                    dateTimeOffsetValue.Offset == TimeSpan.Zero
                        ? dateTimeOffsetValue.ToString("yyyy-MM-ddTHH:mm:ssZ")
                        : dateTimeOffsetValue.ToString("yyyy-MM-ddTHH:mm:ssK"),
                    offsetDateTimeValue.ToString()
                );
            }
            else
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = (DateTimeOffset)offsetDateTimeValue);
                Assert.AreEqual(stringValue, offsetDateTimeValue.ToString());
            }
        }
    }
}
