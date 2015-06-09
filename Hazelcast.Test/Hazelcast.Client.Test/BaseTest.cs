using System;
using System.Threading;
using Hazelcast.Client.Model;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class HazelcastBaseTest
    {
        internal static string UNLIMITED_LICENSE =
            "HazelcastEnterprise#9999Nodes#9999Clients#HDMemory:99999999GB#w7yAkRj1IbHcBfVimEOuKr638599939999peZ319999z05999Wn149zGxG09";

        protected static volatile Random random = new Random((int) Clock.CurrentTimeMillis());
        protected IHazelcastInstance client;

        protected static string Name
        {
            get { return "csharp-name-" + random.Next(1000000); }
        }

        protected IHazelcastInstance InitClient()
        {
            client = NewHazelcastClient();
            return client;
        }

        protected virtual IHazelcastInstance NewHazelcastClient()
        {
            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            config.AddNearCacheConfig("nearCachedMap*", new NearCacheConfig().SetInMemoryFormat(InMemoryFormat.Object));
            config.GetSerializationConfig().AddPortableFactory(1, new PortableFactory());
            config.SetLicenseKey(UNLIMITED_LICENSE);
            return HazelcastClient.NewHazelcastClient(config);
        }

        [TestFixtureSetUp]
        public void InitFixture()
        {
            InitClient();
            while (!client.GetLifecycleService().IsRunning())
            {
                Console.WriteLine("Waiting to start up  client");
                Thread.Sleep(100);
            }
            InitMoreFixture();
        }

        public virtual void InitMoreFixture()
        {
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Console.WriteLine("TearDown Shut down client");
            //Task.Factory.StartNew(client.Shutdown);
            client.Shutdown();
            Console.WriteLine("Shut down client");
            client = null;
        }
    }
}