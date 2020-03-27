﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using Hazelcast.Client.Model;
using Hazelcast.Config;
using Hazelcast.Remote;
using Hazelcast.Test;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture, Ignore("Not implemented yet")]
    public class ClientStatisticsTest : SingleMemberBaseTest
    {
        [TearDown]
        public static void Destroy()
        {
            Environment.SetEnvironmentVariable("hazelcast.client.statistics.enabled", null);
        }

        protected override string GetServerConfig()
        {
            return Resources.HazelcastStat;
        }

        protected override void ConfigureClient(Configuration config)
        {
            Environment.SetEnvironmentVariable("hazelcast.client.statistics.enabled", "true");
            base.ConfigureClient(config);

            var nearCacheConfig = new NearCacheConfig
            {
                InvalidateOnChange = false,
                InMemoryFormat = InMemoryFormat.Object,
                EvictionPolicy = EvictionPolicy.Lru,
                MaxIdleSeconds = 1,
                MaxSize = 1000
            };
            config.NearCacheConfigs.Add("nearCachedMap*", nearCacheConfig);

            config.SerializationConfig.PortableFactories.Add(1, new PortableFactory());
            config.SerializationConfig
                .DataSerializableFactories.Add(IdentifiedFactory.FactoryId, new IdentifiedFactory());
        }

        [Test]
        public void TestStatisticsContent()
        {
            TestSupport.AssertTrueEventually(() =>
            {
                var stat = GetClientStatsFromServer();
                Assert.NotNull(stat);
                Assert.True(stat.Contains("clientName="+Client.Name));
                Assert.True(stat.Contains("lastStatisticsCollectionTime="));
                Assert.True(stat.Contains("enterprise=false"));
                Assert.True(stat.Contains("clientType=CSHARP"));
                Assert.True(stat.Contains("clientVersion="+VersionUtil.GetDllVersion()));
                // var ownerConnectionAddress = ClientInternal.ClusterService.GetOwnerConnectionAddress();
                // var ownerConnection = ClientInternal.ConnectionManager.GetConnection(ownerConnectionAddress);
                // Assert.True(stat.Contains("clusterConnectionTimestamp="+ownerConnection.ConnectionStartTime));
                // Assert.True(stat.Contains("clientAddress="+ ownerConnection.GetLocalSocketAddress()));
            });
        }

        [Test]
        public void TestNearCacheStatisticsContent()
        {
            var map = Client.GetMap<string, string>("nearCachedMap1");
            map.Put("key", "value");
            map.Get("key");
            map.Get("key");
            map.Get("other-key");
            
            TestSupport.AssertTrueEventually(() =>
            {
                var stat = GetClientStatsFromServer();
                Assert.NotNull(stat);
                Assert.True(stat.Contains("nc.nearCachedMap1.hits=1"));
                Assert.True(stat.Contains("nc.nearCachedMap1.misses=2"));
                Assert.True(stat.Contains("nc.nearCachedMap1.ownedEntryCount=1"));
            });
        }
        
        private string GetClientStatsFromServer()
        {
            const string script = @"client0=instance_0.getClientService().getConnectedClients().toArray()[0]
result=client0.getClientStatistics()";
            var response = RemoteController.executeOnController(HzCluster.Id, script, Lang.PYTHON);
            return response.Success && response.Result!=null ? System.Text.Encoding.UTF8.GetString(response.Result) : null;
        }
        
    }
}