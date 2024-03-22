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

using System;
using System.Globalization;
using System.Numerics;
using Hazelcast.Models;
using NUnit.Framework;

namespace Hazelcast.Tests.Models
{
    [TestFixture]
    public class HazelcastDataTypesTests
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        // FIXME - these tests need to be re-organized + need more tests

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

                var otherBigDecimalValue = new HBigDecimal(13); // random different value
                Assert.True(bigDecimalValue != otherBigDecimalValue);
#pragma warning disable CS1718 // Comparison made to same variable
                // ReSharper disable once EqualExpressionComparison
                Assert.True(bigDecimalValue == bigDecimalValue);
#pragma warning restore CS1718 // Comparison made to same variable
                Assert.True(HBigDecimal.TryParse(stringValue, Culture, out _));
            }
            else
            {
                Assert.Throws<OverflowException>(() => _ = (decimal)bigDecimalValue);
                Assert.AreEqual(stringValue, bigDecimalValue.ToString(Culture));
            }
        }

        [Test]
        public void DecimalScalingFactor10Pow0To28()
        {
            // validate the structure and range of a .NET 'decimal' number.

            // a .NET 'decimal' is a 128 bits floating-point number consisting in a
            // sign, a 96 bits integer numeric value, and a scaling factor. The sign
            // and scaling factor fit in the 32 remaining bytes. scaling factor is
            // 0-28. therefore, range is  -(2^96 - 1) to +(2^96 + 1) and then scale
            // is zero, and finest precision is 1 / 10^28 (smaller than this is zero).

            var d = 1M; // 1*10^0
            for (var i = 0; i < 30; i++)
            {
                // i == 0  -> d = 1   = 1*10^0,   dd = .1  = 1*10^-1
                // i == 2  -> d = .1  = 1*10^-1,  dd = .01 = 1*10^-2
                // ...
                // i == 27 -> d = ... = 1*10^-27, dd = ... = 1*10^-28
                // i == 28 -> d = ... = 1*10^-28, dd = ... = 1*10^-29

                var dd = d / 10;

                if (i < 28)
                {
                    Assert.That(dd != d);
                    Assert.That(dd != 0);
                }
                else if (i == 28)
                {
                    // dd = 1*10^-29 which is zero (scaling factor only supports 28 bits)
                    Assert.That(dd != d);
                    Assert.That(dd == 0);
                }
                else
                {
                    Assert.That(dd == d);
                }

                d = dd;
            }
        }

        [Test]
        public void BigDecimalStructure()
        {
            var b1 = new HBigDecimal(10);
            Assert.That(b1.UnscaledValue, Is.EqualTo(new BigInteger(10)));
            Assert.That(b1.Scale, Is.EqualTo(0));
            Assert.That(b1.ToString(CultureInfo.InvariantCulture), Is.EqualTo("10"));

            var b2 = new HBigDecimal(100, 1);
            Assert.That(b2.UnscaledValue, Is.EqualTo(new BigInteger(100)));
            Assert.That(b2.Scale, Is.EqualTo(1));
            Assert.That(b2.ToString(CultureInfo.InvariantCulture), Is.EqualTo("10.0")); // consistent with decimal (see below)
            Assert.That(10.0M.ToString(CultureInfo.InvariantCulture), Is.EqualTo("10.0"));

            var b3 = new HBigDecimal(.1M);
            Assert.That(b3.UnscaledValue, Is.EqualTo(new BigInteger(1)));
            Assert.That(b3.Scale, Is.EqualTo(1));
            Assert.That(b3.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0.1")); // consistent with decimal (see below)
            Assert.That(0.1M.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0.1"));

            var b4 = new HBigDecimal(1000, 4);
            Assert.That(b4.UnscaledValue, Is.EqualTo(new BigInteger(1000)));
            Assert.That(b4.Scale, Is.EqualTo(4));
            Assert.That(b4.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0.1000")); // consistent with decimal (see below)

            // decimal - has clever internal code so that when created from a double,
            // the unscaled + scale are adjusted, so it's 'normal' - OTOH when explicitly
            // created from unscaled + scale, they remain the same, hence the weird string
            // representation with trailing zeroes.

            var d0 = new decimal(.1D);
            Assert.That(d0, Is.EqualTo(.1M));
            Assert.That(d0.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0.1"));

            var d1 = new decimal(1000, 0, 0, false, 4);
            Assert.That(d1, Is.EqualTo(.1M));
            Assert.That(d1.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0.1000"));

            var d2 = new decimal(100, 0, 0, false, 3);
            Assert.That(d2, Is.EqualTo(.1M));
            Assert.That(d2.ToString(CultureInfo.InvariantCulture), Is.EqualTo("0.100"));

            // but in any case comparing normalizes the numbers and sees them equal

            Assert.That(d2, Is.EqualTo(d1));

            // big decimal - so, it makes sense that we do not normalize when explicitly
            // created from unscaled + scale. when created from decimal... we re-use
            // whatever was in the decimal and not normalize
            var b3X = new HBigDecimal(d1);
            Assert.That(b3X.UnscaledValue, Is.EqualTo(new BigInteger(1000)));
            Assert.That(b3X.Scale, Is.EqualTo(4));

            // now, comparison should normalize first, so let's test normalizing

            // this is the original 'normalize' method in 5.0 - it has issues
            HBigDecimal OriginalNormalize(HBigDecimal d)
            {
                var bigint10 = new BigInteger(10);
                return d.Scale >= 0 ? d : new HBigDecimal(d.UnscaledValue * BigInteger.Pow(bigint10, -d.Scale));
            }

            var n1 = OriginalNormalize(b1);
            Assert.That(n1.UnscaledValue, Is.EqualTo(new BigInteger(10)));
            Assert.That(n1.Scale, Is.EqualTo(0));

            var n2 = OriginalNormalize(b2);
            Assert.That(n2.UnscaledValue, Is.EqualTo(new BigInteger(100))); // and well, it's not normalized at all!
            Assert.That(n2.Scale, Is.EqualTo(1));

            var nf1 = n1.Normalize();
            Assert.That(nf1.UnscaledValue, Is.EqualTo(new BigInteger(10)));
            Assert.That(nf1.Scale, Is.EqualTo(0));

            var nf2 = b2.Normalize();
            Assert.That(nf2.UnscaledValue, Is.EqualTo(new BigInteger(10)));
            Assert.That(nf2.Scale, Is.EqualTo(0));

            var nf3 = b3.Normalize();
            Assert.That(nf3.UnscaledValue, Is.EqualTo(new BigInteger(1)));
            Assert.That(nf3.Scale, Is.EqualTo(1));

            var nf4 = b4.Normalize();
            Assert.That(nf4.UnscaledValue, Is.EqualTo(new BigInteger(1)));
            Assert.That(nf4.Scale, Is.EqualTo(1));

            // and then, test comparision
            // with normalized fixed, comparison works
            Assert.That(b1.Equals(b2));
            Assert.That(!b1.Equals(b3));
            Assert.That(b3.Equals(b4));
            Assert.That(b3.Equals(b3X));

            // original normalized failed
            //Assert.That(b1.Equals(b2), $"{b1} != {b2}"); // failed, 10 != 10.0
            //Assert.That(b1, Is.Not.EqualTo(b2)); // failed, expected 10.0 but was 10
        }

        [Test]
        public void BigDecimalConversions()
        {
            // asserts that the decimal <-> HBigDecimal also work with nullable

            var d0 = 123.456789M;
            var b = (HBigDecimal)d0;
            var d = (decimal)b;
            Assert.That(d, Is.EqualTo(d0));

            decimal? nd0 = d0;
            var nb = (HBigDecimal?)nd0;
            var nd = (decimal?)nb;
            Assert.That(nd, Is.EqualTo(nd0));
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
                var refLocalDateVal = new HLocalDate(localDateValue.ToDateTime());
                
                Assert.AreEqual(dateTimeValue, (DateTime)localDateValue);
                Assert.AreEqual((HLocalDate)dateTimeValue, localDateValue);
                Assert.AreEqual(dateTimeValue.ToString("yyyy-MM-dd"), localDateValue.ToString());
                Assert.False(refLocalDateVal!= localDateValue);
                // ReSharper disable once EqualExpressionComparison
                Assert.True(localDateValue == localDateValue);
                Assert.True(localDateValue.Equals(refLocalDateVal));
                Assert.True(localDateValue.Equals((object)refLocalDateVal));
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
            var reflocalTime = HLocalTime.Parse(stringValue);
            var timeSpanValue = TimeSpan.Parse(stringValue, Culture);

            Assert.AreEqual(timeSpanValue, (TimeSpan)localTimeValue);
            Assert.AreEqual((HLocalTime)timeSpanValue, localTimeValue);
            Assert.AreEqual(timeSpanValue.ToString(@"hh\:mm\:ss"), localTimeValue.ToString());
            Assert.True(localTimeValue == reflocalTime);
            Assert.False(localTimeValue != reflocalTime);
            Assert.True(localTimeValue.Equals(localTimeValue));
            Assert.True(localTimeValue.Equals((object) localTimeValue));

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
                var refLocalDateTimeValue = HLocalDateTime.Parse(stringValue);
                Assert.AreEqual(dateTimeValue, (DateTime)localDateTimeValue);
                Assert.AreEqual((HLocalDateTime)dateTimeValue, localDateTimeValue);
                Assert.AreEqual(dateTimeValue.ToString("yyyy-MM-ddTHH:mm:ss"), localDateTimeValue.ToString());
                Assert.True(localDateTimeValue == refLocalDateTimeValue);
                Assert.False(localDateTimeValue != refLocalDateTimeValue);
                Assert.True(localDateTimeValue.Equals(refLocalDateTimeValue));
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
                
                // ReSharper disable once EqualExpressionComparison
                Assert.False(offsetDateTimeValue != offsetDateTimeValue);
                // ReSharper disable once EqualExpressionComparison
                Assert.True(offsetDateTimeValue == offsetDateTimeValue);
                Assert.True(offsetDateTimeValue.Equals(offsetDateTimeValue));
                Assert.True(offsetDateTimeValue.Equals((object)offsetDateTimeValue));
            }
            else
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = (DateTimeOffset)offsetDateTimeValue);
                Assert.AreEqual(stringValue, offsetDateTimeValue.ToString());
            }
        }
    }
}
