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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;

namespace Hazelcast.Tests.NearCache
{
    public class NearCacheTestBase : MultiMembersRemoteTestBase
    {
        internal SerializationService SerializationService { get; set; }

        internal NearCacheBase GetNearCache<TKey, TValue>(IHMap<TKey, TValue> dictionary)
        {
            return dictionary is HMapWithCache<TKey, TValue> cachedDictionary ? cachedDictionary.NearCache.InnerCache : null;
        }

        internal IData ToData(object o) => SerializationService.ToData(o);

        private async Task<Response> ExecuteRemotePythonAsync(string script)
        {
            return await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.PYTHON);
        }

        protected async Task<Response> RemoveKeyAtServerAsync(string map, string key)
        {
            var script = $@"
map=instance_0.getMap('{map}')
map.remove('{key}')
";
            return await ExecuteRemotePythonAsync(script);
        }

        protected async Task<Response> PutOnMemberAsync(int key, int value, string map)
        {
            var script = $@"
map=instance_0.getMap('{map}')
map.put({key}, {value})
";
            return await ExecuteRemotePythonAsync(script);
        }

        protected async Task<Response> PopulateMapFromServerAsync(string mapName, int mapSize)
        {
            var script = $@"
map=instance_0.getMap('{mapName}')
for i in xrange(0, {mapSize}):
    map.put(i, i)
";
            return await ExecuteRemotePythonAsync(script);
        }

        protected async Task<Response> PopulateMapWithRandomValueFromServerAsync(string mapName, int mapSize)
        {
            var script = $@"
from com.hazelcast.internal.util import RandomPicker
map=instance_0.getMap('{mapName}')
for i in xrange(0, {mapSize}):
    map.put(i, RandomPicker.getInt( {mapSize}))
";
            return await ExecuteRemotePythonAsync(script);
        }

        protected async Task<List<object>> GetAllValueFromMemberAsync(int mapSize, string mapName)
        {
            var script = $@"
from java.util import ArrayList
map=instance_0.getMap('{mapName}')
list= ArrayList()
for i in xrange(0, {mapSize}):
    list.add(map.get(i))
result=instance_0.getSerializationService().toBytes(list)
";
            var response = await ExecuteRemotePythonAsync(script);
            if (!response.Success)
                throw new InvalidOperationException(response.ToString());

            var data = new HeapData(response.Result);
            return SerializationService.ToObject<List<object>>(data);
        }

        protected async Task<Response> DistortRandomPartitionUuidAsync()
        {
            const string script = @"
from com.hazelcast.core import HazelcastInstance
from com.hazelcast.internal.nearcache.impl.invalidation import Invalidator
from com.hazelcast.internal.nearcache.impl.invalidation import MetaDataGenerator
from com.hazelcast.map.impl import MapService
from com.hazelcast.map.impl import MapServiceContext
from com.hazelcast.map.impl.nearcache import MapNearCacheManager
from com.hazelcast.spi.impl import NodeEngineImpl
from com.hazelcast.instance.impl import TestUtil
from  com.hazelcast.internal.util import RandomPicker
from com.hazelcast.internal.util import UuidUtil
nodeEngineImpl = TestUtil.getNode(instance_0).nodeEngine
partitionCount = nodeEngineImpl.getPartitionService().getPartitionCount()
partitionId = RandomPicker.getInt(partitionCount)
mapService = nodeEngineImpl.getService(MapService.SERVICE_NAME)
mapServiceContext = mapService.getMapServiceContext()
mapNearCacheManager = mapServiceContext.getMapNearCacheManager()
invalidator = mapNearCacheManager.getInvalidator()
metaDataGenerator = invalidator.getMetaDataGenerator()
metaDataGenerator.setUuid(partitionId, UuidUtil.newUnsecureUUID())
";
            return await ExecuteRemotePythonAsync(script);
        }

        protected async Task<Response> DistortRandomPartitionSequenceAsync(string mapName)
        {
            var script = $@"
from com.hazelcast.core import HazelcastInstance
from com.hazelcast.internal.nearcache.impl.invalidation import Invalidator
from com.hazelcast.internal.nearcache.impl.invalidation import MetaDataGenerator
from com.hazelcast.map.impl import MapService
from com.hazelcast.map.impl import MapServiceContext
from com.hazelcast.map.impl.nearcache import MapNearCacheManager
from com.hazelcast.spi.impl import NodeEngineImpl
from com.hazelcast.instance.impl import TestUtil
from  com.hazelcast.internal.util import RandomPicker
from com.hazelcast.internal.partition import InternalPartitionService
from java.lang import Integer
nodeEngineImpl = TestUtil.getNode(instance_0).nodeEngine
mapService = nodeEngineImpl.getService(MapService.SERVICE_NAME)
mapServiceContext = mapService.getMapServiceContext()
mapNearCacheManager = mapServiceContext.getMapNearCacheManager()
invalidator = mapNearCacheManager.getInvalidator()
metaDataGenerator = invalidator.getMetaDataGenerator()
partitionService = nodeEngineImpl.getPartitionService()
partitionCount = partitionService.getPartitionCount()
metaDataGenerator.setCurrentSequence('{mapName}', RandomPicker.getInt(partitionCount), RandomPicker.getInt(Integer.MAX_VALUE))
";
            return await ExecuteRemotePythonAsync(script);
        }
    }
}
