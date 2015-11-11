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
using System.Linq;
using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientHeartBeatTest : HazelcastBaseTest
    {
        [TearDown]
        public new void TearDown()
        {
            Cluster.RemoveNode();
            Cluster.AddNode();
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            config.GetNetworkConfig().SetRedoOperation(true);
            base.ConfigureClient(config);
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.timeout", "1000");
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.interval", "1000");
            Environment.SetEnvironmentVariable("hazelcast.client.request.retry.count", "25");
            Environment.SetEnvironmentVariable("hazelcast.client.request.retry.wait.time", "1000");
        }

        [TestFixtureTearDown]
        public void RestoreEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.timeout", null);
            Environment.SetEnvironmentVariable("hazelcast.client.heartbeat.interval", null);
            Environment.SetEnvironmentVariable("hazelcast.client.request.retry.count", null);
            Environment.SetEnvironmentVariable("hazelcast.client.request.retry.wait.time", null);
        }

        //TODO: Intermittently failing test
        [Test, Ignore]
        public void TestHeartBeatStoppedOnOwnerNode()
        {
            var map = Client.GetMap<string, string>(TestSupport.RandomString());

            var key = TestSupport.RandomString();
            var key2 = TestSupport.RandomString();

            var value = TestSupport.RandomString();
            var value2 = TestSupport.RandomString();
            map.Put(key, value);

            var eventCount = 0;
            var regId = map.AddEntryListener(new EntryAdapter<string, string>
            {
                Added = e => Interlocked.Increment(ref eventCount)
            }, key2, false);
            var nodeId = Cluster.NodeIds.First();

            Cluster.SuspendNode(nodeId);
            Thread.Sleep(2000);
            Cluster.ResumeNode(nodeId);

            Assert.That(map.Get(key), Is.EqualTo(value));

            TestSupport.AssertTrueEventually(() =>
            {
                map.Put(key2, value2);
                Assert.IsTrue(eventCount > 0);
            });
        }

        //TODO: Intermittently failing test
        [Test, Ignore]
        public void TestHeartStoppedOnNonOwnerNode()
        {
            var id = AddNodeAndWait();

            var map = Client.GetMap<int, string>(TestSupport.RandomString());
            var count = 50;
            // make sure we have a connection open to the second node
            for (var i = 0; i < count/2; i++)
            {
                map.Put(i, TestSupport.RandomString());
            }
            Cluster.SuspendNode(id);
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
            Thread.Sleep(5000);
            Cluster.ResumeNode(id);

            TestSupport.AssertTrueEventually(() => { Assert.AreEqual(count, map.Size()); });

            RemoveNodeAndWait(id);
        }
    }
}