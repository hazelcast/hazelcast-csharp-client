using System;
using System.Threading;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Examples.Map
{
    class MapListenerExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<string, string>("listener-example");

            var cdown = new CountdownEvent(2);
            map.AddEntryListener(new EntryAdapter<string, string>()
            {
                Added = e =>
                {
                    Console.WriteLine("Key '{0}' with value ' {1}' was added.", e.GetKey(), e.GetValue());
                    cdown.Signal();
                }, 
                Removed = e =>
                {
                    Console.WriteLine("Key '{0}' with value ' {1}' was removed.", e.GetKey(), e.GetOldValue());
                    cdown.Signal();
                }, 
            }, true);

            map.Put("key", "value");
            map.Remove("key");

            cdown.Wait();
            map.Destroy();
        }

    }
}
