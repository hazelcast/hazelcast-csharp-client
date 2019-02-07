// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using Hazelcast.Client.Proxy;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture("AddAndGet")]
    [TestFixture("Get")]
    [Category("3.10")]
    public class ClientPNCounterConsistencyLossTest : MultiMemberBaseNoSetupTest
    {
        private readonly string _type;
        private ClientPNCounterProxy _pnCounter;

        public ClientPNCounterConsistencyLossTest(string type)
        {
            _type = type;
        }

        [SetUp]
        public void Setup()
        {
            SetupCluster();
            _pnCounter = Client.GetPNCounter(TestSupport.RandomString()) as ClientPNCounterProxy;
        }

        [TearDown]
        public void TearDown()
        {
            ShutdownRemoteController();
        }

        protected override void InitMembers()
        {
            //Init 2 members
            MemberList.Add(RemoteController.startMember(HzCluster.Id));
            MemberList.Add(RemoteController.startMember(HzCluster.Id));
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            config.GetNetworkConfig().SetConnectionAttemptLimit(1);
            config.GetNetworkConfig().SetConnectionAttemptPeriod(2000);
        }

        protected override string GetServerConfig()
        {
            return Resources.hazelcast_quick_node_switching;
        }

        [Test]
        public void DriverCanContinueSessionByCallingReset()
        {
            _pnCounter.AddAndGet(5);
            Assert.AreEqual(5, _pnCounter.Get());

            TerminateTargetReplicaMember();
            Thread.Sleep(1000);

            _pnCounter.Reset();
            Mutation();
        }

        [Test]
        public void ConsistencyLostExceptionIsThrownWhenTargetReplicaDisappears()
        {
            _pnCounter.AddAndGet(5);
            Assert.AreEqual(5, _pnCounter.Get());

            TerminateTargetReplicaMember();
            Thread.Sleep(1000);

            Assert.Throws<ConsistencyLostException>(Mutation);
        }

        private void Mutation()
        {
            switch (_type)
            {
                case "AddAndGet":
                    _pnCounter.AddAndGet(5);
                    break;

                case "Get":
                    _pnCounter.Get();
                    break;
            }
        }

        private void TerminateTargetReplicaMember()
        {
            // Shutdown "primary" member
            var allMembers = Client.GetCluster().GetMembers();
            var currentTarget = _pnCounter._currentTargetReplicaAddress;
            var primaryMember = allMembers.First(x => x.GetAddress().Equals(currentTarget));

            RemoteController.terminateMember(HzCluster.Id, primaryMember.GetUuid());
        }
    }
}