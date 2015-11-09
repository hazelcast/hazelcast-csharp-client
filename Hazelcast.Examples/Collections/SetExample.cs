using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Collections
{
    class SetExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var set = client.GetSet<string>("set-example");

            set.Add("item1");
            set.Add("item2");
            set.Add("item3");
            set.Add("item3");

            Console.WriteLine("Enumerator : " + string.Join(", ", set));

            Console.WriteLine("Contains: " + string.Join(", ", set.Contains("item2")));

            Console.WriteLine("Count: " + string.Join(", ", set.Count));

            set.Destroy();
            client.Shutdown();
        }
    }
}
