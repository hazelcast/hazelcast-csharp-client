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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Config;
using Hazelcast.Logging;
using Hazelcast.Remote;
using Hazelcast.Transaction;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientRetryTest : HazelcastTestSupport
    {
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

        private IRemoteController _remoteController;
        private Cluster _cluster;
        private readonly int Count = 1000;

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetNetworkConfig().SetRedoOperation(true);
        }

        protected override void ConfigureGroup(ClientConfig config)
        {
            config.SetClusterName(_cluster.Id);
        }

        [Test]
        public void TestClientTransactionRetry()
        {
            Assert.Throws<TransactionException>(() =>
            {
                var member = _remoteController.startMember(_cluster.Id);
                var client = CreateClient();

                var context = client.NewTransactionContext();
                context.BeginTransaction();

                var map = context.GetMap<int, string>(TestSupport.RandomString());

                Task.Factory.StartNew(() =>
                {
                    _remoteController.shutdownMember(_cluster.Id, member.Uuid);
                    _remoteController.startMember(_cluster.Id);
                });
                try
                {
                    for (var i = 0; i < Count; i++)
                    {
                        // put should eventually fail as the node which the transaction is running against 
                        // will be shut down
                        map.Put(i, TestSupport.RandomString());
                    }
                }
                finally
                {
                    context.RollbackTransaction();
                }
            });
        }

        [Test, Ignore("https://github.com/hazelcast/hazelcast-csharp-client/issues/28")]
        public void TestRetryAsyncRequest()
        {
            var member = _remoteController.startMember(_cluster.Id);
            var client = CreateClient();

            var count = 100;
            var member2 = StartMemberAndWait(client, _remoteController, _cluster, 2);
            var map = client.GetMap<int, string>(TestSupport.RandomString());
            for (var i = 0; i < count; i++)
            {
                map.PutAsync(i, TestSupport.RandomString()).IgnoreExceptions();
            }

            _remoteController.shutdownMember(_cluster.Id, member2.Uuid);

            TestSupport.AssertTrueEventually(() =>
            {
                var keys = map.KeySet();
                for (var i = 0; i < count; i++)
                {
                    Assert.IsTrue(keys.Contains(i), "Key " + i + " was not found");
                }
                Assert.AreEqual(count, map.Size());
            }, 60);
        }

        [Test]
        public void TestRetryRequestsWhenInstanceIsShutdown()
        {
            var member = _remoteController.startMember(_cluster.Id);
            var client = CreateClient();

            var member2 = StartMemberAndWait(client, _remoteController, _cluster, 2);
            var map = client.GetMap<int, string>(TestSupport.RandomString());
            for (var i = 0; i < Count; i++)
            {
                map.Put(i, TestSupport.RandomString());
                if (i == Count / 2)
                {
                    StopMember(_remoteController, _cluster, member2);
                }
            }

            TestSupport.AssertTrueEventually(() =>
            {
                var keys = map.KeySet();
                for (var i = 0; i < Count; i++)
                {
                    Assert.True(keys.Contains(i), "Key " + i + " was not found");
                }
                Assert.AreEqual(Count, map.Size());
            }, 60);
        }

        [Test]
        public void TestRetryTimeout()
        {
            var member = _remoteController.startMember(_cluster.Id);
            Environment.SetEnvironmentVariable("hazelcast.client.invocation.timeout.seconds", "2");
            var client = CreateClient();
            try
            {
                var map = client.GetMap<string, string>(TestSupport.RandomString());

                StopMember(_remoteController, _cluster, member);

                var resetEvent = new ManualResetEventSlim();
                Task.Factory.StartNew(() =>
                {
                    Logger.GetLogger("ClientRetryTest").Info("Calling map.Put");
                    map.Put("key", "value");
                }).ContinueWith(t =>
                {
                    var e = t.Exception; //observe the exception
                    if (t.IsFaulted) resetEvent.Set();
                    else Assert.Fail("Method invocation did not fail as expected");
                });

                Assert.True(resetEvent.Wait(4000), "Did not get an exception within seconds");
            }
            finally
            {
                Environment.SetEnvironmentVariable("hazelcast.client.invocation.timeout.seconds", null);
                client.Shutdown();
            }
        }
    }
}