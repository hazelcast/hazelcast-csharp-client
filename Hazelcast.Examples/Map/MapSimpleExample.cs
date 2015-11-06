using System;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Map
{
    class MapSimpleExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");
            
            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<string, string>("simple-example");

            map.Put("key", "value");
            map.Put("key2", "value2");

            Console.WriteLine("Key: " + map.Get("key"));

            Console.WriteLine("Values : " + string.Join(", ",map.Values()));

            Console.WriteLine("KeySet: " + string.Join(", ",map.KeySet()));

            Console.WriteLine("Size: " + string.Join(", ",map.Size()));
            
            Console.WriteLine("EntrySet: " + string.Join(", ", map.EntrySet()));
            
            Console.WriteLine("ContainsKey: " + string.Join(", ", map.ContainsKey("key")));
            
            Console.WriteLine("ContainsValue: " + string.Join(", ", map.ContainsValue("value")));

            map.Destroy();
            client.Shutdown();
        }
    }
}
