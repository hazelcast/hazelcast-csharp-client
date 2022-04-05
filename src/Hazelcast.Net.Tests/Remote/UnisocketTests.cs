using System;
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
        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()
                .Configure().SetMaxLevel()
                .Configure(this).SetPrefix("TEST")
                .Configure<UnisocketTests>().SetIndent(8).SetPrefix("UNISOCKET"));

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
            HConsoleForTest();
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
                opt.Networking.Addresses.Clear();
                opt.Networking.Addresses.Add(memberA.Host + ":" + memberA.Port);
                opt.Networking.Addresses.Add(memberB.Host + ":" + memberB.Port);
            });

            var script = "result = instance_0.getClientService().getConnectedClients().size().toString();";
            var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Hazelcast.Testing.Remote.Lang.JAVASCRIPT);

            var countOfClients = int.Parse(Encoding.UTF8.GetString(response.Result));
            Console.WriteLine("CLIENT COUNT " + countOfClients);
            Assert.AreEqual(1, countOfClients);

            // instance1 shouldn't have any clients due to unisocket mode
            script = "result = instance_1.getClientService().getConnectedClients().size().toString();";
            response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Hazelcast.Testing.Remote.Lang.JAVASCRIPT);
            countOfClients = int.Parse(Encoding.UTF8.GetString(response.Result));

            Assert.AreEqual(0, countOfClients);
        }

    }
}
