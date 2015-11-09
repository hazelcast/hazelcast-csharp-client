using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Collections
{
    class QueueExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var queue = client.GetQueue<string>("queue-example");

            var producer = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    queue.Offer("value " + i);
                }
            });

            var consumer = Task.Factory.StartNew(() =>
            {
                int nConsumed = 0;
                string e;
                while (nConsumed++ < 100 && (e = queue.Take()) != null)
                {
                    Console.WriteLine("Consuming " + e);
                }
            });

            Task.WaitAll(producer, consumer);
            queue.Destroy();
            client.Shutdown();
        }
    }
}
