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
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Remote;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientHeartBeatDetailTest : HazelcastTestSupport
    {
        private IRemoteController _remoteController;
        private Cluster _cluster;

        [SetUp]
        public void Setup()
        {
            _remoteController = CreateRemoteController();
            _cluster = CreateCluster(_remoteController, GetServerConfig());
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
            config.NetworkConfig.RedoOperation = true;
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

        private string GetServerConfig()
        {
            return Resources.HazelcastHb;
        }

        [Test]
        public void TestIdle()
        {
            _remoteController.startMember(_cluster.Id);
            _remoteController.startMember(_cluster.Id);
            var client = CreateClient();
            var clientDisconnected = TestSupport.WaitForClientState(client, LifecycleEvent.LifecycleState.ClientDisconnected);

            Thread.Sleep(10000);
            Assert.False(clientDisconnected.Wait(1000), "Client should not be disconnected");
        }
        
        [Test]
        public void TestContinuousGet()
        {
            _remoteController.startMember(_cluster.Id);
            _remoteController.startMember(_cluster.Id);
            var client = CreateClient();
            var clientDisconnected = TestSupport.WaitForClientState(client, LifecycleEvent.LifecycleState.ClientDisconnected);

            var map = client.GetMap<string, string>(TestSupport.RandomString());
            map.Put("key", "value");

            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                map.Get("key");
            }
            Assert.False(clientDisconnected.Wait(1000), "Client should not be disconnected");
        }

        // TODO: bring it back for regular framework
#if NETCOREAPP2
        [Test]
        public void TestContinuousGC()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            _remoteController.startMember(_cluster.Id);
            _remoteController.startMember(_cluster.Id);
            var client = CreateClient();
            var clientDisconnected = TestSupport.WaitForClientState(client, LifecycleEvent.LifecycleState.ClientDisconnected);

            var map = client.GetMap<string, string>(TestSupport.RandomString());
            map.Put("key", "value");

            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                GC.Collect(9, GCCollectionMode.Forced, true, true);
                Thread.Sleep(1000);
            }
            Assert.False(clientDisconnected.Wait(10000), "Client should not be disconnected");
        }
#endif
    }
}