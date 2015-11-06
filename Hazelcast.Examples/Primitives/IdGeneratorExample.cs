using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Primitives
{
    class IdGeneratorExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var generator = client.GetIdGenerator("example-id-generator");

            generator.Init(1000);

            Action generateId = () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Console.WriteLine("Generated id: " + generator.NewId());
                }
            };
            var task1 = Task.Factory.StartNew(generateId);
            var task2 = Task.Factory.StartNew(generateId);

            Task.WaitAll(task1, task2);
            generator.Destroy();
        }
    }
}
