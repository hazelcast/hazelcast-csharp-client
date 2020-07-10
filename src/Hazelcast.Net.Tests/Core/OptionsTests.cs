using System;
using System.Collections.Generic;
using System.Text;
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
            var d = new Dictionary<string, string>
            {
                { "key.a", "value.a" },
                { "key.b", "value.b" },
            };

            var options = new InjectionOptions { TypeName = "typeName", Args = d };

            Assert.That(options.TypeName, Is.EqualTo("typeName"));
            Assert.That(options.Args, Is.SameAs(d));

            var toString = options.ToString();
            Console.WriteLine(toString);
            Assert.That(toString, Is.EqualTo("InjectionOptions typeName: 'typeName', key.a: 'value.a', key.b: 'value.b'"));
        }

        [Test]
        public void RetryOptions()
        {
            var options = new RetryOptions
            {
                ClusterConnectionTimeoutMilliseconds = 100,
                InitialBackoffMilliseconds = 200,
                Jitter = 2.2,
                MaxBackoffMilliseconds = 300,
                Multiplier = 3.3
            };

            static void AssertOptions(RetryOptions o)
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
