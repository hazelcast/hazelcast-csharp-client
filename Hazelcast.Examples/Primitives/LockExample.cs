using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Primitives
{
    class LockExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var theLock = client.GetLock("lock-example");

            int i = 0;

            Action increment = () =>
            {
                for (int j = 0; j < 100; j++)
                {
                    theLock.Lock();
                    try
                    {
                        i++;
                    }
                    finally
                    {
                        theLock.Unlock();
                    }
                }
            };
            var task1 = Task.Factory.StartNew(increment);
            var task2 = Task.Factory.StartNew(increment);

            Task.WaitAll(task1, task2);

            Console.WriteLine("final value: " + i);

            theLock.Destroy();
        }
    }
}
