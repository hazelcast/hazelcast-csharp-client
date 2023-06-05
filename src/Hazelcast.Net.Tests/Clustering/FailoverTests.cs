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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Exceptions;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Partitioner = Hazelcast.Partitioning.Partitioner;

namespace Hazelcast.Tests.Clustering
{
    [Category("enterprise")]
    [Timeout(150_000)]
    internal class FailoverTests : RemoteTestBase
    {
        private const string Key0 = "keyForCluster0";
        private const string Value0 = "valueForCluster0";

        private const string Key1 = "keyForCluster1";
        private const string Value1 = "valueForCluster1";

        private const string Cluster0Address = "127.0.0.1:5701";
        private const string Cluster1Address = "127.0.0.1:5711";
        private const string Cluster2Address = "127.0.0.1:5721";

        private IRemoteControllerClient _rcClient;
        private Hazelcast.Testing.Remote.Cluster _cluster0; // primary cluster
        private Hazelcast.Testing.Remote.Cluster _cluster1; // alternate cluster
        private Hazelcast.Testing.Remote.Cluster _cluster2; // another cluster with a different partition count

        private readonly Dictionary<Hazelcast.Testing.Remote.Cluster, List<Member>> _members
            = new Dictionary<Hazelcast.Testing.Remote.Cluster, List<Member>>();

        private async Task<Hazelcast.Testing.Remote.Cluster> CreateClusterAsync(string config)
        {
            try
            {
                return await _rcClient.CreateClusterAsync(config).CfAwait();
            }
            catch (ServerException e)
            {
                // Thrift exceptions are weird and need to be "fixed"
                e.FixMessage();
                throw;
            }
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _rcClient = await ConnectToRemoteControllerAsync().CfAwait();

            var config0 = TestFiles.ReadAllText(this, "Cluster/default.xml");
            var config1 = TestFiles.ReadAllText(this, "Cluster/default-alt.xml");
            var config2 = TestFiles.ReadAllText(this, "Cluster/default-part.xml");

            _cluster0 = await CreateClusterAsync(config0).CfAwait();
            _cluster1 = await CreateClusterAsync(config1).CfAwait();
            _cluster2 = await CreateClusterAsync(config2).CfAwait();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_rcClient != null)
            {
                if (_cluster0 != null)
                    await _rcClient.ShutdownClusterAsync(_cluster0).CfAwait();

                if (_cluster1 != null)
                    await _rcClient.ShutdownClusterAsync(_cluster1).CfAwait();

                if (_cluster2 != null)
                    await _rcClient.ShutdownClusterAsync(_cluster2).CfAwait();

                await _rcClient.ExitAsync().CfAwait();
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_rcClient != null)
            {
                foreach (var (cluster, members) in _members)
                foreach (var member in members)
                {
                    await _rcClient.ShutdownMemberAsync(cluster.Id, member.Uuid);
                }
            }

            _members.Clear();
        }

        /// <summary>
        /// Starts members on a cluster.
        /// </summary>
        protected async Task<Member[]> StartMembersAsync(Hazelcast.Testing.Remote.Cluster cluster, int count)
        {
            if (!_members.TryGetValue(cluster, out var clusterMembers))
                clusterMembers = _members[cluster] = new List<Member>();

            var members = new Member[count];

            for (var i = 0; i < count; i++)
            {
                members[i] = await _rcClient.StartMemberAsync(cluster.Id);
                clusterMembers.Add(members[i]);
            }

            return members;
        }

        /// <summary>
        /// Kills the specified cluster members.
        /// </summary>
        protected async Task StopMembersAsync(Hazelcast.Testing.Remote.Cluster cluster, Member[] members)
        {
            if (!_members.TryGetValue(cluster, out var clusterMembers))
                clusterMembers = _members[cluster] = new List<Member>();

            foreach (var member in members)
            {
                await _rcClient.ShutdownMemberAsync(cluster.Id, member.Uuid);
                clusterMembers.Remove(member);
            }
        }

        /// <inheritdoc />
        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            if (_cluster0 != null && !string.IsNullOrWhiteSpace(_cluster0.Id))
                options.ClusterName = _cluster0.Id;
            return options;
        }

        private static ClusterState MockClusterState(HazelcastOptions options)
        {
            return new ClusterState(options, "clusterName", "clientName", Mock.Of<Partitioner>(), new NullLoggerFactory());
        }

        [Test]
        public async Task TestFailsWhenEnabledWithNoCluster()
        {
            var failoverOptions = new HazelcastFailoverOptionsBuilder().Build();

            await AssertEx.ThrowsAsync<ConfigurationException>(async () => await HazelcastClientFactory.StartNewFailoverClientAsync(failoverOptions));
        }

        [Test]
        public void TestClusterOptionsRotate()
        {
            const string clusterName1 = "first";
            const string address1 = "1.1.1.1";

            const string clusterName2 = "second";
            const string address2 = "2.2.2.2";
            const string username = "MARVIN";

            var clusterChangedCount = 0;

            var failoverOptions = new HazelcastFailoverOptionsBuilder()
                .With(fo =>
                {
                    fo.TryCount = 2;
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            o.ClusterName = clusterName1;
                            o.Networking.Addresses.Add(address1);
                        })
                        .Build());
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            o.ClusterName = clusterName2;
                            o.Networking.Addresses.Add(address2);
                            o.Authentication.ConfigureUsernamePasswordCredentials(username, "");
                        })
                        .Build());
                })
                .Build();

            // is normally set by HazelcastClientFactory, have to do it here for tests
            failoverOptions.Enabled = true;

            var options = failoverOptions.Clients[0];
            options.FailoverOptions = failoverOptions;

            var clusterState = MockClusterState(options);

            var failover = new Failover(clusterState, options);

            failover.ClusterChanged += _ =>
            {
                clusterChangedCount++;
            };

            void AssertCluster1(Failover fo)
            {
                Assert.AreEqual(clusterName1, fo.CurrentClusterOptions.ClusterName);
                Assert.True(fo.CurrentClusterOptions.Networking.Addresses.Contains(address1));
            }

            void AssertCluster2(Failover fo)
            {
                Assert.AreEqual(clusterName2, fo.CurrentClusterOptions.ClusterName);
                Assert.True(fo.CurrentClusterOptions.Networking.Addresses.Contains(address2));
                Assert.False(fo.CurrentClusterOptions.Networking.Addresses.Contains(address1));
                Assert.AreEqual(fo.CurrentClusterOptions.Authentication.CredentialsFactory.Service.NewCredentials().Name, username);
            }

            // initial one must be cluster 1
            AssertCluster1(failover);
            clusterState.ChangeState(ClientState.Disconnected);

            var expectedCount = 1;

            // Loop 1, Try 1
            Assert.That(failover.TryNextCluster());
            AssertCluster2(failover);
            Assert.AreEqual(expectedCount, clusterChangedCount);
            Assert.AreEqual(expectedCount, failover.CurrentTryCount);
            expectedCount++;

            // Loop 1, Try 2
            Assert.That(failover.TryNextCluster());
            AssertCluster1(failover);
            Assert.AreEqual(expectedCount, clusterChangedCount);
            Assert.AreEqual(expectedCount, failover.CurrentTryCount);
            expectedCount++;

            // Loop 2, Try 1
            Assert.That(failover.TryNextCluster());
            AssertCluster2(failover);
            Assert.AreEqual(expectedCount, clusterChangedCount);
            Assert.AreEqual(expectedCount, failover.CurrentTryCount);
            expectedCount++;

            // Loop 2, Try 2
            Assert.That(failover.TryNextCluster());
            AssertCluster1(failover);
            Assert.AreEqual(expectedCount, clusterChangedCount);
            Assert.AreEqual(expectedCount, failover.CurrentTryCount);

            // Loop 3-> spent all tries
            Assert.False(failover.TryNextCluster());
        }

        [TestCase(true, 1)]
        [TestCase(false, 1)]
        public async Task TestClientCanFailover(bool smartRouting, int memberCount)
        {
            HConsole.Configure(options => options
                .ConfigureDefaults(this)
                .Configure<Failover>().SetPrefix("FAILOVER").SetMaxLevel()
            );

            var states = new ConcurrentQueue<ClientState>();

            var failoverOptions = new HazelcastFailoverOptionsBuilder()
                 .With(fo =>
                 {
                     fo.TryCount = 2;

                     // first cluster is the primary, and able to configure everything
                     fo.Clients.Add(new HazelcastOptionsBuilder()
                         .With(o =>
                         {
                             // this applies to this cluster only
                             o.ClusterName = _cluster0.Id;
                             o.Networking.Addresses.Clear();
                             o.Networking.Addresses.Add(Cluster0Address);
                             o.Networking.ReconnectMode = Hazelcast.Networking.ReconnectMode.ReconnectAsync;
                             o.Networking.SmartRouting = smartRouting;

                             // each single socket connection attempt has a 10s timeout
                             // connection to a cluster has a total timeout of 20s
                             o.Networking.ConnectionTimeoutMilliseconds = 10_000;
                             o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 20_000;

                             // this applies to all clusters
                             o.AddSubscriber(events =>
                                 events.StateChanged((sender, arg) =>
                                 {
                                     HConsole.WriteLine(this, $"State changed to: {arg.State}");
                                     states.Enqueue(arg.State);
                                 }));
                         })
                         .Build());

                     // second cluster is alternate, can only configure network
                     fo.Clients.Add(new HazelcastOptionsBuilder()
                         .With(o =>
                         {
                             o.ClusterName = _cluster1.Id;
                             o.Networking.Addresses.Add(Cluster1Address);
                             o.Networking.SmartRouting = smartRouting; // that doesn't override primary
                         })
                         .Build());
                 })
                 .WithHConsoleLogger()
                 .Build();

            HConsole.WriteLine(this, "Start members of clusters 0 and 1");
            var members0 = await StartMembersAsync(_cluster0, memberCount);
            var members1 = await StartMembersAsync(_cluster1, memberCount);

            HConsole.WriteLine(this, "Start failover client");
            await using var client = await HazelcastClientFactory.StartNewFailoverClientAsync(failoverOptions);
            var mapName = CreateUniqueName();
            var map = await client.GetMapAsync<string, string>(mapName);
            Assert.IsNotNull(map);

            // first cluster should be 0
            AssertState(states, ClientState.Starting);
            AssertState(states, ClientState.Started);
            AssertState(states, ClientState.Connected);
            await AssertCluster(client, _cluster0.Id, map);

            // stop cluster 0 members
            HConsole.WriteLine(this, "Stop members of cluster 0");
            await StopMembersAsync(_cluster0, members0);

            // we should disconnect
            await AssertStateEventually(states, ClientState.Disconnected);

            // we should failover to cluster 1
            await AssertStateEventually(states, ClientState.ClusterChanged);
            await AssertStateEventually(states, ClientState.Connected);
            await AssertCluster(client, _cluster1.Id, map);

            // start cluster 0 members again
            HConsole.WriteLine(this, "Start members of Cluster 0");
            await StartMembersAsync(_cluster0, memberCount);

            // stop cluster 1 members
            HConsole.WriteLine(this, "Stop members of Cluster 1");
            await StopMembersAsync(_cluster1, members1);

            // we should disconnect
            await AssertStateEventually(states, ClientState.Disconnected);

            // we should failover to cluster 0
            await AssertStateEventually(states, ClientState.ClusterChanged);
            await AssertStateEventually(states, ClientState.Connected);
            await AssertCluster(client, _cluster0.Id, map);
        }

        [TestCase(true, 1)]
        [TestCase(false, 1)]
        public async Task TestClientCanFailoverFirstClusterNotUp(bool smartRouting, int memberCount)
        {
            HConsole.Configure(options => options.ConfigureDefaults(this));

            var states = new ConcurrentQueue<ClientState>();

            var failoverOptions = new HazelcastFailoverOptionsBuilder()
                .With(fo =>
                {
                    fo.TryCount = 2;

                    // first cluster is the primary, and able to configure everything
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            // this applies to this cluster only
                            o.ClusterName = _cluster0.Id;
                            o.Networking.Addresses.Clear();
                            o.Networking.Addresses.Add(Cluster0Address);
                            o.Networking.ReconnectMode = Hazelcast.Networking.ReconnectMode.ReconnectAsync;
                            o.Networking.SmartRouting = smartRouting;

                            // each single socket connection attempt has a 10s timeout
                            // connection to a cluster has a total timeout of 20s
                            o.Networking.ConnectionTimeoutMilliseconds = 10_000;
                            o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 20_000;

                            // this applies to all clusters
                            o.AddSubscriber(events =>
                                events.StateChanged((sender, arg) =>
                                {
                                    HConsole.WriteLine(this, $"State changed to: {arg.State}");
                                    states.Enqueue(arg.State);
                                }));
                        })
                        .Build());

                    // second cluster is alternate, can only configure network
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            o.ClusterName = _cluster1.Id;
                            o.Networking.Addresses.Add(Cluster1Address);
                            o.Networking.SmartRouting = smartRouting; // that doesn't override primary
                        })
                        .Build());
                })
                .WithHConsoleLogger()
                .Build();

            HConsole.WriteLine(this, "Start members of clusters 1");
            var members1 = await StartMembersAsync(_cluster1, memberCount);

            HConsole.WriteLine(this, "Start failover client");
            await using var client = await HazelcastClientFactory.StartNewFailoverClientAsync(failoverOptions);
            var mapName = CreateUniqueName();
            var map = await client.GetMapAsync<string, string>(mapName);
            Assert.IsNotNull(map);

            // first cluster should be 1, since 0 is not up
            AssertState(states, ClientState.Starting);
            AssertState(states, ClientState.Started);
            AssertState(states, ClientState.ClusterChanged); // and we failed over
            AssertState(states, ClientState.Connected);
            await AssertCluster(client, _cluster1.Id, map);

            HConsole.WriteLine(this, "Start members of clusters 0");
            var members0 = await StartMembersAsync(_cluster0, memberCount);

            HConsole.WriteLine(this, "Stop members of cluster 1");
            await StopMembersAsync(_cluster1, members1);

            // we should failover to cluster 0
            await AssertStateEventually(states, ClientState.Disconnected);
            await AssertStateEventually(states, ClientState.ClusterChanged);
            await AssertStateEventually(states, ClientState.Connected);
            await AssertCluster(client, _cluster0.Id, map);

            // and again...

            HConsole.WriteLine(this, "Start members of clusters 1");
            await StartMembersAsync(_cluster1, memberCount);

            HConsole.WriteLine(this, "Stop members of cluster 0");
            await StopMembersAsync(_cluster0, members0);

            // we should failover to cluster 1
            await AssertStateEventually(states, ClientState.Disconnected);
            await AssertStateEventually(states, ClientState.ClusterChanged);
            await AssertStateEventually(states, ClientState.Connected);
            await AssertCluster(client, _cluster1.Id, map);
        }

        [Test]

        public async Task TestClientThrowExceptionOnFailover()
        {
            HConsole.Configure(options => options
                .ConfigureDefaults(this)
                .Configure<Failover>().SetPrefix("FAILOVER").SetMaxLevel()
            );

            var failoverOptions = new HazelcastFailoverOptionsBuilder()
                .With(fo =>
                {
                    fo.TryCount = 2;

                    // first cluster is the primary, and able to config everything
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            o.ClusterName = _cluster0.Id;
                            o.Networking.Addresses.Clear();
                            o.Networking.Addresses.Add(Cluster0Address);
                            o.Networking.ReconnectMode = Hazelcast.Networking.ReconnectMode.ReconnectAsync;
                            o.Networking.ConnectionTimeoutMilliseconds = 10_000;
                            o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10_000;
                        })
                        .Build());

                    // cannot override load balancer, retry, heartbeat etc. just network info
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            o.ClusterName = _cluster1.Id;
                            o.Networking.Addresses.Add(Cluster2Address);
                        })
                        .Build());
                })
                .WithHConsoleLogger()
                .Build();

            HConsole.WriteLine(this, "Creating Members");
            var membersA = await StartMembersAsync(_cluster0, 1);

            await using var client = await HazelcastClientFactory.StartNewFailoverClientAsync(failoverOptions);
            var mapName = CreateUniqueName();

            var map = await client.GetMapAsync<string, string>(mapName);
            await map.PutAsync(Key0, Value0);

            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster A :{_cluster0.Id}");

            // kill all members, client will try to fall over and eventually fail and shutdown
            await StopMembersAsync(_cluster0, membersA);

            await AssertEx.SucceedsEventually(async () =>
            {
                try
                {
                    await client.GetMapAsync<string, string>(Key0);
                    throw new Exception("Expected GetMapAsync to fail.");
                }
                catch (ClientOfflineException)
                {
                    // expected
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw; // fail
                }
            }, 60_000, 500);
        }

        [Test]
        public async Task TestClientCannotFailoverToDifferentPartitionCount()
        {
            HConsole.Configure(options => options
                .ConfigureDefaults(this)
                .Configure<Failover>().SetPrefix("FAILOVER").SetMaxLevel()
            );

            var states = new ConcurrentQueue<ClientState>();

            var failoverOptions = new HazelcastFailoverOptionsBuilder()
                .With(fo =>
                {
                    fo.TryCount = 2;

                    // first cluster is the primary, and able to configure everything
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            // this applies to this cluster only
                            o.ClusterName = _cluster0.Id;
                            o.Networking.Addresses.Clear();
                            o.Networking.Addresses.Add(Cluster0Address);
                            o.Networking.ReconnectMode = Hazelcast.Networking.ReconnectMode.ReconnectAsync;

                            // each single socket connection attempt has a 10s timeout
                            // connection to a cluster has a total timeout of 60s
                            o.Networking.ConnectionTimeoutMilliseconds = 10_000;
                            o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10_000;

                            // this applies to all clusters
                            o.AddSubscriber(events =>
                                events.StateChanged((sender, arg) =>
                                {
                                    HConsole.WriteLine(this, $"State changed to: {arg.State}");
                                    states.Enqueue(arg.State);
                                }));
                        })
                        .Build());

                    // second cluster is alternate, can only configure network
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            o.ClusterName = _cluster1.Id;
                            o.Networking.Addresses.Add(Cluster2Address);
                        })
                        .Build());
                })
                .WithHConsoleLogger()
                .Build();

            HConsole.WriteLine(this, "Start members of clusters 0 and 1");
            var members0 = await StartMembersAsync(_cluster0, 1);
            await StartMembersAsync(_cluster1, 1);

            // Since connections are managed at the backend,
            // cannot catch the exception with an simple assertion

            HConsole.WriteLine(this, "Start failover client");
            await using var client = await HazelcastClientFactory.StartNewFailoverClientAsync(failoverOptions);
            var mapName = CreateUniqueName();
            var map = await client.GetMapAsync<string, string>(mapName);
            Assert.IsNotNull(map);

            // first cluster should be 0
            AssertState(states, ClientState.Starting);
            AssertState(states, ClientState.Started);
            AssertState(states, ClientState.Connected);
            await AssertCluster(client, _cluster0.Id, map);

            // stop cluster 0 members
            HConsole.WriteLine(this, "Stop members of cluster 0");
            await StopMembersAsync(_cluster0, members0);

            // we should disconnect
            await AssertStateEventually(states, ClientState.Disconnected);
            Assert.AreEqual(ClientState.Disconnected, client.State);

            // failover to cluster 1 is going to fail because of the different partition count
            // options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10_000;
            // so we need to wait for more than this to make sure we actually tried to failover,
            // and are not simply still trying to reconnect to the original cluster 0 before
            // failover - we test this because the ClusterChanged event *must* trigger here
            HConsole.WriteLine(this, "Wait...");
            await Task.Delay((int)(failoverOptions.Clients[0].Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds + 2_000));

            // start cluster 0 members again
            HConsole.WriteLine(this, "Start members of Cluster 0");
            await StartMembersAsync(_cluster0, 1);

            // we should reconnect to cluster 0
            // *with* triggering ClusterChanged (failover occurred)
            await AssertStateEventually(states, ClientState.ClusterChanged);
            await AssertStateEventually(states, ClientState.Connected);
            await AssertCluster(client, _cluster0.Id, map);
        }

        [Test]
        public async Task TestClientRetryCurrentClusterBeforeFailover()
        {
            HConsole.Configure(options => options
                .ConfigureDefaults(this)
                .Configure<Failover>().SetPrefix("FAILOVER").SetMaxLevel()
            );

            var states = new ConcurrentQueue<ClientState>();

            var failoverOptions = new HazelcastFailoverOptionsBuilder()
                .With(fo =>
                {
                    fo.TryCount = 2;

                    // first cluster is the primary, and able to configure everything
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            // this applies to this cluster only
                            o.ClusterName = _cluster0.Id;
                            o.Networking.Addresses.Clear();
                            o.Networking.Addresses.Add(Cluster0Address);

                            // each single socket connection attempt has a 10s timeout
                            // connection to a cluster has a total timeout of 60s
                            o.Networking.ConnectionTimeoutMilliseconds = 10_000;
                            o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 60_000;

                            // this applies to all clusters
                            o.AddSubscriber(events =>
                                events.StateChanged((sender, arg) =>
                                {
                                    HConsole.WriteLine(this, $"State changed to: {arg.State}");
                                    states.Enqueue(arg.State);
                                }));
                        })
                        .Build());

                    // second cluster is alternate, can only configure network
                    fo.Clients.Add(new HazelcastOptionsBuilder()
                        .With(o =>
                        {
                            o.ClusterName = _cluster1.Id;
                            o.Networking.Addresses.Add(Cluster2Address);
                        })
                        .Build());
                })
                .WithHConsoleLogger()
                .Build();

            HConsole.WriteLine(this, "Start members of clusters 0 and 1");
            var members0 = await StartMembersAsync(_cluster0, 1);
            await StartMembersAsync(_cluster1, 1);

            HConsole.WriteLine(this, "Start failover client");
            await using var client = await HazelcastClientFactory.StartNewFailoverClientAsync(failoverOptions);
            var mapName = CreateUniqueName();
            var map = await client.GetMapAsync<string, string>(mapName);
            Assert.IsNotNull(map);

            // first cluster should be 0
            AssertState(states, ClientState.Starting);
            AssertState(states, ClientState.Started);
            AssertState(states, ClientState.Connected);
            await AssertCluster(client, _cluster0.Id, map);

            // stop cluster 0 members
            HConsole.WriteLine(this, "Stop members of cluster 0");
            await StopMembersAsync(_cluster0, members0);

            // we should disconnect
            await AssertStateEventually(states, ClientState.Disconnected);
            Assert.AreEqual(ClientState.Disconnected, client.State);

            // cluster 1 is live - but we should first try to connect again to cluster 0
            // within o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds
            Assert.AreEqual(_cluster0.Id, client.ClusterName);
            Assert.True(client.Members.Any(x=>
                x.Member.Address.Host==members0[0].Host &&
                x.Member.Address.Port==members0[0].Port)
            );

            // start cluster 0 members again
            HConsole.WriteLine(this, "Start members of Cluster 0");
            await StartMembersAsync(_cluster0, 1);

            // we should reconnect to cluster 0
            // *without* triggering ClusterChanged (no failover)
            await AssertStateEventually(states, ClientState.Connected);
            await AssertCluster(client, _cluster0.Id, map);
        }

        // assert that it's possible to immediately dequeue the expected state from the states queue
        private static void AssertState(ConcurrentQueue<ClientState> states, ClientState expectedState)
        {
            Assert.That(states.TryDequeue(out var state));
            Assert.That(state, Is.EqualTo(expectedState));
        }

        // assert that it's possible to eventually dequeue the expected state from the states queue
        private static async Task AssertStateEventually(ConcurrentQueue<ClientState> states, ClientState expectedState)
        {
            var state = (ClientState) (-1);
            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(states.TryDequeue(out state));
            }, 120_000, 1000);
            Assert.That(state, Is.EqualTo(expectedState));
        }

        // assert that the client is connected to the expected cluster & can access the map
        private async Task AssertCluster(IHazelcastClient client, string expectedClusterId, IHMap<string, string> map)
        {
            Assert.AreEqual(expectedClusterId, client.ClusterName);

            var isCluster0 = client.ClusterName == _cluster0.Id;
            var key = isCluster0 ? Key0 : Key1;
            var value = isCluster0 ? Value0 : Value1;
            var otherKey = isCluster0 ? Key1 : Key0;
            //var otherValue = isCluster0 ? ValueB : ValueA;

            // can use the map
            await map.PutAsync(key, value);
            Assert.AreEqual(value, await map.GetAsync(key));

            // cannot access other value
            Assert.IsNull(await map.GetAsync(otherKey));
        }
    }
}
