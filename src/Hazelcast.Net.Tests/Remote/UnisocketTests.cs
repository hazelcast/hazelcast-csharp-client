﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    public class UnisocketTests : MultiMembersRemoteTestBase
    {
        [TearDown]
        public async Task RemoveAllMembers()
        {
            foreach (var member in RcMembers)
            {
                await ShutdownMember(member.Key);
            }
        }
        /// <summary>
        /// Port of testClientListener_withDummyClient
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestEventsWithDummyClient()
        {
            var memberA = await AddMember();
            int eventCount = 0;

            var client = await CreateAndStartClientAsync(opt =>
             {
                 opt.Networking.SmartRouting = false;
                 opt.AddSubscriber(events => events.StateChanged((sender, args) =>
                 {
                     HConsole.WriteLine(this, args.State);
                     if (args.State == ClientState.Connected || args.State == ClientState.Shutdown)
                         eventCount++;
                 }));
             });

            await client.DisposeAsync();

            Assert.AreEqual(2, eventCount);
        }

        [Test]
        public async Task TestDistributedObjectEventsWithDummyClient()
        {
            var memberA = await AddMember();
            var memberB = await AddMember();

            int eventCount = 0;

            var client = await CreateAndStartClientAsync(opt =>
            {
                opt.Networking.SmartRouting = false;
            });

            var map = await client.GetMapAsync<int, int>("myMap");
            await map.SubscribeAsync(events => events.EntryAdded((sender, args) => { eventCount++; }));

            await ShutdownMember(Guid.Parse(memberA.Uuid));
            memberA = await AddMember();

            await AssertEx.SucceedsEventually(async () =>
            {
                var script = "instance_0.getMap(\"myMap\").put(1,1)";
                var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Hazelcast.Testing.Remote.Lang.JAVASCRIPT);
                Assert.GreaterOrEqual(1, eventCount);
            }, 10_000, 500);
        }

        /// <summary>
        /// Port of testMemberConnectionOrder
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task TestClientConnectsToOneMember()
        {
            var memberA = await AddMember();
            var memberB = await AddMember();

            var client = await CreateAndStartClientAsync(opt =>
            {
                opt.Networking.SmartRouting = false;
                opt.Networking.ShuffleAddresses = false;
                opt.Networking.Addresses.Clear();
                opt.Networking.Addresses.Add(memberA.Host + ":" + memberA.Port);
                opt.Networking.Addresses.Add(memberB.Host + ":" + memberB.Port);
            });

            var map = await client.GetMapAsync<int, int>("oneMemberMap");
            for (int i = 0; i < 100; i++)
            {
                await map.PutAsync(i, i);
            }

            var script = "result = instance_1.getClientService().getConnectedClients().size().toString();";
            var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Hazelcast.Testing.Remote.Lang.JAVASCRIPT);

            var countOfClients = int.Parse(Encoding.UTF8.GetString(response.Result));

            // instance1 shouldn't have any clients due to unisocket mode
            script = "result = instance_0.getClientService().getConnectedClients().size().toString();";
            response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Hazelcast.Testing.Remote.Lang.JAVASCRIPT);
            countOfClients += int.Parse(Encoding.UTF8.GetString(response.Result));

            //sum should be 1 since don't know which member is connected to
            Assert.AreEqual(1, countOfClients);
        }

    }
}
