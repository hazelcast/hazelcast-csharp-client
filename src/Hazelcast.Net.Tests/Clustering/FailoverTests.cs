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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Partitioning;
using Hazelcast.Security;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Hazelcast.Testing.Configuration;
using static Hazelcast.Testing.Remote.RemoteController;
using Hazelcast.Exceptions;
using Hazelcast.Testing.Remote;

namespace Hazelcast.Tests.Clustering
{
    [Category("enterprise")]
    [Timeout(30_000)]
    internal class FailoverTests : MultipleClusterRemoteTestBase
    {
        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()
                .Configure().SetMinLevel()
                .Configure<HConsoleLoggerProvider>().SetMaxLevel()
                .Configure<FailoverTests>().SetPrefix("TEST").SetMaxLevel()
            );

        const string clusterAName = "clusterA";
        const string clusterBName = "clusterB";

        const string clusterAKey = "keyForClusterA";
        const string clusterAData = "dataForClusterA";

        const string clusterBKey = "keyForClusterB";
        const string clusterBData = "dataForClusterB";

        private static ClusterState MockClusterState
        {
            get
            {
                var options = new HazelcastOptionsBuilder()
                    .Build();

                var mockpartitioner = Mock.Of<Partitioner>();
                var mock = new Mock<ClusterState>(options, "clusterName", "clientName", mockpartitioner, new NullLoggerFactory());
                return mock.Object;
            }
        }

        [Test]
        public void TestFailsWhenEnabledWithNoCluster()
        {
            var options = new HazelcastOptionsBuilder()
                .With("hazelcast.failover.enabled", "true")
                .Build();

            Assert.Throws<Hazelcast.Configuration.ConfigurationException>(delegate { new Failover(MockClusterState, options, new NullLoggerFactory()); });
        }

        [Test]
        public void TestClusterDisconnectedIncreaseCount()
        {
            var options = new HazelcastOptionsBuilder()
                .With("hazelcast.clusterName", "first")
                .With("hazelcast.networking.addresses.0", "123.1.1.1")
                .With("hazelcast.failover.enabled", "true")
                .With("hazelcast.failover.tryCount", "2")
                .With("hazelcast.failover.clusters.0.clusterName", "second")
                .With("hazelcast.failover.clusters.0.networking.addresses.0", "123.1.1.2")
                .Build();

            var failover = new Failover(MockClusterState, options, new NullLoggerFactory());

            Assert.AreEqual(0, failover.CurrentTryCount);

            failover.OnClusterStateChanged(ClientState.Disconnected);
            Assert.AreEqual(1, failover.CurrentTryCount);
            Assert.True(failover.CanSwitchClusterOptions);

            failover.OnClusterStateChanged(ClientState.Disconnected);
            Assert.AreEqual(2, failover.CurrentTryCount);
            Assert.False(failover.CanSwitchClusterOptions);//no right to switch anymore, alredy tried 2 times

            failover.OnClusterStateChanged(ClientState.Connected);
            Assert.AreEqual(0, failover.CurrentTryCount);
            Assert.True(failover.CanSwitchClusterOptions);
        }


        [Test]
        public void TestClusterOptionsRotate()
        {
            const string clusterName1 = "first";
            const string address1 = "1.1.1.1";

            const string clusterName2 = "second";
            const string address2 = "2.2.2.2";
            const string loadBalancer = "ROUNDROBIN";
            const string username = "MARVIN";
            const string password = "GAYE";
            const string hearthBeat = "995";

            int countOfClusterChangedRaised = 0;

            var options = new HazelcastOptionsBuilder()
                .With("hazelcast.clusterName", clusterName1)
                .With("hazelcast.networking.addresses.0", address1)
                .With("hazelcast.failover.enabled", "true")
                .With("hazelcast.failover.tryCount", "2")

                .With("hazelcast.failover.clusters.0.clusterName", clusterName2)
                .With("hazelcast.failover.clusters.0.networking.addresses.0", address2)
                .With("hazelcast.failover.clusters.0.loadBalancer.typeName", loadBalancer)
                .With("hazelcast.failover.clusters.0.authentication.username-password.username", username)
                .With("hazelcast.failover.clusters.0.authentication.username-password.password", password)
                .With("hazelcast.failover.clusters.0.heartbeat.timeoutMilliseconds", hearthBeat)
                .Build();

            var failover = new Failover(MockClusterState, options, new NullLoggerFactory());
            failover.ClusterOptionsChanged += delegate (ClusterOptions currentCluster)
            {
                countOfClusterChangedRaised++;
            };

            void assertForCluster1(Failover failover)
            {
                Assert.AreEqual(clusterName1, failover.CurrentClusterOptions.ClusterName);
                Assert.True(failover.CurrentClusterOptions.Networking.Addresses.Contains(address1));
            };

            void assertForCluster2(Failover failover)
            {
                Assert.AreEqual(clusterName2, failover.CurrentClusterOptions.ClusterName);
                Assert.True(failover.CurrentClusterOptions.Networking.Addresses.Contains(address2));
                Assert.False(failover.CurrentClusterOptions.Networking.Addresses.Contains(address1));
                Assert.IsInstanceOf<RoundRobinLoadBalancer>(failover.CurrentClusterOptions.LoadBalancer.Service);
                //Assert.IsInstanceOf<IPasswordCredentials>(failover.CurrentClusterOptions.Authentication.CredentialsFactory.Service);
                //var credentials = (IPasswordCredentials)failover.CurrentClusterOptions.Authentication.CredentialsFactory.Service;
                //Assert.AreEqual(username, credentials.Name);
                //Assert.AreEqual(password, credentials.Password);
                Assert.AreEqual(int.Parse(hearthBeat), failover.CurrentClusterOptions.Heartbeat.TimeoutMilliseconds);
            };

            //initial one must be cluster 1
            assertForCluster1(failover);

            //try 1
            failover.OnClusterStateChanged(ClientState.Disconnected);
            assertForCluster2(failover);
            Assert.AreEqual(1, countOfClusterChangedRaised);

            //try 2
            failover.OnClusterStateChanged(ClientState.Disconnected);
            assertForCluster1(failover);
            Assert.AreEqual(2, countOfClusterChangedRaised);

            //already tried 2 cluster, so no switching
            failover.OnClusterStateChanged(ClientState.Disconnected);
            assertForCluster1(failover);
            Assert.AreEqual(2, countOfClusterChangedRaised);
        }

        [TestCase(true, 1)]
        [TestCase(false, 1)]
        public async Task TestClientCanFailover(bool useSmartConnection, int memberCount)
        {
            var _ = HConsoleForTest();

            int numberOfStateChanged = 0;

            var options = new HazelcastOptionsBuilder()
                .With((config, opt) =>
                {
                    opt.ClusterName = RcClusterPrimary.Id;
                    opt.Networking.Addresses.Clear();
                    opt.Networking.Addresses.Add("127.0.0.1:5701");
                    opt.Networking.ReconnectMode = Hazelcast.Networking.ReconnectMode.ReconnectAsync;
                    opt.Networking.ConnectionTimeoutMilliseconds = 10_000;
                    opt.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10_000;
                    opt.Networking.SmartRouting = useSmartConnection;

                    opt.Failover.Enabled = true;
                    opt.Failover.TryCount = 2;
                    var clusterAlternative = new ClusterOptions()
                    {
                        ClusterName = RcClusterAlternative.Id
                    };

                    clusterAlternative.Networking.Addresses.Add("127.0.0.1:5703");
                    clusterAlternative.Networking.SmartRouting = useSmartConnection;// that doesn't override to primary
                    clusterAlternative.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 5_000;
                    clusterAlternative.Authentication.CredentialsFactory.Creator = () => new UsernamePasswordCredentialsFactory("test", "1234");

                    opt.Failover.Clusters.Add(clusterAlternative);

                    opt.AddSubscriber(events =>
                    events.StateChanged((sender, arg) =>
                    {
                        HConsole.WriteLine(this, $"State Changed:{ arg.State}");
                        numberOfStateChanged++;
                    }));

                })
                .WithHConsoleLogger()
                .WithUserSecrets(GetType().Assembly)
                .Build();



            //Actual testing

            HConsole.WriteLine(this, "Creating Members");
            var membersA = await StartMembersOn(RcClusterPrimary.Id, 1);
            var membersB = await StartMembersOn(RcClusterAlternative.Id, 1);

            var client = await HazelcastClientFactory.StartNewClientAsync(options);
            string mapName = nameof(FailoverTests);
            var map = await client.GetMapAsync<string, string>(mapName);
            Assert.IsNotNull(map);

            // firs cluster should be A
            await assertClusterA(map, client.ClusterName, options);

            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster A :{RcClusterPrimary.Id}");
            await KillMembersOnAsync(RcClusterPrimary.Id, membersA);

            //give some time to client to change cluster
            await Task.Delay(3_000);

            //Now, we should failover to cluster B
            Assert.AreEqual(ClientState.Connected, client.State);

            await assertClusterB(map, client.ClusterName, options);

            // Start cluster A again
            HConsole.WriteLine(this, $"START: Members of Cluster A :{RcClusterPrimary.Id}");
            membersA = await StartMembersOn(RcClusterPrimary.Id, memberCount);

            // Kill B
            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster B :{RcClusterAlternative.Id}");
            await KillMembersOnAsync(RcClusterAlternative.Id, membersB);

            await Task.Delay(3_000);

            Assert.AreEqual(ClientState.Connected, client.State);
            await assertClusterA(map, client.ClusterName, options);

            Assert.GreaterOrEqual(numberOfStateChanged, 11);
            /*
                Expected State Flow: Due to test enviorment, client can experience more state changes, it's ok.
                0 Starting
                1 Started
                2 Connected
                3 Disconnected
                4 Switching
                5 Switched
                6 Connected
                7 Disconnected
                8 Switching
                9 Switched
                10 Connected
             */

            await KillMembersOnAsync(RcClusterPrimary.Id, membersA);
        }

        [Test]
        public async Task TestClientThrowExceptionOnFailover()
        {
            var _ = HConsoleForTest();

            var options = new HazelcastOptionsBuilder()
                .With((config, opt) =>
                {
                    opt.ClusterName = RcClusterPrimary.Id;
                    opt.Networking.Addresses.Clear();
                    opt.Networking.Addresses.Add("127.0.0.1:5701");
                    opt.Networking.ReconnectMode = Hazelcast.Networking.ReconnectMode.ReconnectAsync;
                    opt.Networking.ConnectionTimeoutMilliseconds = 10_000;
                    opt.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 10_000;
                    opt.Networking.ConnectionRetry.InitialBackoffMilliseconds = 5_000;

                    opt.Failover.Enabled = true;
                    opt.Failover.TryCount = 2;
                    var clusterAlternative = new ClusterOptions()
                    {
                        ClusterName = RcClusterAlternative.Id
                    };

                    clusterAlternative.Networking.Addresses.Add("127.0.0.1:5702");
                    clusterAlternative.Authentication.CredentialsFactory.Creator = () => new UsernamePasswordCredentialsFactory("test", "1234");

                    opt.Failover.Clusters.Add(clusterAlternative);
                })
                .WithHConsoleLogger()
                .WithUserSecrets(GetType().Assembly)
                .Build();

            //Actual testing

            HConsole.WriteLine(this, "Creating Members");
            var membersA = await StartMembersOn(RcClusterPrimary.Id, 1);

            var client = await HazelcastClientFactory.StartNewClientAsync(options);
            string mapName = nameof(FailoverTests);

            var map = await client.GetMapAsync<string, string>(mapName);

            await map.PutAsync(clusterAKey, clusterAData);

            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster A :{RcClusterPrimary.Id}");

            await KillMembersOnAsync(RcClusterPrimary.Id, membersA);

            await Task.Delay(3_000);
            Assert.ThrowsAsync<ClientOfflineException>(async () => await client.GetMapAsync<string, string>(clusterAKey));
        }

        [Test]
        public async Task TestClientCannotFailoverToDifferentPartitionCount()
        {
            var _ = HConsoleForTest();

            bool isLastStateConnected = false;

            var options = new HazelcastOptionsBuilder()
                .With((config, opt) =>
                {
                    opt.ClusterName = RcClusterPrimary.Id;
                    opt.Networking.Addresses.Clear();
                    opt.Networking.Addresses.Add("127.0.0.1:5701");
                    opt.Networking.ReconnectMode = Hazelcast.Networking.ReconnectMode.ReconnectAsync;
                    opt.Networking.ConnectionTimeoutMilliseconds = 10_000;
                    opt.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 20_000;
                    opt.Networking.ConnectionRetry.InitialBackoffMilliseconds = 5_000;

                    opt.Failover.Enabled = true;
                    opt.Failover.TryCount = 2;
                    var clusterAlternative = new ClusterOptions()
                    {
                        ClusterName = RcClusterPartition.Id
                    };

                    clusterAlternative.Networking.Addresses.Add("127.0.0.1:5703");//has different partition count

                    opt.Failover.Clusters.Add(clusterAlternative);

                    opt.AddSubscriber(events =>
                       events.StateChanged((sender, arg) =>
                       {
                           HConsole.WriteLine(this, $"State Changed:{ arg.State}");

                           isLastStateConnected = arg.State == ClientState.Connected;

                       }));
                })
                .WithHConsoleLogger()
                .WithUserSecrets(GetType().Assembly)
                .Build();

            HConsole.WriteLine(this, "Creating Members");
            var membersA = await StartMembersOn(RcClusterPrimary.Id, 1);
            var membersB = await StartMembersOn(RcClusterPartition.Id, 1);

            bool connectionExceptionThrowed = false;

            // Since connections are managed at the backend,
            // cannot cacth the exception with an simple assertion

            var client = await HazelcastClientFactory.StartNewClientAsync(options);
            string mapName = nameof(FailoverTests);

            var map = await client.GetMapAsync<string, string>(mapName);

            await map.PutAsync(clusterAKey, clusterAData);
            await client.GetMapAsync<string, string>(clusterAKey);

            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster A :{RcClusterPrimary.Id}");
            await KillMembersOnAsync(RcClusterPrimary.Id, membersA);

            HConsole.WriteLine(this, $"START: Members of Cluster A :{RcClusterPrimary.Id}");
            membersA = await StartMembersOn(RcClusterPrimary.Id, 1);

            var val = await map.GetAsync(clusterAKey);

            Assert.AreEqual(ClientState.Connected, client.State);
            Assert.True(isLastStateConnected);
        }

        private async Task assertClusterA(IHMap<string, string> map, string currentClusterId, HazelcastOptions options)
        {
            HConsole.WriteLine(this, $"Asserting Cluster A - {RcClusterPrimary.Id}");

            Assert.AreEqual(RcClusterPrimary.Id, currentClusterId);

            await map.PutAsync(clusterAKey, clusterAData);
            var readData = await map.GetAsync(clusterAKey);
            Assert.AreEqual(clusterAData, readData);

            readData = await map.GetAsync(clusterBData);
            Assert.AreNotEqual(clusterBData, readData);
        }

        private async Task assertClusterB(IHMap<string, string> map, string currentClusterId, HazelcastOptions options)
        {
            HConsole.WriteLine(this, $"Asserting Cluster B - {RcClusterAlternative.Id}");

            Assert.AreEqual(RcClusterAlternative.Id, currentClusterId);

            await map.PutAsync(clusterBKey, clusterBData);
            var readData = await map.GetAsync(clusterBKey);
            Assert.AreEqual(clusterBData, readData);

            readData = await map.GetAsync(clusterAKey);
            Assert.AreNotEqual(clusterAData, readData);
        }

    }
}
