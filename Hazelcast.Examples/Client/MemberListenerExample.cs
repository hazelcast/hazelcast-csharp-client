using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Examples.Client
{
    class MemberListenerExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");

            var reset = new ManualResetEventSlim();
            config.AddListenerConfig(new ListenerConfig(new MembershipListener()
            {
                OnMemberAdded = m =>
                {
                    Console.WriteLine("Added member: " + m.GetMember());
                    reset.Set();
                }
            }));
            var client = HazelcastClient.NewHazelcastClient(config);

            reset.Wait();

            client.Shutdown();
        }
    }
}
