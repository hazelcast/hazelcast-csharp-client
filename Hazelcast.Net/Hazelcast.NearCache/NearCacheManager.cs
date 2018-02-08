// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Config;

namespace Hazelcast.Client
{
    internal class NearCacheManager
    {
        private readonly HazelcastClient _client;
        
        private readonly ConcurrentDictionary<string, NearCache> _caches = new ConcurrentDictionary<string, NearCache>();

        public NearCacheManager(HazelcastClient client)
        {
            _client = client;
        }

        public NearCache GetOrCreateNearCache(string mapName, NearCacheConfig nearCacheConfig)
        {
            return _caches.GetOrAdd(mapName, newMapName => new NearCache(newMapName, _client, nearCacheConfig));
        }

        public void DestroyNearCache(string name)
        {
            NearCache nearCache;
            if (_caches.TryRemove(name, out nearCache))
            {
                nearCache.Destroy();
            }
        }

        private void DestroyAllNearCache()
        {
            foreach (var entry in _caches)
            {
                DestroyNearCache(entry.Key);
            }
        }

        public ICollection<NearCache> GetAllNearCaches()
        {
            return _caches.Values;
        }

        public void Shutdown()
        {
            DestroyAllNearCache();
        }
    }
}