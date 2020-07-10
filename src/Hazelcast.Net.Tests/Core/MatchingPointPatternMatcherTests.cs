using Hazelcast.Configuration;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class MatchingPointPatternMatcherTests
    {
        [TestCase("aaa", null, "bbb")]
        [TestCase("aaa", "aaa", "aaa", "bbb")]
        [TestCase("aaa", "aaa*", "aaa*", "bbb")]
        [TestCase("aaa", "aaa", "aaa", "a*", "aaa*", "bbb")]
        [TestCase("aax", "aa*", "aa*", "bb*")]
        [TestCase("axa", "a*a", "a*a", "b*b")]
        [TestCase("xaa", "*aa", "*aa", "*bb")]
        [TestCase("aax", "aa*", "a*", "aa*")] // greedy
        public void Matches(string search, string expected, params string[] patterns)
        {
            var m = new MatchingPointPatternMatcher();

            Assert.That(m.Matches(patterns, search), Is.EqualTo(expected));
        }

        [Test]
        public void DuplicateException()
        {
            var m = new MatchingPointPatternMatcher();

            Assert.Throws<ConfigurationException>(() =>
                m.Matches(new[] { "aa*", "*aa" }, "aaa"));
        }
    }
}
