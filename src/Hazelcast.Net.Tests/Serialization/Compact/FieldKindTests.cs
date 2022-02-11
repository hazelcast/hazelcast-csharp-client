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
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Serialization;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class FieldKindTests
    {
        private const int MaxValue = 1000;

        [Test]
        public void CanParseAllValues()
        {
            foreach (var obj in Enum.GetValues(typeof (FieldKind)))
            {
                var intValue = (int)obj;
                Assert.That(intValue, Is.GreaterThanOrEqualTo(0), "Value is negative, must fix tests.");
                Assert.That(intValue, Is.LessThanOrEqualTo(MaxValue), "Value exceeds MaxValue, must fix tests.");
                var value = FieldKindEnum.Parse(intValue); // this should not throw
            }
        }

        [Test]
        public void CannotParseNonValues()
        {
            var values = new HashSet<int>(Enum.GetValues(typeof (FieldKind)).Cast<int>());

            // not going to test for *every* integer, so let's assume that we are never going
            // to have neither negative nor greater-than-max values - should that happen, the
            // other test will fail and we will deal with it

            for (var i = 0; i < MaxValue; i++)
            {
                // skip known values
                if (values.Contains(i)) continue;

                // anything else should throw
                Assert.Throws<ArgumentException>(() => FieldKindEnum.Parse(i));
            }
        }
    }
}
