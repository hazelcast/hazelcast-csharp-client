using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Map
{
    class MultiMapExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMultiMap<string, string>("multimap-example");

            map.Put("key", "value");
            map.Put("key", "value2");
            map.Put("key2", "value3");

            Console.WriteLine("Key: " + string.Join(", ", map.Get("key")));

            Console.WriteLine("Values : " + string.Join(", ", map.Values()));

            Console.WriteLine("KeySet: " + string.Join(", ", map.KeySet()));

            Console.WriteLine("Size: " + string.Join(", ", map.Size()));

            Console.WriteLine("EntrySet: " + string.Join(", ", map.EntrySet()));

            Console.WriteLine("ContainsKey: " + string.Join(", ", map.ContainsKey("key")));

            Console.WriteLine("ContainsValue: " + string.Join(", ", map.ContainsValue("value")));

            map.Destroy();
            client.Shutdown();
        }
    }
}
