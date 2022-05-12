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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Partitioning;
using Hazelcast.Security;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Hazelcast.Testing.Configuration;
using Hazelcast.Exceptions;


namespace Hazelcast.Tests.Clustering
{
    [Category("enterprise")]
    [Timeout(120_000)]
    internal class FailoverTests : MultipleClusterRemoteTestBase
    {
        private IDisposable HConsoleForTest()


            => HConsole.Capture(options => options
                .ClearAll()
                .Configure().SetMinLevel()
                .Configure<HConsoleLoggerProvider>().SetMaxLevel()
                .Configure<Failover>().SetPrefix("FAILOVER").SetMaxLevel()
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
                return new ClusterState(options, "clusterName", "clientName", mockpartitioner, new NullLoggerFactory());

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
        public void TestClusterOptionsRotate()
        {
            const string clusterName1 = "first";
            const string address1 = "1.1.1.1";

            const string clusterName2 = "second";
            const string address2 = "2.2.2.2";
            const string username = "MARVIN";
            const string password = "GAYE";


            int countOfClusterChangedRaised = 0;

            var options = new HazelcastOptionsBuilder()
                .With("hazelcast.clusterName", clusterName1)
                .With("hazelcast.networking.addresses.0", address1)
                .With("hazelcast.failover.enabled", "true")
                .With("hazelcast.failover.tryCount", "2")
                .With("hazelcast.failover.clusters.0.clusterName", clusterName2)
                .With("hazelcast.failover.clusters.0.networking.addresses.0", address2)
                .With("hazelcast.failover.clusters.0.authentication.username-password.username", username)
                .With("hazelcast.failover.clusters.0.authentication.username-password.password", password)
                .Build();
            var clusterState = MockClusterState;
            var failover = new Failover(clusterState, options, new NullLoggerFactory());
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
            };

            //initial one must be cluster 1
            assertForCluster1(failover);
            clusterState.ChangeState(ClientState.Disconnected);


            //Loop 1, Try 1
            failover.RequestClusterChange();
            assertForCluster2(failover);
            Assert.AreEqual(1, countOfClusterChangedRaised);
            Assert.AreEqual(0, failover.CurrentTryCount);

            //Loop 1, Try 2
            failover.RequestClusterChange();
            assertForCluster1(failover);
            Assert.AreEqual(2, countOfClusterChangedRaised);
            Assert.AreEqual(1, failover.CurrentTryCount);

            //Loop 2, Try 1
            failover.RequestClusterChange();
            assertForCluster2(failover);
            Assert.AreEqual(3, countOfClusterChangedRaised);
            Assert.AreEqual(1, failover.CurrentTryCount);
            
            //Loop 2, Try 2
            failover.RequestClusterChange();
            assertForCluster1(failover);
            Assert.AreEqual(4, countOfClusterChangedRaised);
            Assert.AreEqual(2, failover.CurrentTryCount);

            //Loop 3-> spent all tries
            Assert.False(failover.RequestClusterChange());
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

            // first cluster should be A
            await assertClusterA(map, client.ClusterName, options);

            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster A :{RcClusterPrimary.Id}");
            await KillMembersOnAsync(RcClusterPrimary.Id, membersA);

            //Now, we should failover to cluster B
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Connected, client.State); }, 20_000, 500);
            await assertClusterB(map, client.ClusterName, options);

            // Start cluster A again
            HConsole.WriteLine(this, $"START: Members of Cluster A :{RcClusterPrimary.Id}");
            membersA = await StartMembersOn(RcClusterPrimary.Id, memberCount);

            // Kill B
            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster B :{RcClusterAlternative.Id}");
            await KillMembersOnAsync(RcClusterAlternative.Id, membersB);

            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Connected, client.State); }, 20_000, 500);
            await assertClusterA(map, client.ClusterName, options);

            Assert.GreaterOrEqual(numberOfStateChanged, 8);
            /*
                Expected State Flow: Due to test environment, client can experience more state changes, it's ok.
                0 Starting
                1 Started
                2 Connected
                3 Disconnected                      
                4 Switched
                5 Connected
                6 Disconnected      
                7 Switched
                8 Connected
             */

            await KillMembersOnAsync(RcClusterPrimary.Id, membersA);
        }


        [TestCase(true, 1)]
        [TestCase(false, 1)]
        public async Task TestClientCanFailoverFirstClusterNotUp(bool useSmartConnection, int memberCount)
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
            
            var membersB = await StartMembersOn(RcClusterAlternative.Id, 1);

            var client = await HazelcastClientFactory.StartNewClientAsync(options);
            string mapName = nameof(FailoverTests);
            var map = await client.GetMapAsync<string, string>(mapName);
            Assert.IsNotNull(map);

            // first cluster should be B, A is not up
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Connected, client.State); }, 60_000, 500);            
            await assertClusterB(map, client.ClusterName, options);

            //Start A before switch
            HConsole.WriteLine(this, $"START: Members of Cluster A :{RcClusterPrimary.Id}");
            var membersA = await StartMembersOn(RcClusterPrimary.Id, 1);

            // Kill B
            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster B :{RcClusterAlternative.Id}");
            await KillMembersOnAsync(RcClusterAlternative.Id, membersB);

            //We should be at A
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Connected, client.State); }, 60_000, 500);
            await assertClusterA(map, client.ClusterName, options);

            // Start cluster B again
            HConsole.WriteLine(this, $"START: Members of Cluster B :{RcClusterAlternative.Id}");
            membersB = await StartMembersOn(RcClusterAlternative.Id, memberCount);

            // Kill A
            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster A :{RcClusterPrimary.Id}");
            await KillMembersOnAsync(RcClusterPrimary.Id, membersA);

            //Now, we should failover to cluster B
            await AssertEx.SucceedsEventually(() => { Assert.AreEqual(ClientState.Connected, client.State); }, 60_000, 500);
            await assertClusterB(map, client.ClusterName, options);            

            Assert.GreaterOrEqual(numberOfStateChanged, 8);
            /*
                Expected State Flow: Due to test environment, client can experience more state changes, it's ok.
                0 Starting
                1 Started
                2 Connected
                3 Disconnected                      
                4 Switched
                5 Connected
                6 Disconnected      
                7 Switched
                8 Connected
             */

            await KillMembersOnAsync(RcClusterAlternative.Id, membersB);
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
                    opt.Networking.ConnectionTimeoutMilliseconds = 5_000;
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

            // Since connections are managed at the backend,
            // cannot cacth the exception with an simple assertion

            var client = await HazelcastClientFactory.StartNewClientAsync(options);
            string mapName = nameof(FailoverTests);

            var map = await client.GetMapAsync<string, string>(mapName);

            await map.PutAsync(clusterAKey, clusterAData);
            await client.GetMapAsync<string, string>(clusterAKey);

            HConsole.WriteLine(this, $"SHUTDOWN: Members of Cluster A :{RcClusterPrimary.Id}");
            await KillMembersOnAsync(RcClusterPrimary.Id, membersA);

            //Failover to B and fail due to different partition count            

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
