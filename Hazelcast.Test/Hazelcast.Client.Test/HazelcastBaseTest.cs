// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class HazelcastBaseTest
    {
        protected static volatile Random random = new Random((int) Clock.CurrentTimeMillis());

        private readonly ILogger logger;


        public HazelcastBaseTest()
        {
            logger = Logger.GetLogger(GetType().Name);
        }

        internal static HazelcastCluster Cluster { get; private set; }
        protected IHazelcastInstance Client { get; set; }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            if (Client != null)
            {
                Client.Shutdown();
            }
            //try to rebalance the cluster size
            if (Cluster.Size > 1)
            {
                while (Cluster.Size != 1)
                {
                    Cluster.RemoveNode();
                }
            }
            if (Cluster.Size == 0)
            {
                Cluster.AddNode();
            }

            Assert.AreEqual(1, Cluster.Size);
        }

        [TestFixtureSetUp]
        public void InitFixture()
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");
            if (Cluster == null)
            {
                Cluster = new HazelcastCluster(1);
                Cluster.Start();
            }
            else
            {
                Assert.AreEqual(1, Cluster.Size);
            }
            Client = CreateClient();
        }

        [SetUp]
        public void LogTestName()
        {
            logger.Finest("Running test " + TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void TearDown()
        {
        }

        protected int AddNodeAndWait()
        {
            var resetEvent = new ManualResetEventSlim();
            var regId = Client.GetCluster().AddMembershipListener(new MembershipListener
            {
                OnMemberAdded = @event => resetEvent.Set()
            });
            var id = Cluster.AddNode();
            Assert.IsTrue(resetEvent.Wait(120*1000), "The member did not get added in 120 seconds");
            Assert.IsTrue(Client.GetCluster().RemoveMembershipListener(regId));

            // make sure partitions are updated
            TestSupport.AssertTrueEventually(() => { Assert.AreEqual(Cluster.Size, GetUniquePartitionOwnerCount()); },
                60, "The partition list did not contain " + Cluster.Size + " partitions.");

            return id;
        }

        protected virtual void ConfigureClient(ClientConfig config)
        {
            config.GetNetworkConfig().AddAddress("127.0.0.1:5701");
            config.GetNetworkConfig().SetConnectionAttemptLimit(20);
            config.GetNetworkConfig().SetConnectionAttemptPeriod(2000);
        }

        protected virtual IHazelcastInstance CreateClient()
        {
            var clientFactory = new HazelcastClientFactory();
            var resetEvent = new ManualResetEventSlim();
            var listener = new ListenerConfig(new LifecycleListener(l =>
            {
                if (l.GetState() == LifecycleEvent.LifecycleState.ClientConnected)
                {
                    resetEvent.Set();
                }
            }));
            var client = clientFactory.CreateClient(c =>
            {
                ConfigureClient(c);
                c.AddListenerConfig(listener);
            });
            Assert.IsTrue(resetEvent.Wait(30*1000), "Client did not start after 30 seconds");
            return client;
        }

        protected int GetUniquePartitionOwnerCount()
        {
            var proxy = ((HazelcastClientProxy) Client);
            var partitionService = proxy.GetClient().GetClientPartitionService();
            var count = partitionService.GetPartitionCount();
            var owners = new HashSet<Address>();
            for (var i = 0; i < count; i++)
            {
                owners.Add(partitionService.GetPartitionOwner(i));
            }
            return owners.Count;
        }

        protected void RemoveNodeAndWait(int id)
        {
            var resetEvent = new ManualResetEventSlim();
            var regId = Client.GetCluster().AddMembershipListener(new MembershipListener
            {
                OnMemberRemoved = @event => resetEvent.Set()
            });
            Cluster.RemoveNode(id);
            Assert.IsTrue(resetEvent.Wait(120*1000), "The member did not get removed in 120 seconds");
            Assert.IsTrue(Client.GetCluster().RemoveMembershipListener(regId));
        }
    }
}