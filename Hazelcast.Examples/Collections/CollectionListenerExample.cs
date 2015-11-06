using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Examples.Collections
{
    class CollectionListenerExample
    {
        static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var list = client.GetList<string>("collection-listener-example");
            var cdown = new CountdownEvent(3);
            list.AddItemListener(new ItemListener<string>()
            {
                OnItemAdded = e =>
                {
                    Console.WriteLine("Item added: " + e.GetItem());
                    cdown.Signal();
                },
                OnItemRemoved = e =>
                {
                    Console.WriteLine("Item removed: " + e.GetItem());
                    cdown.Signal();
                }
            }, true);

            list.Add("item1");
            list.Add("item2");
            list.Remove("item1");

            cdown.Wait();
            list.Destroy();
            client.Shutdown();
        }
    }
}
