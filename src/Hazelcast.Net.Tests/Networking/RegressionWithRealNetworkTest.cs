// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    public class RegressionWithRealNetworkTest : RemoteTestBase
    {
        private IRemoteControllerClient _rcClient;
        private readonly ConcurrentDictionary<Guid, Member> _members = new ConcurrentDictionary<Guid, Member>();
        private Cluster _cluster;

        private IDisposable HConsoleForTest()
            => HConsole.Capture(options => options
                .ClearAll()
                .Configure<HConsoleLoggerProvider>().SetPrefix("LOG").SetMaxLevel()
                .Configure().SetMinLevel().EnableTimeStamp(origin: DateTime.Now)
                .Configure(this).SetMaxLevel().SetPrefix("TEST")
            );

        protected override HazelcastOptionsBuilder CreateHazelcastOptionsBuilder()
        {
            return base.CreateHazelcastOptionsBuilder().WithHConsoleLogger();
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            // create remote client and cluster
            _rcClient = await ConnectToRemoteControllerAsync().CfAwait();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_rcClient != null)
                await _rcClient.ExitAsync().CfAwait();
        }


        [TearDown]
        public async Task TearDown()
        {
            // since tests are network related, require new members for each case
            foreach (var memberId in _members.Keys)
                await RemoveMember(memberId);

            // in case we started a custom cluster (with custom config) stop it
            if (_cluster != null)
            {
                await _rcClient.ShutdownClusterAsync(_cluster.Id);
                _cluster = null;
            }
        }

        protected async Task StartCluster(string configuration)
        {
            try
            {
                _cluster = await _rcClient.CreateClusterAsync(configuration).CfAwait();
            }
            catch (ServerException e)
            {
                // Thrift exceptions are weird and need to be "fixed"
                e.FixMessage();
                throw;
            }
        }

        protected async Task<Member> AddMember()
        {
            var member = await _rcClient.StartMemberAsync(_cluster);
            _members[Guid.Parse(member.Uuid)] = member;
            return member;
        }

        protected Task RemoveMember(string memberId)
            => RemoveMember(Guid.Parse(memberId));

        protected async Task RemoveMember(Guid memberId)
        {
            if (_members.TryRemove(memberId, out var member))
                await _rcClient.StopMemberAsync(_cluster, member);
        }

        // this test validates that a client that starts connecting to a cluster that is not yet ready,
        // will eventually succeeds and connect once the cluster becomes ready.
        [Test]
        [TestCase(true, ReconnectMode.ReconnectAsync)]
        [TestCase(true, ReconnectMode.ReconnectSync)]
        [TestCase(false, ReconnectMode.ReconnectAsync)]
        [TestCase(false, ReconnectMode.ReconnectSync)]
        public async Task TestClientConnectionBeforeServerReady(bool smartRouting, ReconnectMode reconnectMode)
        {
            using var _ = HConsoleForTest();

            await StartCluster(Hazelcast.Testing.Remote.Resources.hazelcast);

            var clientStart = CreateStartingClientAsync(options =>
            {
                options.Networking.SmartRouting = smartRouting;
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = int.MaxValue;
                options.Networking.ReconnectMode = reconnectMode;
                options.ClusterName = _cluster.Id;
            });

            await using var client = clientStart.Client;

            // let the client tries to connect before cluster is ready
            await AssertEx.SucceedsEventually(() =>
                Assert.That(client.State, Is.EqualTo(ClientState.Started)),
                30_000, 1_000);

            // add a member to the cluster = will become ready
            await AddMember();

            // wait for the client to connect *or* time out
            // (when the task completes, the client *is* connected)
            await clientStart.Task.CfAwait(60_000);
            Assert.AreEqual(ClientState.Connected, client.State);
        }

        // this test validates that a client can lose its connection to a cluster, and then reconnects,
        // and ends up with one only connection (no leak of the previous connection).
        [Test]
        [Timeout(20_000)]
        [TestCase(true, "localhost", "127.0.0.1")]
        [TestCase(true, "127.0.0.1", "localhost")]
        [TestCase(true, "localhost", "localhost")]
        [TestCase(true, "127.0.0.1", "127.0.0.1")]
        [TestCase(false, "localhost", "127.0.0.1")]
        [TestCase(false, "127.0.0.1", "localhost")]
        [TestCase(false, "localhost", "localhost")]
        [TestCase(false, "127.0.0.1", "127.0.0.1")]
        public async Task TestConnectionCountAfterClientReconnect(bool smartRouting, string memberAddress, string clientAddress)
        {
            using var _ = HConsoleForTest();

            await StartCluster(GetMemberConfigWithAddress(memberAddress));
            var member = await AddMember();
            await using var client = await CreateAndStartClientAsync(options =>
            {
                options.Networking.SmartRouting = smartRouting;
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = long.MaxValue;
                options.Networking.ReconnectMode = ReconnectMode.ReconnectSync;
                options.Networking.Addresses.Clear();
                options.Networking.Addresses.Add(clientAddress + ":" + member.Port);
                options.ClusterName = _cluster.Id;
            });
            var clientImp = (HazelcastClient)client;

            Assert.AreEqual(1, clientImp.Cluster.Connections.Count);

            await RemoveMember(member.Uuid);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(ClientState.Disconnected, client.State);
            }, 10_000, 500);

            Assert.AreEqual(0, clientImp.Cluster.Connections.Count);

            // client will reconnect to cluster eventually.
            await AddMember();

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(ClientState.Connected, client.State);
            }, 10_000, 500);


            Assert.AreEqual(1, clientImp.Cluster.Connections.Count);
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
            using var _ = HConsoleForTest();

            var countOfEvent = 0;
            await StartCluster(GetMemberConfigWithAddress(memberAddress));
            var member = await AddMember();

            await using var client = await CreateAndStartClientAsync(options =>
            {
                options.Networking.Addresses.Clear();
                options.Networking.Addresses.Add(clientAddress);
                options.Networking.SmartRouting = smartRouting;
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = int.MaxValue;
                options.Networking.ReconnectMode = ReconnectMode.ReconnectSync;
                options.ClusterName = _cluster.Id;
                options.Heartbeat.TimeoutMilliseconds = 6_000;
            });

            await AssertEx.SucceedsEventually(() =>
                    Assert.That(client.State, Is.EqualTo(ClientState.Connected)),
                60_000, 1000);

            var map = await client.GetMapAsync<int, int>("myRandomMap");
            await map.SubscribeAsync(c => c.EntryAdded((sender, args) => Interlocked.Increment(ref countOfEvent)));

            await RemoveMember(member.Uuid);
            await AssertEx.SucceedsEventually(() =>
                    Assert.That(client.State, Is.EqualTo(ClientState.Disconnected)),
                120_000, 1000);

            await AddMember();

            await AssertEx.SucceedsEventually(async () =>
            {
                await map.RemoveAsync(1);
                await map.PutAsync(1, 2);
                Assert.Greater(countOfEvent, 0);
            }, 120_000, 100);
        }

        [Test]
        [TestCase(true, ReconnectMode.ReconnectAsync)]
        [TestCase(true, ReconnectMode.ReconnectSync)]
        [TestCase(false, ReconnectMode.ReconnectAsync)]
        [TestCase(false, ReconnectMode.ReconnectSync)]
        public async Task TestOperationsContinueWhenClientDisconnected(bool smartRouting, ReconnectMode reconnectMode)
        {
            using var _ = HConsoleForTest();
            await StartCluster(Hazelcast.Testing.Remote.Resources.hazelcast);
            var member1 = await AddMember();

            await using var client = await CreateAndStartClientAsync(options =>
            {
                options.Networking.SmartRouting = smartRouting;
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = int.MaxValue;
                options.Networking.ReconnectMode = reconnectMode;
                options.ClusterName = _cluster.Id;
            });

            var mapName = "myTestingMap";
            var member2 = await AddMember();

            await AssertEx.SucceedsEventually(() =>
                Assert.AreEqual(2, client.Members.Count),
                120_000, 1000);
            await AssertEx.SucceedsEventually(() =>
                Assert.That(client.Members.Count(x => x.IsConnected), Is.EqualTo(smartRouting ? 2 : 1)),
                60_000, 1000); // non-smart: only 1 member is connected

            var map = await client.GetMapAsync<int, int>(mapName);

            await RemoveMember(member1.Uuid);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(1, client.Members.Count);
                Assert.AreEqual(member2.Host, client.Members.First().Member.Address.Host);
                Assert.AreEqual(member2.Port, client.Members.First().Member.Address.Port);

            }, 120_000, 500);

            //Java test here also toys with partitions to verify that data migrates between members - irrelevant to us
            //details: https://github.com/hazelcast/hazelcast-csharp-client/pull/614
            await map.PutAsync(1, 2);
            Assert.AreEqual(2, await map.GetAsync(1));
        }

        
        [Test]
        public async Task TestReconnectModes([Values] ReconnectMode mode)
        {
  
            await StartCluster(Hazelcast.Testing.Remote.Resources.hazelcast);
            var member1 = await AddMember();
            await using var client = await CreateAndStartClientAsync(options =>
            {
                options.Networking.ReconnectMode = mode;
                options.ClusterName = _cluster.Id;
            });

            await AssertEx.SucceedsEventually(() =>
                {
                    Assert.AreEqual(1, client.Members.Count);
                    Assert.AreEqual(ClientState.Connected,client.State);
                }, 
                120_000, 1000);
            
            // Shutdown member
            await RemoveMember(member1.Uuid);

            if (mode == ReconnectMode.DoNotReconnect)
            {
                await AssertEx.SucceedsEventually(() =>
                    {
                        //No need to check member count, client will go to shutdown directly.
                        Assert.AreEqual(ClientState.Shutdown,client.State);
                        Assert.ThrowsAsync<ClientOfflineException>(async () => await client.GetMapAsync<int, int>("someMap"));
                    }, 120_000, 1000);
            }

            // Client will reconnect. FIXME: Separate sync and async modes when implemented. 
            else
            {
                member1 = await AddMember();
                await AssertEx.SucceedsEventually(() =>
                    {
                        Assert.AreEqual(1, client.Members.Count);
                        Assert.AreEqual(ClientState.Connected,client.State);
                    }, 120_000, 1000);
                
                await RemoveMember(member1.Uuid);    
            }

            await client.DisposeAsync();
        }
        
        private static string GetMemberConfigWithAddress(string memberAddress)
        {
            if (string.IsNullOrEmpty(memberAddress)) throw new ArgumentNullException(nameof(memberAddress));

            var xmlDoc = XDocument.Parse(Hazelcast.Testing.Remote.Resources.hazelcast);
            Assert.That(xmlDoc, Is.Not.Null);
            Assert.That(xmlDoc.Root, Is.Not.Null);
            var ns = xmlDoc.Root.GetDefaultNamespace();
            var eltNetwork = xmlDoc.Root.Element(ns + "network");
            Assert.That(eltNetwork, Is.Not.Null);
            var eltPublicAddress = eltNetwork.Element(ns + "public-address");
            Assert.That(eltPublicAddress, Is.Not.Null);
            eltPublicAddress.Value = memberAddress;

            return xmlDoc.ToString();
        }
    }
}
