using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Test
{
    public class ListenerApp
    {

        static void Main2(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var clientConfig = new ClientConfig();
            clientConfig.GetNetworkConfig().AddAddress("127.0.0.1");
            var hc = HazelcastClient.NewHazelcastClient(clientConfig);
            var listener1 = new EntryAdapter<string, string>(
                @event => Console.WriteLine("ADD"),
                @event => Console.WriteLine("REMOVE"),
                @event => Console.WriteLine("UPDATE"),
                @event => Console.WriteLine("EVICTED"));

            var map = hc.GetMap<string, string>("default");
            string reg1 = map.AddEntryListener(listener1, false);

        }

    }
}
