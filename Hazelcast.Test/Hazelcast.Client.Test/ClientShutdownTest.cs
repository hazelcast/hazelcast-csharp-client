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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Remote;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientShutdownTest : HazelcastTestSupport
    {
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

        //TODO: This test fails intermittently
        [Ignore("This test fails intermittently")]
        [Test]
        public void TestAsyncOperationDuringClientShutdown()
        {
            Assert.Throws<HazelcastException>(() =>
            {
                var member = _remoteController.startMember(_cluster.Id);
                var client = CreateClient();

                var map = client.GetMap<int, string>(TestSupport.RandomString());

                var count = 100;
                var tasks = new List<Task>();
                var reset = new ManualResetEventSlim();
                Task.Factory.StartNew(() =>
                {
                    for (var i = 0; i < count; i++)
                    {
                        tasks.Add(map.PutAsync(i, TestSupport.RandomString()));
                    }
                    reset.Set();
                });
                Task.Factory.StartNew(() => { client.Shutdown(); });
                try
                {
                    reset.Wait();
                    Assert.IsFalse(Task.WaitAll(tasks.ToArray(), 30 * 1000));
                }
                catch (AggregateException e)
                {
                    throw e.InnerExceptions.First();
                }
            });
        }

        [Test]
        public void TestOperationAfterShutdown()
        {
            Assert.Throws<HazelcastClientNotActiveException>(() =>
            {
                var member = _remoteController.startMember(_cluster.Id);
                var client = CreateClient();

                var map = client.GetMap<int, string>(TestSupport.RandomString());
                for (var i = 0; i < 100; i++)
                {
                    map.Put(i, TestSupport.RandomString());
                }
                client.Shutdown();
                map.Get(0);
            }, "Client is shut down.");
        }

        [Test, Repeat(10)]
        public void TestOperationDuringClientShutdown()
        {
            Assert.Throws<HazelcastClientNotActiveException>(() =>
            {
                var member = _remoteController.startMember(_cluster.Id);
                var client = CreateClient();

                var map = client.GetMap<int, string>(TestSupport.RandomString());
                for (var i = 0; i < 10000; i++)
                {
                    map.Put(i, TestSupport.RandomString());
                    if (i == 0)
                    {
                        Task.Factory.StartNew(() => { client.Shutdown(); });
                    }
                }
            });
        }
    }
}