// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    [Some("FIXTURE", "fixture")]
    public class NUnitTestActionAttributeTests
    {
        [Test]
        [Some("TEST", "test")]
        public void Test()
        {
            var properties = TestContext.CurrentContext.Test.Properties;

            foreach (var key in properties.Keys)
            {
                var values = properties[key];
                if (values == null) continue;
                foreach (var value in values)
                {
                    Console.WriteLine($"{key}: {value}");
                }
            }

            Assert.That(TestContext.CurrentContext.Test.Properties["FIXTURE"]?.FirstOrDefault(), Is.EqualTo("fixture"));
            Assert.That(TestContext.CurrentContext.Test.Properties["TEST"]?.FirstOrDefault(), Is.EqualTo("test"));
        }

        // use a dummy ignored parameter to create a test "suite" ie 1 method running multiple tests
        [TestCase(0)]
        [TestCase(1)]
        [Some("TEST", "test")]
        public void TestSuite(int ignored)
        {
            var properties = TestContext.CurrentContext.Test.Properties;

            foreach (var key in properties.Keys)
            {
                var values = properties[key];
                if (values == null) continue;
                foreach (var value in values)
                {
                    Console.WriteLine($"{key}: {value}");
                }
            }

            Assert.That(TestContext.CurrentContext.Test.Properties["FIXTURE"]?.FirstOrDefault(), Is.EqualTo("fixture"));
            Assert.That(TestContext.CurrentContext.Test.Properties["TEST"]?.FirstOrDefault(), Is.EqualTo("test"));
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SomeAttribute : Attribute, ITestAction
    {
        private readonly string _name;
        private readonly string _value;

        public SomeAttribute(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public void BeforeTest(ITest test)
        {
            test.Properties.Add(_name, _value);
        }

        public void AfterTest(ITest test)
        { }

        public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;
    }
}
