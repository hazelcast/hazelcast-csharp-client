// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.NearCache.Test
{
    [TestFixture]
    [Category("3.10")]
    public class NearCacheStaleReadTest : NearcacheTestSupport
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(NearCacheStaleReadTest));
        private const int NumGetters = 7;
        private const int MaxRuntime = 30;
        private const string Key = "key123";

        private AtomicInteger _valuePut;
        private AtomicBoolean _stop;
        private AtomicInteger _assertionViolationCount;
        private AtomicBoolean _failed;
        private readonly string _mapName = "nearCachedMap-" + TestSupport.RandomString();
        private IMap<string, string> _map;


        [SetUp]
        public void Setup()
        {
            _valuePut = new AtomicInteger(0);
            _stop = new AtomicBoolean(false);
            _failed = new AtomicBoolean(false);
            _assertionViolationCount = new AtomicInteger(0);

            base.SetupCluster();

            _map = Client.GetMap<string, string>(_mapName);
            var nc = GetNearCache(_map);
            Assert.AreEqual(typeof(NearCache), nc.GetType());
        }

        [TearDown]
        public void Destroy()
        {
            base.ShutdownRemoteController();
        }

        [OneTimeTearDown]
        public void RestoreEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("hazelcast.invalidation.max.tolerated.miss.count", null);
        }

        public override void SetupCluster()
        {
            //no op
        }

        public override void ShutdownRemoteController()
        {
            //no op
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
        public void TestNoLostInvalidationsEventually()
        {
            TestNoLostInvalidationsStrict(false);
        }

        [Test]
        public void TestNoLostInvalidationsStrict()
        {
            TestNoLostInvalidationsStrict(true);
        }

        private void TestNoLostInvalidationsStrict(bool strict)
        {
            // run test
            RunTestInternal();

            if (!strict)
            {
                // test eventually consistent
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
            var valuePutLast = _valuePut.Get();
            var valueMapStr = _map.Get(Key);
            var valueMap = int.Parse(valueMapStr);

            // fail if not eventually consistent
            string msg = null;
            if (valueMap < valuePutLast)
            {
                msg = "Near Cache did *not* become consistent. (valueMap = " + valueMap + ", valuePut = " + valuePutLast + ").";

                // flush Near Cache and re-fetch value
                FlushClientNearCache(_map);
                var valueMap2Str = _map.Get(Key);
                var valueMap2 = int.Parse(valueMap2Str);

                // test again
                if (valueMap2 < valuePutLast)
                {
                    msg += " Unexpected inconsistency! (valueMap2 = " + valueMap2 + ", valuePut = " + valuePutLast + ").";
                }
                else
                {
                    msg += " Flushing the Near Cache cleared the inconsistency. (valueMap2 = " + valueMap2 + ", valuePut = " +
                           valuePutLast + ").";
                }
            }
            // stop client
            ClientInternal.GetLifecycleService().Terminate();

            // fail after stopping hazelcast instance
            if (msg != null)
            {
                Logger.Warning(msg);
                Assert.Fail(msg);
            }

            // fail if strict is required and assertion was violated
            if (strict && _assertionViolationCount.Get() > 0)
            {
                msg = "Assertion violated " + _assertionViolationCount.Get() + " times.";
                Logger.Warning(msg);
                Assert.Fail(msg);
            }
        }

        private void FlushClientNearCache<TK, TV>(IMap<TK, TV> map)
        {
            var nc = GetNearCache(map);
            if (nc != null)
            {
                nc.Clear();
            }
        }


        private void RunTestInternal()
        {
            // start 1 putter thread (put0)
            var threadPut = new Thread(PutRunnable) {Name = "put0"};
            threadPut.Start();

            // wait for putter thread to start before starting getter threads
            Thread.Sleep(300);

            // start numGetters getter threads (get0-numGetters)
            var threads = new List<Thread>();

            for (var i = 0; i < NumGetters; i++)
            {
                var thread = new Thread(GetRunnable) {Name = "get" + i};
                threads.Add(thread);
            }
            foreach (var thread in threads)
            {
                thread.Start();
            }

            // stop after maxRuntime seconds
            var j = 0;
            while (!_stop.Get() && j++ < MaxRuntime)
            {
                Thread.Sleep(1000);
            }
            if (!_stop.Get())
            {
                Logger.Info("Problem did not occur within " + MaxRuntime + "s.");
            }
            _stop.Set(true);
            threadPut.Join();
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

        private void PutRunnable()
        {
            Logger.Info(Thread.CurrentThread.Name + " started.");
            int i = 0;
            while (!_stop.Get())
            {
                i++;
                // put new value and update last state
                // note: the value in the map/Near Cache is *always* larger or equal to valuePut
                // assertion: valueMap >= valuePut
                _map.Put(Key, i.ToString());
                _valuePut.Set(i);

                // check if we see our last update
                var valueMapStr = _map.Get(Key);
                if (valueMapStr == null)
                {
                    continue;
                }
                int valueMap = int.Parse(valueMapStr);
                if (valueMap != i)
                {
                    _assertionViolationCount.IncrementAndGet();
                    Logger.Warning("Assertion violated! (valueMap = " + valueMap + ", i = " + i + ")");

                    // sleep to ensure Near Cache invalidation is really lost
                    Thread.Sleep(100);

                    // test again and stop if really lost
                    valueMapStr = _map.Get(Key);
                    valueMap = int.Parse(valueMapStr);
                    if (valueMap != i)
                    {
                        Logger.Warning("Near Cache invalidation lost! (valueMap = " + valueMap + ", i = " + i + ")");
                        _failed.Set(true);
                        _stop.Set(true);
                    }
                }
            }
            Logger.Info(Thread.CurrentThread.Name + " performed " + i + " operations.");
        }

        private void GetRunnable()
        {
            Logger.Info(Thread.CurrentThread.Name + " started.");
            var n = 0;
            while (!_stop.Get())
            {
                n++;
                // blindly get the value (to trigger the issue) and parse the value (to get some CPU load)
                var valueMapStr = _map.Get(Key);
                var i = int.Parse(valueMapStr);
                Assert.AreEqual("" + i, valueMapStr);
            }
            Logger.Info(Thread.CurrentThread.Name + " performed " + n + " operations.");
        }
    }
}