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
    class LifecycleExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");

            var reset = new ManualResetEventSlim();
            config.AddListenerConfig(new ListenerConfig(new LifecycleListener(e =>
            {
                Console.WriteLine("new state: " + e.GetState());
                if (e.GetState() == LifecycleEvent.LifecycleState.ClientConnected)
                {
                    reset.Set();
                }
            })));
            var client = HazelcastClient.NewHazelcastClient(config);

            reset.Wait();

            client.Shutdown();
        }
    }
}
