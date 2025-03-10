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
using System;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class OptionsTests
    {
        [Test]
        public void CoreOptions()
        {
            var options = new CoreOptions { Clock = { OffsetMilliseconds = 100 } };

            static void AssertOptions(CoreOptions o)
            {
                Assert.That(o.Clock.OffsetMilliseconds, Is.EqualTo(100));
            }

            var clone = options.Clone();
            AssertOptions(clone);

            clone.Clock.OffsetMilliseconds = 200;
            AssertOptions(options);
        }

        [Test]
        public void InjectionOptions()
        {
            var options = new InjectionOptions { TypeName = "typeName" };
            options.Args["key.a"] = "value.a";
            options.Args["key.b"] = "value.b";

            Assert.That(options.TypeName, Is.EqualTo("typeName"));

            var toString = options.ToString();
            Console.WriteLine(toString);
            Assert.That(toString, Is.EqualTo("InjectionOptions typeName: 'typeName', key.a: 'value.a', key.b: 'value.b'"));
        }

        [Test]
        public void RetryOptions()
        {
            var options = new ConnectionRetryOptions
            {
                ClusterConnectionTimeoutMilliseconds = 100,
                InitialBackoffMilliseconds = 200,
                Jitter = 2.2,
                MaxBackoffMilliseconds = 300,
                Multiplier = 3.3
            };

            static void AssertOptions(ConnectionRetryOptions o)
            {
                Assert.That(o.ClusterConnectionTimeoutMilliseconds, Is.EqualTo(100));
                Assert.That(o.InitialBackoffMilliseconds, Is.EqualTo(200));
                Assert.That(o.Jitter, Is.EqualTo(2.2));
                Assert.That(o.MaxBackoffMilliseconds, Is.EqualTo(300));
                Assert.That(o.Multiplier, Is.EqualTo(3.3));
            }

            AssertOptions(options);

            var clone = options.Clone();
            AssertOptions(clone);

            clone.ClusterConnectionTimeoutMilliseconds = 0;
            clone.InitialBackoffMilliseconds = 0;
            clone.Jitter = 0;
            clone.MaxBackoffMilliseconds = 0;
            clone.Multiplier = 0;
            AssertOptions(options);
        }
    }
}
