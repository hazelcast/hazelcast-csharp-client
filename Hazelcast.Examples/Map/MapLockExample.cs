using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Map
{
    class MapLockExample
    {
        private static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<string, string>("map-lock-example");

            map.Put("key", "value");

            map.Lock("key");
            var task = Task.Factory.StartNew(() =>
            {
                map.Put("key", "newValue");
                Console.WriteLine("Put new value");
            });
            try
            {
                var value = map.Get("key");
                //do something with the value..
                Thread.Sleep(5000);
            }
            finally
            {
                map.Unlock("key");
            }
            task.Wait();

            Console.WriteLine("New value: " + map.Get("key"));
        }
    }
}
