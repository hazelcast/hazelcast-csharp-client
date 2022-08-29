// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.NearCaching;
using Hazelcast.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.NearCache
{
    // this was NearCacheStaleReadTest.TestNoLostInvalidation in the original code
    //
    // but ... it's not testing stale reads, just that GetAsync after AddOrUpdateAsync
    // always gets the updated value so... that the local cache has been cleared
    // correctly

    [TestFixture]
    public class NearCacheClearedBySetTest : NearCacheTestBase
    {
        private const int MaxRuntime = 30;
        private const string Key = "key123";

        private int _valuePut;
        private bool _stop;
        private int _assertionViolationCount;
        private string _name;

        private IHazelcastClient _client;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            // add a member to the cluster
            await AddMember();
        }

        [SetUp]
        public async Task Init()
        {
            _client = await CreateAndStartClientAsync();
            var client = _client as HazelcastClient;
            Assert.That(client, Is.Not.Null);
            SerializationService = client.SerializationService;

            _valuePut = _assertionViolationCount = 0;
            _stop = false;

            _name = "nc-" + TestUtils.RandomString();
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_client != null) await _client.DisposeAsync();
            _client = null;
        }

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();

            options.NearCaches["nc*"] = new NearCacheOptions
            {
                EvictionPolicy = EvictionPolicy.None,
                InvalidateOnChange = true,
                InMemoryFormat = InMemoryFormat.Binary
            };

            options.NearCache.MaxToleratedMissCount = 0;

            return options;
        }

        [TestCase(true)]
        [TestCase(false)]
        [Timeout(5*60_000)]
        public async Task NearCacheClearedBySet(bool strict)
        {
            var dictionary = await _client.GetMapAsync<string, string>(_name);
            await using var _ = new AsyncDisposable(() => dictionary?.DestroyAsync() ?? default);
            var cache = GetNearCache(dictionary);

            // run test
            await RunTestInternal(dictionary);

            // if not strict, take plenty of time to become consistent
            if (!strict)
                await Task.Delay(10_000);

            var lastValue = _valuePut;
            var valueStr = await dictionary.GetAsync(Key);
            Assert.That(valueStr, Is.Not.Null);
            var valueInt = int.Parse(valueStr);

            // fail if not eventually consistent
            string msg = null;
            if (valueInt < lastValue)
            {
                msg = $"Err: getting {valueInt} instead of {lastValue}";

                // flush Near Cache and re-fetch value
                cache.Clear();
                valueStr = await dictionary.GetAsync(Key);
                Assert.That(valueStr, Is.Not.Null);
                valueInt = int.Parse(valueStr);

                // test again
                if (valueInt < lastValue)
                    msg += $" (flushing the cache the did not fix the inconsistency, getting {valueInt}).";
                else
                    msg += " (flushing the cache did fix the inconsistency).";
            }

            Logger.LogInformation($"Statistics: \n\tHits:   {cache.Statistics.Hits}\n\tMisses: {cache.Statistics.Misses}\n\tStale:  {cache.Statistics.StaleReads}\n\tEvict:  {cache.Statistics.Evictions}");

            // fail after stopping hazelcast instance
            if (msg != null)
            {
                Logger.LogWarning(msg);
                Assert.Fail(msg);
            }

            // fail if strict is required and assertion was violated
            if (strict && _assertionViolationCount > 0)
            {
                msg = "Got " + _assertionViolationCount + " errors.";
                Logger.LogWarning(msg);
                Assert.Fail(msg);
            }
        }

        private async Task RunTestInternal(IHMap<string, string> dictionary)
        {
            var tasks = new List<Task>();

            const int readerTaskCount = 4;

            // start 1 writer thread
            using var started = new SemaphoreSlim(0);
            tasks.Add(Task.Run(() => SetTask(dictionary, started, 0)));

            // wait for writer thread to start before starting getter threads
            var hasStarted = await started.WaitAsync(20_000);
            if (!hasStarted) Assert.Fail("Fail to start AddOrUpdate task.");

            // start reader tasks
            for (var i = 0; i < readerTaskCount; i++)
            {
                var id = i;
                tasks.Add(Task.Run(() => GetTask(dictionary, id)));
            }

            // stop after maxRuntime seconds
            var j = 0;
            while (!_stop && j++ < MaxRuntime)
            {
                await Task.Delay(1000);
            }

            if (!_stop)
                Logger.LogInformation("Good: problem did not occur within " + MaxRuntime + "s.");

            _stop = true;
            await Task.WhenAll(tasks);
        }

        private async Task SetTask(IHMap<string, string> dictionary, SemaphoreSlim started, int id)
        {
            var i = 0;
            while (!_stop)
            {
                i++;
                // put new value and update last state
                // note: the value in the map/Near Cache is *always* larger or equal to valuePut
                // assertion: valueMap >= valuePut
                await dictionary.SetAsync(Key, i.ToString()).CfAwait();
                _valuePut = i;

                // ensure we have a value
                if (i == 1) started.Release();

                // check if we see our last update
                var valueStr = await dictionary.GetAsync(Key).CfAwait();
                if (valueStr == null) continue; // not found

                var valueInt = int.Parse(valueStr);
                if (valueInt == i) continue; // match = ok

                _assertionViolationCount++;
                Logger.LogWarning($"Err: set {i} but got {valueInt}.");

                // maybe it needs a bit of time?
                await Task.Delay(200).CfAwait();

                // test again and stop if really lost
                valueStr = await dictionary.GetAsync(Key).CfAwait();
                valueInt = int.Parse(valueStr);
                if (valueInt == i) continue; // fixed

                Logger.LogWarning($"Err: still getting {valueInt} instead of {i}.");

                // maybe it needs a lot of time?
                // in non-strict, we'll give it plenty

                // abort all
                _stop = true;
            }
            Logger.LogInformation($"AddOrUpdate task {id} performed {i} operations.");
        }

        private async Task GetTask(IHMap<string, string> dictionary, int id)
        {
            var n = 0;
            while (!_stop)
            {
                n++;

                // get the value
                var valueStr = await dictionary.GetAsync(Key).CfAwait();

                // that should never happen!
                Assert.That(valueStr, Is.Not.Null);

                // parse the value (to get some CPU load)
                var valueInt = int.Parse(valueStr);
                Assert.AreEqual(valueInt.ToString(), valueStr);

                // slow down
                await Task.Delay(100).CfAwait();
            }
            Logger.LogInformation($"Get task {id} performed {n} operations.");
        }
    }
}
