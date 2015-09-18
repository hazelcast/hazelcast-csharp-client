using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Model;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Logging;

namespace Hazelcast.Client.Test
{
    class HazelcastTestClient
    {
        internal static string UNLIMITED_LICENSE =
            "HazelcastEnterprise#9999Nodes#9999Clients#HDMemory:99999999GB#w7yAkRj1IbHcBfVimEOuKr638599939999peZ319999z05999Wn149zGxG09";

        private static readonly ILogger logger = Logger.GetLogger(typeof(HazelcastTestClient));

        protected virtual IHazelcastInstance NewHazelcastClient(ListenerConfig[] listeners)
        {
            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1:5701");
            config.GetNetworkConfig().SetConnectionAttemptLimit(20);
            config.GetNetworkConfig().SetConnectionAttemptPeriod(2000);
            config.AddNearCacheConfig("nearCachedMap*", new NearCacheConfig().SetInMemoryFormat(InMemoryFormat.Object));
            config.GetSerializationConfig().AddPortableFactory(1, new PortableFactory());
            config.SetLicenseKey(UNLIMITED_LICENSE);
            config.SetListenerConfigs(new List<ListenerConfig>(listeners));
            return HazelcastClient.NewHazelcastClient(config);
        }

        public IHazelcastInstance GetNewClient(params ListenerConfig[] listeners)
        {
            return NewHazelcastClient(listeners);
        }

            
    }
}
