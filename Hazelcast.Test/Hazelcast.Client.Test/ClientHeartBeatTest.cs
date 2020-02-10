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

using System;
using System.Linq;
using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Remote;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientHeartBeatTest : HazelcastTestSupport
    {
        private RemoteController.Client _remoteController;
        private Cluster _cluster;

        [SetUp]
        public void Setup()
        {
            _remoteController = CreateRemoteController();
            _cluster = CreateCluster(_remoteController);
        }

        [TearDown]
        public void TearDown()
        {
            HazelcastClient.ShutdownAll();
            StopCluster(_remoteController, _cluster);
            StopRemoteController(_remoteController);
        }
        
        protected override void ConfigureGroup(ClientConfig config)
        {
            config.SetClusterName(_cluster.Id);
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            config.GetNetworkConfig().SetRedoOperation(true);
            config.GetConnectionStrategyConfig().ConnectionRetryConfig.ClusterConnectTimeoutMillis = int.MaxValue;
            base.ConfigureClient(config);
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.timeout", "5000");
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.interval", "1000");
        }

        [OneTimeTearDown]
        public void RestoreEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.timeout", null);
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.interval", null);
        }

        [Test]
        public void TestHeartBeatStoppedOnOwnerNode()
        {
            var member = _remoteController.startMember(_cluster.Id);
            var client = CreateClient();

            var map = client.GetMap<string, string>(TestSupport.RandomString());

            var key = TestSupport.RandomString();

            var value = TestSupport.RandomString();
            var value2 = TestSupport.RandomString();
            map.Put(key, value);

            var eventCount = 0;
            var regId = map.AddEntryListener(new EntryAdapter<string, string>
            {
                Added = e => Interlocked.Increment(ref eventCount)
            }, false);

            SuspendMember(_remoteController, _cluster, member);
            Thread.Sleep(10000);
            ResumeMember(_remoteController, _cluster, member);

            Assert.That(map.Get(key), Is.EqualTo(value));

            TestSupport.AssertTrueEventually(() =>
            {
                map.Put(TestSupport.RandomString(), value2);
                Assert.IsTrue(eventCount > 0);
            });
        }

        [Test]
        public void TestHeartStoppedOnNonOwnerNode()
        {
            var member1 = _remoteController.startMember(_cluster.Id);
            var client = CreateClient();

            var member2 = StartMemberAndWait(client, _remoteController, _cluster, 2);

            var map = client.GetMap<int, string>(TestSupport.RandomString());
            var count = 50;
            // make sure we have a connection open to the second node
            for (var i = 0; i < count/2; i++)
            {
                map.Put(i, TestSupport.RandomString());
            }
            
            SuspendMember(_remoteController, _cluster, member2);

            for (var i = count/2; i < count; i++)
            {
                try
                {
                    map.PutAsync(i, TestSupport.RandomString());
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            }
            Thread.Sleep(10000);

            ResumeMember(_remoteController, _cluster, member2);

            TestSupport.AssertTrueEventually(() => { Assert.AreEqual(count, map.Size()); });
        }
    }
}