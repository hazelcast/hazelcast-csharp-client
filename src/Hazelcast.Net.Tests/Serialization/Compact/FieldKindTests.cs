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
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Serialization;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class FieldKindTests
    {
        private const int MinValue = -1000;
        private const int MaxValue = 1000;

        [Test]
        public void TestsAreSafe()
        {
            // all defined values must fit within out tests range 0-MaxValue
            // else our tests are not really testing and we need to fix the range

            foreach (var obj in Enum.GetValues(typeof (FieldKind)))
            {
                var intValue = (int) obj;
                Assert.That(intValue, Is.GreaterThanOrEqualTo(MinValue), "Value < tests MinValue, must fix tests.");
                Assert.That(intValue, Is.LessThanOrEqualTo(MaxValue), "Value > tests MaxValue, must fix tests.");
            }
        }

        [Test]
        public void CanParseAllValues()
        {
            // parsing all defined values should not throw

            foreach (var obj in Enum.GetValues(typeof (FieldKind)))
            {
                var intValue = (int) obj;
                var value = FieldKindEnum.Parse(intValue);
            }
        }

        [Test]
        public void CannotParseNonValues()
        {
            var values = new HashSet<int>(Enum.GetValues(typeof (FieldKind)).Cast<int>());

            // not going to test for *every* integer, let's stick to the test range

            for (var i = MinValue; i < MaxValue; i++)
            {
                // skip known values
                if (values.Contains(i)) continue;

                // anything else should throw
                Assert.Throws<ArgumentException>(() => FieldKindEnum.Parse(i));
            }
        }
    }
}
