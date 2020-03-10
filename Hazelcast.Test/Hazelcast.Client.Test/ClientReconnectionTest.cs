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

using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Remote;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture, Ignore("Test failure")]
    internal class ClientReconnectionTest : HazelcastTestSupport
    {
        private static ILogger Logger = Hazelcast.Logging.Logger.GetLogger(typeof(ClientReconnectionTest));
        
        private IRemoteController _remoteController;
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

        [Test]
        public void TestStartClientBeforeMember()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                StartMember(_remoteController, _cluster);
            });

            var client = CreateClient();

            var map = client.GetMap<int, int>(TestSupport.RandomString());

            map.Put(1, 1);
            Assert.AreEqual(1, map.Get(1));
        }

        [Test]
        //TODO Fix required for listener (re)registration 
        public void TestListenerReconnect()
        {
            var member = StartMember(_remoteController, _cluster);
            var client = CreateClient();

            var name = TestSupport.RandomString();
            var map = client.GetMap<string, string>(name);
            var eventCount = 0;
            var count = 2;
            var regId = map.AddEntryListener(
                new EntryAdapter<string, string> {Added = e => { Interlocked.Increment(ref eventCount); }}, true);

            // try to start and stop the instance several times
            for (var i = 0; i < count; i++)
            {
                var clientDisconnected = TestSupport.WaitForClientState(client, LifecycleEvent.LifecycleState.ClientDisconnected);
                _remoteController.shutdownMember(_cluster.Id, member.Uuid);
                TestSupport.AssertCompletedEventually(clientDisconnected, taskName: "clientDisconnected");
                Interlocked.Exchange(ref eventCount, 0);
                var clientConnected = TestSupport.WaitForClientState(client, LifecycleEvent.LifecycleState.ClientConnected);
                member = _remoteController.startMember(_cluster.Id);
                TestSupport.AssertCompletedEventually(clientConnected, taskName: "clientConnected");

                TestSupport.AssertTrueEventually(() =>
                {
                    map.Put(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                    return eventCount > 0;
                });
            }
            Assert.IsTrue(map.RemoveEntryListener(regId));
            map.Destroy();
        }

        [Test]
        public void TestReconnect()
        {
            var member = _remoteController.startMember(_cluster.Id);
            var client = CreateClient();

            var name = TestSupport.RandomString();
            var map = client.GetMap<string, string>(name);
            map.Put("key", "value");
            Assert.AreEqual("value", map.Get("key"));

            var clientDisconnected = TestSupport.WaitForClientState(client, LifecycleEvent.LifecycleState.ClientDisconnected);

            _remoteController.shutdownMember(_cluster.Id, member.Uuid);

            TestSupport.AssertCompletedEventually(clientDisconnected, taskName: "clientDisconnected");

            var clientConnected = TestSupport.WaitForClientState(client, LifecycleEvent.LifecycleState.ClientConnected);

            _remoteController.startMember(_cluster.Id);

            TestSupport.AssertCompletedEventually(clientConnected, taskName: "clientConnected");

            map.Put("key", "value2");
            Assert.AreEqual("value2", map.Get("key"));

            map.Destroy();
        }

        [Test]
        public void TestExtremeReconnect()
        {
            var member = _remoteController.startMember(_cluster.Id);
            var client = CreateClient();
            for (int i = 0; i < 50; i++)
            {
                var clientDisconnected = TestSupport.WaitForClientState(client, 
                    LifecycleEvent.LifecycleState.ClientDisconnected);

                _remoteController.shutdownMember(_cluster.Id, member.Uuid);
                TestSupport.AssertCompletedEventually(clientDisconnected, taskName: $"clientDisconnected-{i}");

                var clientConnected = TestSupport.WaitForClientState(client, 
                    LifecycleEvent.LifecycleState.ClientConnected);

                member = _remoteController.startMember(_cluster.Id);
                TestSupport.AssertCompletedEventually(clientConnected, taskName: $"clientConnected-{i}");
            }
        }
    }
}