using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Client.Test
{
    public class HazelcastBaseTest
    {
        protected static volatile IHazelcastInstance client ;

        protected static volatile object loc=new object();

        protected static IHazelcastInstance InitClient()
        {
            if (client == null)
            {
                lock (loc)
                {
                    if (client == null)
                    {
                        client = NewHazelcastClient();
                    }
                }
            }
            return client;

        }

        protected static IHazelcastInstance NewHazelcastClient()
        {
            var config = new ClientConfig();
            config.AddAddress("127.0.0.1");
            return HazelcastClient.NewHazelcastClient(config);
        }

        public HazelcastBaseTest()
        {
        }


    }
}
