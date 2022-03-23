// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Testing;
using NUnit.Framework;
using Hazelcast.Core;
using Hazelcast.Testing.Remote;
using System.Threading;

namespace Hazelcast.Tests.Networking
{
    internal class NetworkHostnameTests : RemoteTestBase
    {
        public IRemoteControllerClient RcClient { get; set; }

        public Cluster RcCluster { get; set; }

        [OneTimeSetUp]
        public async Task ClusterOneTimeSetUp()
        {
            // create remote client and cluster
            RcClient = await ConnectToRemoteControllerAsync().CfAwait();
            RcCluster = await RcClient.CreateClusterAsync(Hazelcast.Testing.Remote.Resources.hazelcast_hostname).CfAwait();
        }

        [OneTimeTearDown]
        public async Task ClusterOneTimeTearDown()
        {
            // terminate & remove client and cluster
            if (RcClient != null)
            {
                if (RcCluster != null)
                    await RcClient.ShutdownClusterAsync(RcCluster).CfAwait();
                await RcClient.ExitAsync().CfAwait();
            }
        }

        [Test]
        public async Task ConnectWithDNSHostnames()
        {
            var memberA = await RcClient.StartMemberAsync(RcCluster);

            var client = await CreateAndStartClientAsync(opt =>
            {
                opt.Networking.Addresses.Clear();
                opt.Networking.Addresses.Add("localhost");
                opt.ClusterName = RcCluster.Id;
            });

            bool eventTriggered = false;
            _ = client.SubscribeAsync(events => events.ConnectionOpened((sender, args) =>
                {
                    Assert.AreEqual(2, sender.Members.Count);
                    eventTriggered = true;
                }));

            var memberB = await RcClient.StartMemberAsync(RcCluster);

            await AssertEx.SucceedsEventually(async () =>
                {
                    Assert.AreEqual(2, client.Members.Count);

                    if (client.Members.Count == 2)
                        await RcClient.ShutdownMemberAsync(RcCluster.Id, memberB.Uuid);

                }, 10_000, 500);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(1, client.Members.Count);

            }, 10_000, 500);

            Assert.IsTrue(eventTriggered);
        }

        [Test]
        public async Task ListenersWorkingWhenDNSHostnamesAreUsed()
        {
            int countOfAddedEntries = 0;
            int hearbeatPeriod = 5_000;

            var memberA = await RcClient.StartMemberAsync(RcCluster);

            var client = await CreateAndStartClientAsync(opt =>
            {
                opt.Networking.Addresses.Clear();
                opt.Networking.Addresses.Add("localhost");
                opt.ClusterName = RcCluster.Id;
                opt.Heartbeat.PeriodMilliseconds = hearbeatPeriod;
            });

            var map = await client.GetMapAsync<string, string>("testMap");

            _ = map.SubscribeAsync(events => events.EntryAdded((sender, args) =>
            {
                Interlocked.Increment(ref countOfAddedEntries);
            }));

            var memberB = await RcClient.StartMemberAsync(RcCluster);

            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.AreEqual(2, client.Members.Count);
            }, 10_000, 500);

            await RcClient.ShutdownMemberAsync(RcCluster.Id, memberB.Uuid);
            await Task.Delay(hearbeatPeriod * 2);

            await AssertEx.SucceedsEventually(async () =>
            {
                await map.PutAsync("testKey", "testValue");
                Assert.AreEqual(1, Interlocked.CompareExchange(ref countOfAddedEntries, 1, 1));
            }, 10_000, 500);
        }

    }
}
