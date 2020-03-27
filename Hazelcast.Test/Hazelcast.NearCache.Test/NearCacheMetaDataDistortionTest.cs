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
using System.Linq;
using System.Threading;
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Test;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Remote;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.NearCache.Test
{
    [TestFixture]
    [Category("3.10")]
    public class NearCacheMetaDataDistortionTest : NearcacheTestSupport
    {
        protected override void InitMembers()
        {
            //Init 2 members
            StartNewMember();
            StartNewMember();
        }
        
        protected override string GetServerConfig()
        {
            return Resources.HazelcastNearCache;
        }

        [OneTimeTearDown]
        public void RestoreEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("hazelcast.invalidation.max.tolerated.miss.count", null);
        }

        protected override void ConfigureClient(Configuration config)
        {
            base.ConfigureClient(config);
            Environment.SetEnvironmentVariable("hazelcast.invalidation.max.tolerated.miss.count", "0");
            var defaultConfig = new NearCacheConfig
            {
                MaxSize = int.MaxValue,
                InvalidateOnChange = true,
                EvictionPolicy = EvictionPolicy.None,
                InMemoryFormat = InMemoryFormat.Binary
            };
            config.NearCacheConfigs.Add("nearCachedMap*", defaultConfig);
        }

        [Test]
        public void TestInvalidationDistortionSequenceAndGuid()
        {
            var mapName = "nearCachedMapDistortion";
            var mapSize = 100000;
            var stopTest = new AtomicBoolean(false);

            Assert.True(PopulateMapFromServer(mapName, mapSize).Success);
            var clientMap = Client.GetMap<int, int>(mapName);

            var populateNearCache = new Thread(() =>
            {
                while (!stopTest.Get())
                {
                    for (int i = 0; i < mapSize; i++)
                    {
                        clientMap.Get(i);
                    }
                }
            });
            
            var distortSequence = new Thread(()=>
            {
                while (!stopTest.Get())
                {
                    var response = DistortRandomPartitionSequence(mapName);
                    Assert.True(response.Success, response.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            });

            var distortUuid = new Thread(()=>
            {
                while (!stopTest.Get())
                {
                    var response = DistortRandomPartitionUuid();
                    Assert.True(response.Success, response.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            });

            var putOnMember = new Thread(()=>
            {
                var random = new Random();
                // change some data
                while (!stopTest.Get())
                {
                    var key = random.Next(mapSize);
                    var value = random.Next(int.MaxValue);
                    Assert.True(PutOnMember(key, value, mapName).Success);
                    Thread.Sleep(100);
                }
            });

            // start threads
            putOnMember.Start();
            populateNearCache.Start();
            distortSequence.Start();
            distortUuid.Start();

            Thread.Sleep(TimeSpan.FromSeconds(60));

            // stop threads
            stopTest.Set(true);
            distortUuid.Join();
            distortSequence.Join();
            populateNearCache.Join();
            putOnMember.Join();

            TestSupport.AssertTrueEventually(()=>
            {
                var allValueFromMember = GetAllValueFromMember(mapSize, mapName);
                for (int i = 0; i < mapSize; i++)
                {
                    var valueSeenFromMember = allValueFromMember[i] as int?;
                    var valueSeenFromClient = clientMap.Get(i);
                    Assert.AreEqual(valueSeenFromMember, valueSeenFromClient);
                }
            });
        }
    }
}