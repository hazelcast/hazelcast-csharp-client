using System;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Primitives
{
    class AtomicLongExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var atomicLong = client.GetAtomicLong("atomic-long-exmaple");

            Action increment = () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    atomicLong.IncrementAndGet();
                }
            };
            var task1 = Task.Factory.StartNew(increment);
            var task2 = Task.Factory.StartNew(increment);

            Task.WaitAll(task1, task2);
            Console.WriteLine("Final value: " + atomicLong.Get());
            atomicLong.Destroy();
        }
    }
}
