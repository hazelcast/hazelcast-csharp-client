using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{

    [TestFixture]
    public class ClientNearCacheTest : HazelcastBaseTest
    {

        internal static IMap<object, object> map;

        //
        [SetUp]
        public void Init()
        {
            map = client.GetMap<object, object>("nearCachedMap-" + Name);
        }

        [TearDown]
        public static void Destroy()
        {
            map.Clear();
        }

        [Test]
        public void testNearCache() 
        {
            for (int i = 0; i < 100; i++) 
            {
                map.Put("key" + i, "value" + i);
            }
            long begin = Clock.CurrentTimeMillis();
            for (int i = 0; i < 100; i++) {
                map.Get("key" + i);
            }
            long firstRead = Clock.CurrentTimeMillis() - begin;
            begin = Clock.CurrentTimeMillis();
            for (int i = 0; i < 100; i++) {
                map.Get("key" + i);
            }
            long secondRead = Clock.CurrentTimeMillis() - begin;
            Assert.IsTrue(secondRead < firstRead);
        }

    }
}
