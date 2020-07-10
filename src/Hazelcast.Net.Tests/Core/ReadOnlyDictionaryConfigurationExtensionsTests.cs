using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ReadOnlyDictionaryConfigurationExtensionsTests
    {
        [Test]
        public void GetStringValue()
        {
            var d = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { "key_a", "value_a" }
            });

            Assert.That(d.GetStringValue("key_a"), Is.EqualTo("value_a"));
            Assert.Throws<InvalidOperationException>(() => d.GetStringValue("key_b"));

            Assert.That(d.TryGetStringValue("key_a", out var value), Is.True);
            Assert.That(value, Is.EqualTo("value_a"));
            Assert.That(d.TryGetStringValue("key_b", out value), Is.False);
        }

        [Test]
        public void GetGuidValue()
        {
            var g = Guid.NewGuid();
            var d = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { "key_a", "value_a" },
                { "key_b", g.ToString() }
            });

            Assert.Throws<InvalidOperationException>(() => d.GetGuidValue("key_a"));
            Assert.That(d.GetGuidValue("key_b"), Is.EqualTo(g));
            Assert.Throws<InvalidOperationException>(() => d.GetGuidValue("key_c"));

            Assert.That(d.TryGetGuidValue("key_a", out var value), Is.False);
            Assert.That(d.TryGetGuidValue("key_b", out value), Is.True);
            Assert.That(value, Is.EqualTo(g));
            Assert.That(d.TryGetGuidValue("key_c", out value), Is.False);
        }

        [Test]
        public void GetIntValue()
        {
            var d = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { "key_a", "value_a" },
                { "key_b", "42" }
            });

            Assert.Throws<InvalidOperationException>(() => d.GetIntValue("key_a"));
            Assert.That(d.GetIntValue("key_b"), Is.EqualTo(42));
            Assert.Throws<InvalidOperationException>(() => d.GetIntValue("key_c"));

            Assert.That(d.TryGetIntValue("key_a", out var value), Is.False);
            Assert.That(d.TryGetIntValue("key_b", out value), Is.True);
            Assert.That(value, Is.EqualTo(42));
            Assert.That(d.TryGetIntValue("key_c", out value), Is.False);
        }

        [Test]
        public void ArgumentExceptions()
        {
            ReadOnlyDictionary<string, string> d = null;

            Assert.Throws<ArgumentNullException>(() => d.GetStringValue("key"));
            Assert.Throws<ArgumentNullException>(() => d.TryGetStringValue("key", out _));

            Assert.Throws<ArgumentNullException>(() => d.GetGuidValue("key"));
            Assert.Throws<ArgumentNullException>(() => d.TryGetGuidValue("key", out _));

            Assert.Throws<ArgumentNullException>(() => d.GetIntValue("key"));
            Assert.Throws<ArgumentNullException>(() => d.TryGetIntValue("key", out _));
        }
    }
}
