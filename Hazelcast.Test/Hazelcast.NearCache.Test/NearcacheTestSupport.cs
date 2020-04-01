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
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Test;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Remote;

namespace Hazelcast.NearCache.Test
{
    public class NearcacheTestSupport : MultiMemberBaseTest
    {
        internal BaseNearCache GetNearCache<TK, TV>(IMap<TK, TV> map)
        {
            var mapProxy = (map as ClientMapNearCacheProxy<TK, TV>);
            return mapProxy != null ? mapProxy.NearCache : null;
        }

        protected Response RemoveKeyAtServer(string map, string key)
        {
            var script = string.Format(@"map=instance_0.getMap('{0}')
map.remove('{1}')
", map, key);
            return RemoteController.executeOnController(HzCluster.Id, script, Lang.PYTHON);
        }

        protected Response PutOnMember(int key, int value, string map)
        {
            var script = string.Format(@"map=instance_0.getMap('{0}')
map.put({1}, {2})
", map, key, value);
            return RemoteController.executeOnController(HzCluster.Id, script, Lang.PYTHON);
        }

        protected Response PopulateMapFromServer(string mapName, int mapSize)
        {
            var script = string.Format(@"map=instance_0.getMap('{0}')
for i in xrange(0, {1}):
    map.put(i, i)
", mapName, mapSize);
            return RemoteController.executeOnController(HzCluster.Id, script, Lang.PYTHON);
        }

        protected Response PopulateMapWithRandomValueFromServer(string mapName, int mapSize)
        {
            var script = string.Format(@"
from com.hazelcast.util import RandomPicker
map=instance_0.getMap('{0}')
for i in xrange(0, {1}):
    map.put(i, RandomPicker.getInt( {1}))
", mapName, mapSize);
            return RemoteController.executeOnController(HzCluster.Id, script, Lang.PYTHON);
        }

        protected List<object> GetAllValueFromMember(int mapSize, string mapName)
        {
            var script = string.Format(@"
from java.util import ArrayList
map=instance_0.getMap('{0}')
list= ArrayList()
for i in xrange(0, {1}):
    list.add(map.get(i))
result=instance_0.getSerializationService().toBytes(list)
", mapName, mapSize);
            var response = RemoteController.executeOnController(HzCluster.Id, script, Lang.PYTHON);
            if (!response.Success)
            {
                throw new InvalidOperationException(response.ToString());
            }
            var data = new HeapData(response.Result);
            return ClientInternal.GetSerializationService().ToObject<List<object>>(data);
        }

        protected Response DistortRandomPartitionUuid()
        {
            const string script = @"
from com.hazelcast.core import HazelcastInstance
from com.hazelcast.internal.nearcache.impl.invalidation import Invalidator
from com.hazelcast.internal.nearcache.impl.invalidation import MetaDataGenerator
from com.hazelcast.map.impl import MapService
from com.hazelcast.map.impl import MapServiceContext
from com.hazelcast.map.impl.nearcache import MapNearCacheManager
from com.hazelcast.spi.impl import NodeEngineImpl
from com.hazelcast.instance import TestUtil
from com.hazelcast.util import RandomPicker
from com.hazelcast.util import UuidUtil
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
            return RemoteController.executeOnController(HzCluster.Id, script, Lang.PYTHON);
        }

        protected Response DistortRandomPartitionSequence(string mapName)
        {
            const string script = @"
from com.hazelcast.core import HazelcastInstance
from com.hazelcast.internal.nearcache.impl.invalidation import Invalidator
from com.hazelcast.internal.nearcache.impl.invalidation import MetaDataGenerator
from com.hazelcast.map.impl import MapService
from com.hazelcast.map.impl import MapServiceContext
from com.hazelcast.map.impl.nearcache import MapNearCacheManager
from com.hazelcast.spi.impl import NodeEngineImpl
from com.hazelcast.instance import TestUtil
from com.hazelcast.util import RandomPicker
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
metaDataGenerator.setCurrentSequence('{0}', RandomPicker.getInt(partitionCount), RandomPicker.getInt(Integer.MAX_VALUE))
";
            return RemoteController.executeOnController(HzCluster.Id, string.Format(script, mapName), Lang.PYTHON);
        }

    }
}