using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Collections
{
    class RingbufferExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var ringbuffer = client.GetRingbuffer<string>("ringbuffer-example");

            var writer = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    ringbuffer.Add("item " + i);
                }
            });

            var reader = Task.Factory.StartNew(() =>
            {
                var sequence = ringbuffer.HeadSequence();
                while (sequence < 100)
                {
                    var item = ringbuffer.ReadOne(sequence++);
                    Console.WriteLine("Reading value " + item);
                }
            });

            Task.WaitAll(reader, writer);
            ringbuffer.Destroy();
            client.Shutdown();
        }
    }
}
