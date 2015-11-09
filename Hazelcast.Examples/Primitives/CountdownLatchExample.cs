using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Examples.Primitives
{
    class CountdownLatchExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var latch = client.GetCountDownLatch("countdown-latch-example");

            latch.TrySetCount(2);

            var task1 = Task.Factory.StartNew(() =>
            {
                //do some work
                Thread.Sleep(5000);
                latch.CountDown();
            });

            var task2 = Task.Factory.StartNew(() =>
            {
                //do some work
                Thread.Sleep(10000);
                latch.CountDown();
            });

            latch.Await(20, TimeUnit.SECONDS);
            Console.WriteLine("Tasks completed");
            latch.Destroy();
        }
    }
}
