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

using System.Collections.Generic;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Remote;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientPartitionTest : HazelcastTestSupport
    {
        private RemoteController.Client _remoteController;
        private Cluster _cluster;
        private IHazelcastInstance _client;

        [SetUp]
        public void Setup()
        {
            _remoteController = CreateRemoteController();
            _cluster = CreateCluster(_remoteController);

            StartMember(_remoteController, _cluster);
            _client = CreateClient();

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
            config.GetGroupConfig().SetName(_cluster.Id).SetPassword(_cluster.Id);
        }

        private static HashSet<Address> GetPartitionOwners(int partitionCount, IClientPartitionService partitionService)
        {
            var partitionOwners = new HashSet<Address>();
            for (var i = 0; i < partitionCount; i++)
            {
                partitionOwners.Add(partitionService.GetPartitionOwner(i));
            }
            return partitionOwners;
        }

        [Test]
        public void TestPartitionsUpdatedAfterNewNode()
        {
            var proxy = (HazelcastClientProxy) _client;
            var partitionService = proxy.GetClient().GetClientPartitionService();

            var partitionCount = partitionService.GetPartitionCount();
            Assert.AreEqual(271, partitionCount);

            var owners = GetPartitionOwners(partitionCount, partitionService);
            Assert.AreEqual(1, owners.Count);

            var member = StartMemberAndWait(_client, _remoteController, _cluster, 2);
            try
            {
                TestSupport.AssertTrueEventually(() =>
                {
                    try
                    {
                        owners = GetPartitionOwners(partitionCount, partitionService);
                    }
                    catch (TargetNotMemberException)
                    {
                        Assert.Fail("Partition table is stale.");
                    }
                    Assert.AreEqual(2, owners.Count);
                });
            }
            finally
            {
                StopMemberAndWait(_client, _remoteController, _cluster, member);

                TestSupport.AssertTrueEventually(() =>
                {
                    try
                    {
                        owners = GetPartitionOwners(partitionCount, partitionService);
                    }
                    catch (TargetNotMemberException)
                    {
                        Assert.Fail("Partition table is stale.");
                    }
                    Assert.AreEqual(1, owners.Count);
                }, 60);
            }
        }
    }
}