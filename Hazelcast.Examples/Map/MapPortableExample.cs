using System;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Examples.Models;

namespace Hazelcast.Examples.Map
{
    class MapPortableExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetSerializationConfig().AddPortableFactory(1, new ExamplePortableFactory());
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<int, Customer>("portable-example");

            var customer = new Customer() { Id = 1, LastOrder = DateTime.UtcNow, Name = "first-customer" };
            
            Console.WriteLine("Adding customer: " + customer);
            map.Put(customer.Id, customer);

            var c = map.Get(customer.Id);

            Console.WriteLine("Gotten customer: " + c);

            client.Shutdown();
        }
    }
}
