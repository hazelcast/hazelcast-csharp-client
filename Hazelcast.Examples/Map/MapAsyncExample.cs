using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Map
{
    class MapAsyncExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<string, string>("simple-example");

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var key = "key " + i;
                var task = map.PutAsync(key, " value " +i).ContinueWith(t =>
               {
                   Console.WriteLine("Added " + key);
               });
               tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            map.Destroy();
            client.Shutdown();
        }
    }
}
