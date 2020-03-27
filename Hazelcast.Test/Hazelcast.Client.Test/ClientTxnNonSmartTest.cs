// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Remote;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class ClientTxnNonSmartTest: HazelcastTestSupport
    {
        private IRemoteController _remoteController;
        private Cluster _cluster;
        private IHazelcastInstance _client;

        [SetUp]
        public void Setup()
        {
            _remoteController = CreateRemoteController();
            _cluster = CreateCluster(_remoteController);
            
            StartMember(_remoteController, _cluster);
            StartMember(_remoteController, _cluster);
            _client = CreateClient();

        }

        [TearDown]
        public void TearDown()
        {
            HazelcastClient.ShutdownAll();
            StopCluster(_remoteController, _cluster);
            StopRemoteController(_remoteController);
        }

        protected override void ConfigureGroup(Configuration config)
        {
            config.ClusterName = _cluster.Id;
        }

        protected override void ConfigureClient(Configuration config)
        {
            base.ConfigureClient(config);
            config.NetworkConfig.SmartRouting = false;
        }

        [Test]
        public void TestListenerWithNonSmartRouting()
        {
            var cm  = ((HazelcastClient) _client).ConnectionManager;
            for (int i = 0; i < 100; i++)
            {
                var options = new TransactionOptions().SetTransactionType(TransactionOptions.TransactionType.TwoPhase);
                var context = _client.NewTransactionContext(options);

                context.BeginTransaction();
                var tmap = context.GetMap<int, string>("test");
                var tmap2 = context.GetMap<int, string>("test2");

                tmap.Set(i, "value");
                tmap2.Set(i, "value");
                context.CommitTransaction();
                Assert.AreEqual(1, cm.ActiveConnections.Count);
            }
        }

    }
}