﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    public class ClientPNCounterConsistencyLossTest : MultiMemberBaseNoSetupTest
    {
        private readonly string _type;
        private ClientPNCounterProxy _pnCounter;

        public ClientPNCounterConsistencyLossTest(string type)
        {
            _type = type;
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            StopRemoteController(RemoteController);
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
            RemoteController.shutdownCluster(HzCluster.Id);
        }

        protected override void InitMembers()
        {
            //Init 2 members
            StartNewMember();
            StartNewMember();
        }

        protected override void ConfigureClient(Configuration config)
        {
            var cs = config.ConnectionStrategyConfig;
            cs.AsyncStart = false;
            cs.ReconnectMode = ReconnectMode.OFF;
            cs.ConnectionRetryConfig.ClusterConnectTimeoutMillis = 2000;
            cs.ConnectionRetryConfig.InitialBackoffMillis = 2000;
        }

        protected override string GetServerConfig()
        {
            return Resources.HazelcastCrdtReplication;
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
            var allMembers = Client.Cluster.Members;
            var currentTarget = _pnCounter._currentTargetReplicaAddress;
            var primaryMember = allMembers.First(x => x.Equals(currentTarget));

            RemoteController.terminateMember(HzCluster.Id, primaryMember.Uuid.ToString());
        }
    }
}