// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Serialization;
namespace Hazelcast.Tests.Models
{
    using NUnit.Framework;
    using NSubstitute;

    [TestFixture]
    public class TimedExpiryPolicyFactoryOptionsTests
    {
        [Test]
        public void DefaultConstructor_SetsPropertiesToDefault()
        {
            var options = new TimedExpiryPolicyFactoryOptions();

            Assert.AreEqual(ExpiryPolicyType.Created, options.ExpiryPolicyType);
        }

        [Test]
        public void Constructor_WithArguments_SetsProperties()
        {
            var durationConfig = new DurationOptions();
            var options = new TimedExpiryPolicyFactoryOptions(ExpiryPolicyType.Created, durationConfig);

            Assert.AreEqual(ExpiryPolicyType.Created, options.ExpiryPolicyType);
            Assert.AreEqual(durationConfig, options.DurationConfig);
        }

        [Test]
        public void WriteData_WritesProperties()
        {
            var output = Substitute.For<IObjectDataOutput>();
            var durationConfig = new DurationOptions();
            var options = new TimedExpiryPolicyFactoryOptions(ExpiryPolicyType.Created, durationConfig);

            options.WriteData(output);

            output.Received().WriteString(ExpiryPolicyType.Created.ToJavaString());
            output.Received().WriteObject(durationConfig);
        }

        [Test]
        public void ReadData_ReadsProperties()
        {
            var input = Substitute.For<IObjectDataInput>();
            var options = new TimedExpiryPolicyFactoryOptions();

            input.ReadString().Returns(ExpiryPolicyType.Created.ToJavaString());
            input.ReadObject<DurationOptions>().Returns(new DurationOptions());

            options.ReadData(input);

            Assert.AreEqual(ExpiryPolicyType.Created, options.ExpiryPolicyType);
            Assert.IsNotNull(options.DurationConfig);
        }

        [Test]
        public void ToString_ReturnsExpectedFormat()
        {
            var durationConfig = new DurationOptions();
            var options = new TimedExpiryPolicyFactoryOptions(ExpiryPolicyType.Created, durationConfig);

            var result = options.ToString();

            StringAssert.Contains("expiryPolicyType=Created", result);
            StringAssert.Contains("durationConfig=", result);
        }
    }
}
