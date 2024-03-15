﻿// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [TestFixture]
    [ServerVersion("0.1")] // avoid detection
    public class TestConditionIgnoresTest
    {
        private int _count;

        [OneTimeTearDown]
        public void TearDown()
        {
            Assert.That(_count, Is.EqualTo(2), "Some tests did not run?");
        }

        // this test should never execute
        [Test]
        [ServerCondition("[0.3]")]
        public void WouldFail()
        {
            Assert.Fail();
        }

        [Test]
        public void NoConditionSucceeds()
        {
            _count++;
        }

        [Test]
        [ServerCondition("[0.1]")]
        public void ConditionSucceeds()
        {
            _count++;
        }
    }
}
