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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Remote;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class NonSmartRoutingTest : HazelcastTestSupport
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
            ShutdownCluster(_remoteController, _cluster);
            StopRemoteController(_remoteController);
        }

        protected override void ConfigureGroup(ClientConfig config)
        {
            config.SetClusterName(_cluster.Id);
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetNetworkConfig().SetSmartRouting(false);
        }

        [Test]
        public void TestListenerWithNonSmartRouting()
        {
            var map = _client.GetMap<string, string>(TestSupport.RandomString());

            var keys = TestSupport.RandomArray(TestSupport.RandomString, 10);
            var registrations = new List<Guid>();
            var tasks = new List<Task>();
            foreach (var key in keys)
            {
                var tcs = new TaskCompletionSource<bool>();
                var id = map.AddEntryListener(new EntryAdapter<string, string>
                {
                    Added = e => tcs.SetResult(true)
                }, key, false);
                registrations.Add(id);
                tasks.Add(tcs.Task);
            }

            foreach (var key in keys)
            {
                map.Put(key, TestSupport.RandomString());
            }

            Assert.IsTrue(Task.WaitAll(tasks.ToArray(), 500), "Did not get all entry added events within 500ms");
            foreach (var id in registrations)
            {
                Assert.IsTrue(map.RemoveEntryListener(id));
            }
        }

        [Test]
        public void TestPutAllWithNonSmartRouting()
        {
            var map = _client.GetMap<string, string>(TestSupport.RandomString());
            var n = 1000;
            var toInsert = new Dictionary<string, string>();
            for (var i = 0; i < n; i++)
            {
                toInsert.Add(TestSupport.RandomString(), TestSupport.RandomString());
            }
            map.PutAll(toInsert);

            var resp = map.GetAll(toInsert.Keys);

            foreach (var pair in resp)
            {
                Assert.AreEqual(toInsert[pair.Key] , pair.Value);
            }
        }

        [Test]
        public void TestPutWithNonSmartRouting()
        {
            var cm = ((HazelcastClient) _client).ConnectionManager;
            var map = _client.GetMap<int, int>(TestSupport.RandomString());
            var n = 1000;
            for (var i = 0; i < n; i++)
            {
                map.Put(i, i);
                Assert.AreEqual(1, cm.ActiveConnections.Count);
            }
        }
    }
}