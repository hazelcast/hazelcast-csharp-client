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
    }
}
