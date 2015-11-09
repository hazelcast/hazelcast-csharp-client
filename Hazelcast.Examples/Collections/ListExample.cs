using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Collections
{
    class ListExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var list = client.GetList<string>("list-example");

            list.Add("item1");
            list.Add("item2");
            list.Add("item3");

            Console.WriteLine("Get: " + list[0]);

            Console.WriteLine("Enumerator : " + string.Join(", ", list));

            Console.WriteLine("Contains: " + string.Join(", ", list.Contains("item2")));

            Console.WriteLine("Count: " + string.Join(", ", list.Count));

            Console.WriteLine("Sublist: " + string.Join(", ", list.SubList(0, 2)));

            list.Destroy();
            client.Shutdown();
        }
    }
}
