using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Examples.Models;

namespace Hazelcast.Examples.Map
{
    class MapDataSerializableExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");

            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<int, Person>("portable-example");

            var person = new Person(1, "A person", 30);

            Console.WriteLine("Adding person: " + person);
            //map.Put(person.Id, person);

            var p = map.Get(person.Id);

            Console.WriteLine("Gotten person: " + p);

            client.Shutdown();
        }
    }
}
