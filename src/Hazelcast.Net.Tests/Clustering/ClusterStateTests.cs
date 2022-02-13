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
using Hazelcast.Clustering;
using Hazelcast.Networking;
using Hazelcast.Partitioning;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Clustering
{
    internal class ClusterStateTests
    {
        private ClusterState CreateClusterState(string clusterName1 = "first", string clusterName2 = "second", string address1 = "127.0.0.1", string address2 = "127.0.0.2")
        {
            var mockPartitioner = Mock.Of<Partitioner>();
            var options = new HazelcastOptionsBuilder()
               .With("hazelcast.clusterName", clusterName1)
               .With("hazelcast.networking.addresses.0", address1)
               .With("hazelcast.failover.enabled", "true")
               .With("hazelcast.failover.tryCount", "2")
               .With("hazelcast.failover.clusters.0.clusterName", clusterName2)
               .With("hazelcast.failover.clusters.0.networking.addresses.0", address2)
               .Build();

            var clusterState = new ClusterState(options, clusterName1, "hzclient", mockPartitioner, new NullLoggerFactory());

            return clusterState;
        }

        [Test]
        public void TestAddressProviderChangesOnFailover()
        {
            const string clusterName1 = "first";
            const string clusterName2 = "second";
            const string address1 = "127.0.0.1";
            const string address2 = "127.0.0.2";
            int countOfStateChanged = 0;

            var clusterState = CreateClusterState(clusterName1, clusterName2, address1, address2);

            void verifyClusterIs(ClusterState state, string clusterName, string address)
            {
                Assert.AreEqual(clusterName, state.CurrentClusterOptions.ClusterName);
                Assert.True(state.CurrentClusterOptions.Networking.Addresses.Contains(address));

                Assert.AreEqual(clusterName, clusterState.ClusterName);

                var addresses = clusterState.AddressProvider.GetAddresses();
                //Console.WriteLine(String.Join(", ", addresses.Select(p => p.HostName + ":" + p.Port).ToArray()));

                Assert.That(addresses.Count(), Is.EqualTo(3));//yes 3, because address provider appends ports(5701,5702,5703) for each address with no port.

                var networkAddress = new NetworkAddress(address, 5701);
                Assert.True(addresses.Contains(networkAddress));
            };
            
            clusterState.StateChanged += x =>
            {
                countOfStateChanged++;
                //Console.WriteLine(x.ToString());
                return default;
            };

            //assume that client connected to first cluster
            clusterState.ChangeState(ClientState.Connected);
            Assert.That(countOfStateChanged, Is.EqualTo(1));

            // Verify cluster is first
            verifyClusterIs(clusterState, clusterName1, address1);

            //failover to next-> cluster 2
            clusterState.ChangeState(ClientState.Disconnected);//disconnected->switching->switched
            verifyClusterIs(clusterState, clusterName2, address2);
            Assert.AreEqual(ClientState.Switched, clusterState.ClientState);
            Assert.That(countOfStateChanged, Is.EqualTo(4));

            //failover to next-> cluster 1
            clusterState.ChangeState(ClientState.Disconnected);//disconnected->switching->switched
            verifyClusterIs(clusterState, clusterName1, address1);
            Assert.AreEqual(ClientState.Switched, clusterState.ClientState);
            Assert.That(countOfStateChanged, Is.EqualTo(7));

            //cannot failover anymore, still cluster 1
            clusterState.ChangeState(ClientState.Disconnected);//disconnected.
            verifyClusterIs(clusterState, clusterName1, address1);

            Assert.AreEqual(ClientState.Disconnected, clusterState.ClientState);
            Assert.That(countOfStateChanged, Is.EqualTo(8));
        }
    }
}
