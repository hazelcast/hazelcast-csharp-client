/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Hazelcast.Client.Proxy;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{

    [TestFixture]
    public class ClientNearCacheTest : HazelcastBaseTest
    {
        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            var nearCacheConfig = new NearCacheConfig().SetInMemoryFormat(InMemoryFormat.Object);
            config.AddNearCacheConfig("nearCachedMap*", nearCacheConfig);
        }

        internal static IMap<object, object> map;

        //
        [SetUp]
        public void Init()
        {
            map = Client.GetMap<object, object>("nearCachedMap-" + TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            map.Clear();
            map.Destroy();
        }

        [Test]
        public void TestNearCache() 
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

        [Test]
        public void testNearCacheGetAll() 
        {
            var keys=new List<object>();
            for (int i = 0; i < 100; i++) 
            {
                map.Put("key" + i, "value" + i);
                keys.Add("key" + i);
            }

            map.GetAll(keys);
            var mapImpl = map as ClientMapProxy<object, object>;
            var clientNearCache = mapImpl.NearCache;

            Assert.AreEqual(100, clientNearCache.cache.Count);
        }

        [Test]
        public void TestNearCacheGetAsync() 
        {
             
            map.Put("key", "value");

            var val = map.GetAsync("key");

            var result = val.Result;
            Assert.AreEqual("value",result);


            var mapImpl = map as ClientMapProxy<object, object>;
            var clientNearCache = mapImpl.NearCache;

            TestSupport.AssertTrueEventually(() =>
            {
                Assert.AreEqual(1, clientNearCache.cache.Count);
            });

            val = map.GetAsync("key");
            result = val.Result;
            Assert.AreEqual("value", result);

            ClientNearCache.CacheRecord cacheRecord = clientNearCache.cache.Values.ToArray()[0];
            Assert.AreEqual(1, cacheRecord.hit.Get());
        }

        [Test]
        public void TestNearCacheLocalInvalidations() 
        {
             
            map.Put("key", "value");

            var val = map.Get("key");
            Assert.AreEqual("value",val);


            var mapImpl = map as ClientMapProxy<object, object>;
            var clientNearCache = mapImpl.NearCache;

            Assert.AreEqual(1, clientNearCache.cache.Count);

            map.Remove("key");
            Assert.Null(map.Get("key"));

            //ClientNearCache.CacheRecord cacheRecord = clientNearCache.cache.Values.ToArray()[0];
            //Assert.AreEqual(0, cacheRecord.hit.Get());
        }

    }
}
