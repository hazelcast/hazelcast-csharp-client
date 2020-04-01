// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Client.Test;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.NearCache.Test
{
    [TestFixture]
    [Category("3.10")]
    public class NearCacheStalenessTest : NearcacheTestSupport
    {
        private const int EntryCount = 10;
        private const int NearCacheInvalidatorThreadCount = 1;
        private const int NearCachePutterThreadCount = 10;
        private readonly string _mapName = "nearCachedMap-" + TestSupport.RandomString();
        private readonly AtomicBoolean _stop = new AtomicBoolean(false);
        private IMap<int, int> _map;

        [SetUp]
        public void Setup()
        {
            _map = Client.GetMap<int, int>(_mapName);
            var nc = GetNearCache(_map);
            Assert.AreEqual(typeof(NearCache), nc.GetType());
        }

        [TearDown]
        public void Destroy()
        {
            _map.Destroy();
        }

        [OneTimeTearDown]
        public void RestoreEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("hazelcast.invalidation.max.tolerated.miss.count", null);
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            Environment.SetEnvironmentVariable("hazelcast.invalidation.max.tolerated.miss.count", "0");
            var defaultConfig = new NearCacheConfig().SetInvalidateOnChange(true).SetEvictionPolicy("None")
                .SetInMemoryFormat(InMemoryFormat.Binary);
            config.AddNearCacheConfig("nearCachedMap*", defaultConfig);
        }

        [Test]
        public void TestNearCache_notContainsStaleValue_whenUpdatedByMultipleThreads()
        {
            var threads = new List<Thread>();

            for (int i = 0; i < NearCacheInvalidatorThreadCount; i++)
            {
                var invalidationThread = new Thread(() =>
                {
                    while (!_stop.Get())
                    {
                        Assert.True(PopulateMapWithRandomValueFromServer(_mapName, EntryCount).Success);
                    }
                });
                threads.Add(invalidationThread);
            }

            for (var i = 0; i < NearCachePutterThreadCount; i++)
            {
                var putterThread = new Thread(() =>
                {
                    while (!_stop.Get())
                    {
                        for (int j = 0; j < EntryCount; j++)
                        {
                            _map.Get(j);
                        }
                    }
                });
                threads.Add(putterThread);
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            Thread.Sleep(TimeSpan.FromSeconds(5));

            _stop.Set(true);

            foreach (var thread in threads)
            {
                thread.Join();
            }
            TestSupport.AssertTrueEventually(() => { AssertNoStaleDataExistInNearCache(); });
        }

        private void AssertNoStaleDataExistInNearCache()
        {
            // 1. get all entries when Near Cache is full, so some values will come from Near Cache
            var fromNearCache = getAllEntries();

            // 2. get all values from server directly
            var valueFromMember = GetAllValueFromMember(EntryCount, _mapName);

            for (int i = 0; i < EntryCount; i++)
            {
                Assert.AreEqual(valueFromMember[i], fromNearCache[i]);
            }
        }

        private List<int> getAllEntries()
        {
            var list = new List<int>();
            for (int i = 0; i < EntryCount; i++)
            {
                list.Add(_map.Get(i));
            }
            return list;
        }
    }
}