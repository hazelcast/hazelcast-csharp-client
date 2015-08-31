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

        private IHazelcastInstance _client;
        private static readonly ILogger logger = Logger.GetLogger(typeof(HazelcastTestClient));

        protected virtual IHazelcastInstance NewHazelcastClient()
        {
            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1:5701");
            config.AddNearCacheConfig("nearCachedMap*", new NearCacheConfig().SetInMemoryFormat(InMemoryFormat.Object));
            config.GetSerializationConfig().AddPortableFactory(1, new PortableFactory());
            config.SetLicenseKey(UNLIMITED_LICENSE);
            return HazelcastClient.NewHazelcastClient(config);
        }

        public IHazelcastInstance Init()
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");
            _client = NewHazelcastClient();
            while (!_client.GetLifecycleService().IsRunning())
            {
                logger.Info("Waiting to start up client");
                Thread.Sleep(100);
            }
            return _client;
        }

            
    }
}
