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
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    //TODO: review comments an todos below

    internal static class CustomTypeFactory
    {
        public static Address CreateAddress(string host, int port)
        {
            try
            {
                return new Address(host, port);
            }
            catch (UnknownHostException e)
            {
                throw new HazelcastException(e);
            }
        }

        //public static CacheEventDataImpl CreateCacheEventData(string name, int cacheEventType, IData dataKey, IData dataValue, IData dataOldValue, bool oldValueAvailable)
        //{
        //    return new CacheEventDataImpl(name, CacheEventType.getByType(cacheEventType), dataKey, dataValue,
        //            dataOldValue, oldValueAvailable);
        //}

        //public static TimedExpiryPolicyFactoryConfig CreateTimedExpiryPolicyFactoryConfig(string expiryPolicyType, DurationConfig durationConfig)
        //{
        //    return new TimedExpiryPolicyFactoryConfig(ExpiryPolicyType.valueOf(expiryPolicyType), durationConfig);
        //}

        //public static CacheSimpleEntryListenerConfig CreateCacheSimpleEntryListenerConfig(bool oldValueRequired,
        //                                                                                  bool synchronous,
        //                                                                                  string cacheEntryListenerFactory,
        //                                                                                  string cacheEntryEventFilterFactory)
        //{
        //    CacheSimpleEntryListenerConfig config = new CacheSimpleEntryListenerConfig();
        //    config.setOldValueRequired(oldValueRequired);
        //    config.setSynchronous(synchronous);
        //    config.setCacheEntryListenerFactory(cacheEntryListenerFactory);
        //    config.setCacheEntryEventFilterFactory(cacheEntryEventFilterFactory);
        //    return config;
        //}

        //public static EventJournalConfig CreateEventJournalConfig(bool enabled, int capacity, int timeToLiveSeconds)
        //{
        //    EventJournalConfig config = new EventJournalConfig();
        //    config.setEnabled(enabled);
        //    config.setCapacity(capacity);
        //    config.setTimeToLiveSeconds(timeToLiveSeconds);
        //    return config;
        //}

        //public static HotRestartConfig CreateHotRestartConfig(bool enabled, bool fsync)
        //{
        //    HotRestartConfig config = new HotRestartConfig();
        //    config.setEnabled(enabled);
        //    config.setFsync(fsync);
        //    return config;
        //}

        //public static MerkleTreeConfig CreateMerkleTreeConfig(bool enabled, int depth)
        //{
        //    MerkleTreeConfig config = new MerkleTreeConfig();
        //    config.setEnabled(enabled);
        //    config.setDepth(depth);
        //    return config;
        //}

        //public static NearCachePreloaderConfig CreateNearCachePreloaderConfig(bool enabled, string directory,
        //                                                                      int storeInitialDelaySeconds,
        //                                                                      int storeIntervalSeconds)
        //{
        //    NearCachePreloaderConfig config = new NearCachePreloaderConfig();
        //    config.setEnabled(enabled);
        //    config.setDirectory(directory);
        //    config.setStoreInitialDelaySeconds(storeInitialDelaySeconds);
        //    config.setStoreIntervalSeconds(storeIntervalSeconds);
        //    return config;
        //}

        public static SimpleEntryView<IData, IData> CreateSimpleEntryView(IData key, IData value, long cost, long creationTime,
                                                            long expirationTime, long hits, long lastAccessTime,
                                                            long lastStoredTime, long lastUpdateTime, long version,
                                                            long ttl, long maxIdle)
        {
            return new SimpleEntryView<IData, IData>
            {
                Key = key,
                Value = value,
                Cost = cost,
                CreationTime = creationTime,
                ExpirationTime = expirationTime,
                Hits = hits,
                LastAccessTime = lastAccessTime,
                LastStoredTime = lastStoredTime,
                LastUpdateTime = lastUpdateTime,
                Version = version,
                Ttl = ttl,
                MaxIdle = maxIdle
            };
        }

        //public static QueryCacheEventData CreateQueryCacheEventData(IData dataKey, IData dataNewValue, long sequence, int eventType, int partitionId)
        //{
        //    return new QueryCacheEventData
        //    {
        //        DataKey = dataKey,
        //        DataNewValue = dataNewValue,
        //        Sequence = sequence,
        //        EventType = eventType,
        //        PartitionId = partitionId
        //    };
        //}

        //public static DurationConfig createDurationConfig(long durationAmount, string timeUnit)
        //{
        //    return new DurationConfig(durationAmount, TimeUnit.valueOf(timeUnit));
        //}
    }
}