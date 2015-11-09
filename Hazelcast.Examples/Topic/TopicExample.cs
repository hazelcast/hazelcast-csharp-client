using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Topic
{
    class TopicExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var topic = client.GetTopic<string>("topic-example");
            
            var countdown = new CountdownEvent(100);
            topic.AddMessageListener(m =>
            {
                Console.WriteLine("Got message: " + m.GetMessageObject());
                countdown.Signal();
            });

            var publisher = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    topic.Publish("Message " + i);
                }
            });

            countdown.Wait();

            topic.Destroy();
            client.Shutdown();
        }
    }
}
