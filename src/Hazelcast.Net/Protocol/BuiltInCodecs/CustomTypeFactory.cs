// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.NearCaching;
using Hazelcast.Networking;
using Hazelcast.Protocol.Models;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Sql;
namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class CustomTypeFactory
    {
        public static NetworkAddress CreateAddress(string host, int port)
        {
            try
            {
                // The creation of the address uses https://docs.microsoft.com/en-us/dotnet/api/system.net.dns.gethostaddresses
                // This method may throw ArgumentException, SocketException, ArgumentOutOfRangeException, ArgumentNullException
                // Java implementation may throw https://docs.oracle.com/javase/7/docs/api/java/net/UnknownHostException.html
                return new NetworkAddress(host, port);
            }
            catch (Exception e)
            {
                throw new HazelcastException(e);
            }
        }

        public static MapEntryStats<IData, IData> CreateSimpleEntryView(IData key, IData value, long cost, long creationTime,
            long expirationTime, long hits, long lastAccessTime, long lastStoredTime, long lastUpdateTime, long version, long ttl,
            long maxIdle)
        {
            return new MapEntryStats<IData, IData>
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

        internal static HazelcastJsonValue CreateHazelcastJsonValue(string value)
        {
            return new HazelcastJsonValue(value);
        }

        public static IndexOptions CreateIndexConfig(string name, int indexType, List<string> attributes, BitmapIndexOptions bitmapIndexOptions, bool bTreeIndexConfigExists, BTreeIndexOptions bTreeIndexConfig)
        {
            var options = new IndexOptions(attributes) { Name = name, Type = (IndexType) indexType, BitmapIndex = bitmapIndexOptions };
            if (bTreeIndexConfigExists) options.BTreeIndex = bTreeIndexConfig;
            return options;
        }

        public static BitmapIndexOptions CreateBitmapIndexOptions(string uniqueKey, int uniqueKeyTransformation)
        {
            return new BitmapIndexOptions { UniqueKey = uniqueKey, UniqueKeyTransformation = (UniqueKeyTransformation) uniqueKeyTransformation };
        }

        public static EndpointQualifier CreateEndpointQualifier(int type, string identifier)
        {
            return new EndpointQualifier((ProtocolType) type, identifier);
        }

        public static SqlColumnMetadata CreateSqlColumnMetadata(string name, int type, bool isNullableExists, bool nullable)
        {
            if (!Enum.IsDefined(typeof(SqlColumnType), type))
                throw new NotSupportedException($"Column type #{type} is not supported.");

            var sqlColumnType = (SqlColumnType) type;

            return new SqlColumnMetadata(name, sqlColumnType,
                // By default, columns are nullable
                // The column becomes non-nullable only if NOT NULL modifier applied during table creation or if an expression is selected
                nullable || !isNullableExists
            );
        }

        public static SchemaField CreateFieldDescriptor(string name, int kind)
            => new SchemaField(name, FieldKindEnum.Parse(kind));

        public static Schema CreateSchema(string typename, IEnumerable<SchemaField> fields)
            => new Schema(typename, fields.ToArray());

        public static Capacity CreateCapacity(long value, int unit) => new(value, (MemoryUnit) unit);

        public static MemoryTierOptions CreateMemoryTierConfig(Capacity capacity)
            => new() { Capacity = capacity };

        public static BTreeIndexOptions CreateBTreeIndexConfig(Capacity pageSize, MemoryTierOptions options)
            => new() { PageSize = pageSize, MemoryTier = options };

        public static DataPersistenceOptions CreateDataPersistenceConfig(bool enabled, bool fsync)
            => new() { Enabled = enabled, Fsync = fsync };

        public static DurationOptions CreateDurationConfig(long durationAmount, int timeUnit)
            => new(durationAmount, (TimeUnit) timeUnit);

        public static MerkleTreeOptions CreateMerkleTreeConfig(bool enabled, int depth, bool isEnabledSetExists, bool enabledSet)
        {
            var config = new MerkleTreeOptions { Depth = depth };
            if (!isEnabledSetExists || enabledSet) config.Enabled = enabled;
            return config;
        }

        public static EventJournalOptions CreateEventJournalConfig(bool enabled, int capacity, int timeToLiveSeconds)
            => new() { Enabled = enabled, Capacity = capacity, TimeToLiveSeconds = timeToLiveSeconds };

        public static TieredStoreOptions CreateTieredStoreConfig(bool enabled, MemoryTierOptions memoryTierConfig, DiskTierOptions diskTierConfig)
            => new() { Enabled = enabled, MemoryTier = memoryTierConfig, DiskTier = diskTierConfig };

        public static HotRestartOptions CreateHotRestartConfig(bool enabled, bool fsync)
            => new() { Enabled = enabled, Fsync = fsync };

        public static NearCachePreloaderOptions CreateNearCachePreloaderConfig(bool enabled, string directory, int storeInitialDelaySeconds, int storeIntervalSeconds)
            => new() { Enabled = enabled, Directory = directory, StoreInitialDelaySeconds = storeInitialDelaySeconds, StoreIntervalSeconds = storeIntervalSeconds };

        public static TimedExpiryPolicyFactoryOptions CreateTimedExpiryPolicyFactoryConfig(int expiryPolicyType, DurationOptions durationConfig)
            => new((ExpiryPolicyType) expiryPolicyType, durationConfig);

        public static CacheSimpleEntryListenerOptions CreateCacheSimpleEntryListenerConfig(bool oldValueRequired, bool synchronous, string cacheEntryListenerFactory, string cacheEntryEventFilterFactory)
            => new() { OldValueRequired = oldValueRequired, Synchronous = synchronous, CacheEntryListenerFactory = cacheEntryListenerFactory, CacheEntryEventFilterFactory = cacheEntryEventFilterFactory };

        public static DiskTierOptions CreateDiskTierConfig(bool enabled, string deviceName)
            => new() { Enabled = enabled, DeviceName = deviceName };
        public static ClusterVersion CreateVersion(byte major, byte minor) => new ClusterVersion(major, minor);
        public static VectorDocument<IData> CreateVectorDocument(IData value, List<VectorPairHolder> vectors)
        {
            var vectorValues = CodecUtil.ToVectorValues(vectors);
            return new VectorDocument<IData>(value, vectorValues);
        }
        public static VectorSearchResultEntry<IData, IData> CreateVectorSearchResult(IData key, IData value, float score, List<VectorPairHolder> vectors)
        {
            var vectorValues = CodecUtil.ToVectorValues(vectors);
            return new VectorSearchResultEntry<IData, IData>(key, value, vectorValues, score);
        }
        public static VectorSearchOptions CreateVectorSearchOptions(bool includeValue, bool includeVectors, int limit, Dictionary<string, string> hints)
        {
            return new VectorSearchOptions(includeValue, includeVectors, limit, hints);
        }
    }
}
