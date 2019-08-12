// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("3.10")]
    public class ClientPNCounterBasicIntegrationTest : SingleMemberBaseTest
    {
        [Test]
        public void SimpleReplicationTest()
        {
            const string counterName = "counter";
            var counter1 = Client.GetPNCounter(counterName);
            var counter2 = Client.GetPNCounter(counterName);

            Assert.AreEqual(5L, counter1.AddAndGet(5L));

            AssertCounterValueEventually(5L, counter1);
            AssertCounterValueEventually(5L, counter2);
        }

        void AssertCounterValueEventually(long expectedValue, IPNCounter counter)
        {
            TestSupport.AssertTrueEventually(() => { Assert.AreEqual(expectedValue, counter.Get()); });
        }
    }
}
