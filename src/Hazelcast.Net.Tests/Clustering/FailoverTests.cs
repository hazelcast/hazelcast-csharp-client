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
using Hazelcast.Partitioning;
using Hazelcast.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering
{
    internal class FailoverTests
    {
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

            failover.OnClusterDisconnected();
            Assert.AreEqual(1, failover.CurrentTryCount);
            Assert.False(failover.CanSwitchClusterOptions);

            failover.OnClusterDisconnected();
            Assert.AreEqual(2, failover.CurrentTryCount);
            Assert.True(failover.CanSwitchClusterOptions);

            failover.OnClusterDisconnected();
            Assert.AreEqual(0, failover.CurrentTryCount);
            Assert.False(failover.CanSwitchClusterOptions);
        }


        [Test]
        public void TestClusterRotateOptions()
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
                Assert.AreEqual(failover.CurrentClusterOptions.ClusterName, clusterName1);
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
            failover.OnClusterDisconnected();
            assertForCluster1(failover);
            //try 2
            failover.OnClusterDisconnected();
            assertForCluster1(failover);

            //Let's trigger to switch back cluster 1

            //time to switch next cluster
            failover.OnClusterDisconnected();
            assertForCluster2(failover);

            //try 1
            failover.OnClusterDisconnected();
            assertForCluster2(failover);
            //try 2
            failover.OnClusterDisconnected();
            assertForCluster2(failover);

            //cluster 2 failed again, swtich to next which is cluster 1
            failover.OnClusterDisconnected();
            assertForCluster1(failover);

            Assert.AreEqual(2, countOfClusterChangedRaised);
        }
    }
}
