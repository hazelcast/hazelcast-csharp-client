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
