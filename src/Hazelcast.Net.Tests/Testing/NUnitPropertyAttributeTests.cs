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

using System.Linq;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    [Property("FIXTURE", "fixture")]
    public class NUnitPropertyAttributeTests
    {
        private TestContext _fixtureContext;
        private TestContext _setupContext;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _fixtureContext = TestContext.CurrentContext;
        }

        [SetUp]
        public void SetUp()
        {
            _setupContext = TestContext.CurrentContext;
        }

        [Test]
        [Property("METHOD", "method")]
        public void CanSetAndGetTestProperties()
        {
            // test fixture properties are available on the fixture context only
            Assert.AreEqual("fixture", _fixtureContext.Test.Properties["FIXTURE"]?.FirstOrDefault());
            Assert.IsNull(TestContext.CurrentContext.Test.Properties["FIXTURE"]?.FirstOrDefault());

            // test method properties are immediately available
            Assert.AreEqual("method", TestContext.CurrentContext.Test.Properties["METHOD"]?.FirstOrDefault());
        }

        [TestCase(0)]
        [TestCase(1)]
        [Property("METHOD", "method")]
        public void CanSetAndGetTestSuiteProperties(int ignored)
        {
            // test fixture properties are available on the fixture context only
            Assert.AreEqual("fixture", _fixtureContext.Test.Properties["FIXTURE"]?.FirstOrDefault());
            Assert.That(TestContext.CurrentContext.Test.Properties["FIXTURE"], Is.Empty);

            // test suite method properties are *not* immediately available
            // and, don't even know how to capture them really
            Assert.That(TestContext.CurrentContext.Test.Properties["METHOD"], Is.Empty);
        }

        [Test]
        public void Contexts()
        {
            // these were captured correctly
            Assert.That(_fixtureContext, Is.Not.Null);
            Assert.That(_setupContext, Is.Not.Null);

            // this one is always present
            Assert.That(TestContext.CurrentContext, Is.Not.Null);

            // the fixture context != the other contexts
            Assert.That(_fixtureContext, Is.Not.SameAs(TestContext.CurrentContext));
            Assert.That(_fixtureContext, Is.Not.SameAs(_setupContext));

            // and even these two tests are !=
            Assert.That(_setupContext, Is.Not.SameAs(TestContext.CurrentContext));
        }
    }
}
