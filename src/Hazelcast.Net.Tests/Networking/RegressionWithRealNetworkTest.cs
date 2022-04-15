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
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
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

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task TestClientConnectionBeforeServerReady(bool smartRouting)
        {
            var clientTask = CreateAndStartClientAsync(options =>
            {
                options.Networking.SmartRouting = smartRouting;
                options.Networking.ConnectionTimeoutMilliseconds = int.MaxValue;
                options.ClusterName = RcCluster.Id;
            });

            var member = await AddMember();

            var client = await clientTask;

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(ClientState.Connected, client.State);
                Assert.DoesNotThrowAsync(async () => await client.GetMapAsync<int, int>("randomMap"));
            }, 10_000, 500);
        }

        [Test]
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
            //await ClusterOneTimeTearDown();
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
               options.LoggerFactory.Creator = () =>
                   Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                       builder
                           .AddHConsole());
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
            //await ClusterOneTimeTearDown();
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
