// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    public class RegressionWithRealNetworkTest : MultiMembersRemoteTestBase
    {
        private IDisposable HConsoleForTest()
            => HConsole.Capture(options => options
                .ClearAll()
                .Configure<HConsoleLoggerProvider>().SetPrefix("LOG").SetMaxLevel()
                .Configure().SetMinLevel().EnableTimeStamp(origin: DateTime.Now)
                .Configure(this).SetMaxLevel().SetPrefix("TEST")
            );

        [TearDown]
        public async Task TearDown()
        {
            //since tests are network related may need fresh members on cases.
            await MembersOneTimeTearDown();
        }

        // this test validates that a client that starts connecting to a cluster that is not yet ready,
        // will eventually succeeds and connect once the cluster becomes ready.
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task TestClientConnectionBeforeServerReady(bool smartRouting)
        {
            using var _ = HConsoleForTest();

            var clientTask = CreateAndStartClientAsync(options =>
            {
                options.Networking.SmartRouting = smartRouting;
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = int.MaxValue;
                options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
                options.ClusterName = RcCluster.Id;
            });

            //Let the client tries to connect before cluster is ready
            await Task.Delay(500);

            var member = await AddMember();

            var client = await clientTask;

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(ClientState.Connected, client.State);
            }, 10_000, 500);
        }

        // this test validates that a client can lost its connection to a cluster, and then reconnects,
        // and ends up with one only connection (no leak of the previous connection).
        [Test]
        [Timeout(20_000)]
        [TestCase(true, "localhost", "127.0.0.1")]
        [TestCase(true, "127.0.0.1", "localhost")]
        [TestCase(true, "localhost", "localhost")]
        [TestCase(false, "localhost", "127.0.0.1")]
        [TestCase(false, "127.0.0.1", "localhost")]
        [TestCase(false, "localhost", "localhost")]
        public async Task TestConnectionCountAfterClientReconnect(bool smartRouting, string memberAddress, string clientAddress)
        {
            using var _ = HConsoleForTest();
            #region SetUp
            var memberConfig = GetMemberConfigWithAddress(memberAddress);
            var customCluster = await RcClient.CreateClusterAsync(memberConfig);
            var member = await RcClient.StartMemberAsync(customCluster.Id);
            var client = await CreateAndStartClientAsync(options =>
           {
               options.Networking.SmartRouting = smartRouting;
               options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10_000;
               options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
               options.Networking.Addresses.Clear();
               options.Networking.Addresses.Add(clientAddress + ":" + member.Port);
               options.ClusterName = customCluster.Id;
           });
            #endregion

            var map = await client.GetMapAsync<int, int>("TestMap_" + DateTime.UtcNow.Millisecond);
            var clientImp = (HazelcastClient)client;

            Assert.AreEqual(1, clientImp.Cluster.Connections.Count);
            await map.PutAsync(1, 1);

            await RcClient.ShutdownMemberAsync(customCluster.Id, member.Uuid);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(ClientState.Disconnected, client.State);
            }, 10_000, 500);

            Assert.AreEqual(0, clientImp.Cluster.Connections.Count);

            //Client will reconnect to cluster eventually.
            member = await RcClient.StartMemberAsync(customCluster.Id);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(ClientState.Connected, client.State);
            }, 10_000, 500);

            await map.PutAsync(1, 1);
            Assert.AreEqual(1, clientImp.Cluster.Connections.Count);

            #region TearDown
            await client.DisposeAsync();
            await RcClient.ShutdownClusterAsync(customCluster);
            #endregion

        }


        // this test validates that event handlers (listeners) that were installed, are correctly 
        // installed on a new cluster after the client has disconnected and reconnected.
        [Test]
        [Timeout(40_000)]
        [TestCase(true, "localhost", "127.0.0.1")]
        [TestCase(true, "127.0.0.1", "localhost")]
        [TestCase(true, "localhost", "localhost")]
        [TestCase(false, "localhost", "127.0.0.1")]
        [TestCase(false, "127.0.0.1", "localhost")]
        [TestCase(false, "localhost", "localhost")]
        public async Task TestListenersAfterClientDisconnected(bool smartRouting, string memberAddress, string clientAddress)
        {
            #region SetUp
            using var _ = HConsoleForTest();
            var countOfEvent = 0;
            var memberConfig = GetMemberConfigWithAddress(memberAddress);
            var customCluster = await RcClient.CreateClusterAsync(memberConfig);
            var member = await RcClient.StartMemberAsync(customCluster.Id);

            var client = await CreateAndStartClientAsync(options =>
            {
                options.Networking.Addresses.Clear();
                options.Networking.Addresses.Add(clientAddress);
                options.Networking.SmartRouting = smartRouting;
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = int.MaxValue;
                options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
                options.ClusterName = customCluster.Id;
            });
            #endregion

            var map = await client.GetMapAsync<int, int>("myRandomMap");
            await map.SubscribeAsync(c => c.EntryAdded((sender, args) => Interlocked.Increment(ref countOfEvent)));

            await AssertEx.SucceedsEventually(() =>
            {
                var clientImpl = (HazelcastClient)client;
                Assert.AreEqual(1, clientImpl.Cluster.Connections.Count);
                Assert.AreEqual(1, clientImpl.Members.Count);
            }, 10_000, 500);

            await RcClient.ShutdownMemberAsync(customCluster.Id, member.Uuid);
            await Task.Delay(client.Options.Heartbeat.PeriodMilliseconds);
            member = await RcClient.StartMemberAsync(customCluster.Id);

            await AssertEx.SucceedsEventually(async () =>
            {
                await map.RemoveAsync(1);
                await map.PutAsync(1, 2);
                Assert.Greater(countOfEvent, 0);
            }, 20_000, 500);

            #region TearDown
            await client.DisposeAsync();
            await RcClient.ShutdownClusterAsync(customCluster);
            #endregion
        }

        [Test]
        [TestCase(true, ReconnectMode.ReconnectAsync)]
        [TestCase(true, ReconnectMode.ReconnectSync)]
        [TestCase(false, ReconnectMode.ReconnectAsync)]
        [TestCase(false, ReconnectMode.ReconnectSync)]
        public async Task TestOperationsContinueWhenClientDisconnected(bool smartRouting, ReconnectMode reconnectMode)
        {
            #region SetUp
            using var _ = HConsoleForTest();
            var customCluster = await RcClient.CreateClusterAsync();
            var member1 = await RcClient.StartMemberAsync(customCluster.Id);

            var client = await CreateAndStartClientAsync(options =>
            {
                options.Networking.SmartRouting = smartRouting;
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = int.MaxValue;
                options.Networking.ReconnectMode = reconnectMode;
                options.ClusterName = customCluster.Id;
            });

            var mapName = "myTestingMap";
            var member2 = await RcClient.StartMemberAsync(customCluster.Id);
            #endregion

            await AssertEx.SucceedsEventually(() => Assert.AreEqual(2, client.Members.Count), 10_000, 500);

            var map = await client.GetMapAsync<int, int>(mapName);

            await RcClient.ShutdownMemberAsync(customCluster.Id, member1.Uuid);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(1, client.Members.Count);
                Assert.AreEqual(member2.Host, client.Members.First().Member.Address.Host);
                Assert.AreEqual(member2.Port, client.Members.First().Member.Address.Port);

            }, 20_000, 500);
            //Java test here also toys with partitions to verify that data migrates between members - irrrelevant to us
            await map.PutAsync(1, 2);
            Assert.AreEqual(2, await map.GetAsync(1));

            #region TearDown
            await client.DisposeAsync();
            await RcClient.ShutdownClusterAsync(customCluster);
            #endregion
        }


        private string GetMemberConfigWithAddress(string memberAddress)
        {
            if (string.IsNullOrEmpty(memberAddress)) throw new ArgumentNullException(nameof(memberAddress));

            var xmlDoc = XDocument.Parse(RcClusterConfiguration);
            var ns = xmlDoc.Root.GetDefaultNamespace();
            xmlDoc.Root.Element(ns + "network").Element(ns + "public-address").Value = memberAddress;

            return xmlDoc.ToString();
        }
    }
}
