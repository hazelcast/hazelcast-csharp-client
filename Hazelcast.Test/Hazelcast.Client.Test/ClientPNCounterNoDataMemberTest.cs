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

using Hazelcast.Client.Proxy;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("3.10")]
    public class ClientPNCounterNoDataMemberTest : MultiMemberBaseTest
    {
        private ClientPNCounterProxy _pnCounter;

        [SetUp]
        public void Setup()
        {
            _pnCounter = Client.GetPNCounter(TestSupport.RandomString()) as ClientPNCounterProxy;
        }

        protected override void ConfigureGroup(ClientConfig config)
        {
            config.GetGroupConfig().SetName(HzCluster.Id).SetPassword(HzCluster.Id);
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            config.GetNetworkConfig().SetConnectionAttemptLimit(1);
            config.GetNetworkConfig().SetConnectionAttemptPeriod(2000);
        }

        protected override string GetServerConfig()
        {
            return Resources.hazelcast_lite_member;
        }

        [Test]
        public void NoDataMemberExceptionIsThrown()
        {
            Assert.Throws<NoDataMemberInClusterException>(Mutate);
        }

        private void Mutate()
        {
            _pnCounter.AddAndGet(5);
        }
    }
}
