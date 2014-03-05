using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class HazelcastBaseTest
    {
        protected IHazelcastInstance client ;

        protected static volatile Random random = new Random((int)Clock.CurrentTimeMillis());


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
            return HazelcastClient.NewHazelcastClient(config);
        }

        public HazelcastBaseTest()
        {
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

        protected string Name
        {
            get
            {
                return "csharp-name-" + random.Next(1000000);
            }
        }
    }
}
