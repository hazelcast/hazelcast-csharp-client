// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents options for map.
/// </summary>
[SuppressMessage("Design", "CA1002:Do not expose generic lists")] // cannot change public APIs
public class MapOptions : IIdentifiedDataSerializable, INamedOptions
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default In-Memory format.
        /// </summary>
        public const InMemoryFormat InMemoryFormat = Core.InMemoryFormat.Binary;

        /// <summary>
        /// Gets the default number of backups
        /// </summary>
        public const int BackupCount = 1;

        /// <summary>
        /// Gets the minimum number of backups.
        /// </summary>
        public const int MinBackupCount = 0;

        /// <summary>
        /// Gets the maximum number of backups
        /// </summary>
        public const int MaxBackupCount = Constants.PartitionMaxBackupCount;

        /// <summary>
        /// Gets the default number of async backups
        /// </summary>
        public const int AsyncBackupCount = MinBackupCount;

        /// <summary>
        /// Gets the number of default Time to Live in seconds.
        /// </summary>
        public const int TtlSeconds = DisabledTtlSeconds;

        /// <summary>
        /// Gets the number of Time to Live that represents disabling TTL.
        /// </summary>
        public const int DisabledTtlSeconds = 0;

        /// <summary>
        /// Gets the default ReadBackupData value.
        /// </summary>
        public const bool ReadBackupData = false;

        /// <summary>
        /// Gets the number of default time to wait eviction in seconds.
        /// </summary>
        public const int MaxIdleSeconds = 0;

        /// <summary>
        /// Gets the default value cache policy.
        /// </summary>
        public const CacheDeserializedValues CachedDeserializedValues = CacheDeserializedValues.IndexOnly;

        /// <summary>
        /// Gets the default metadata policy
        /// </summary>
        public const MetadataPolicy MetadataPolicy = Models.MetadataPolicy.CreateOnUpdate;

        /// <summary>
        /// Gets the default value of whether statistics are enabled or not
        /// </summary>
        public const bool StatisticsEnabled = true;

        /// <summary>
        /// Gets the default value of whether per entry statistics are enabled or not
        /// </summary>
        public const bool EntryStatsEnabled = false;

        /// <summary>
        /// Gets the default split brain protection name.
        /// </summary>
        public const string SplitBrainProtectionName = null;
    }
#pragma warning restore CA1034

    private string _name;
    private string _splitBrainProtectionName;

    private bool _readBackupData = Defaults.ReadBackupData;
    private bool _statisticsEnabled = Defaults.StatisticsEnabled;
    private bool _perEntryStatsEnabled = Defaults.EntryStatsEnabled;
    private int _backupCount = Defaults.BackupCount;
    private int _asyncBackupCount = Defaults.AsyncBackupCount;
    private int _timeToLiveSeconds = Defaults.TtlSeconds;
    private int _maxIdleSeconds = Defaults.MaxIdleSeconds;

    private CacheDeserializedValues _cacheDeserializedValues = Defaults.CachedDeserializedValues;
    private InMemoryFormat _inMemoryFormat = Defaults.InMemoryFormat;
    private MetadataPolicy _metadataPolicy = Defaults.MetadataPolicy;

    private MapStoreOptions _mapStoreConfig = new();
    private MergePolicyOptions _mergePolicyConfig = new();
    private HotRestartOptions _hotRestartConfig = new();
    private DataPersistenceOptions _dataPersistenceConfig = new();
    private MerkleTreeOptions _merkleTreeConfig = new();
    private EventJournalOptions _eventJournalConfig = new();
    private EvictionOptions _evictionConfig = new() { EvictionPolicy = EvictionPolicy.None, MaxSizePolicy = MaxSizePolicy.PerNode, Size = int.MaxValue };
    private TieredStoreOptions _tieredStoreConfig = new();

    private List<PartitioningAttributeOptions> _partitioningAttributeConfigs;
    private NearCacheOptions _nearCacheConfig;
    private WanReplicationRef _wanReplicationRef;
    private List<EntryListenerOptions> _entryListenerConfigs;
    private List<MapPartitionLostListenerOptions> _partitionLostListenerConfigs;
    private List<IndexOptions> _indexConfigs;
    private List<AttributeOptions> _attributeConfigs;
    private List<QueryCacheOptions> _queryCacheConfigs;
    private PartitioningStrategyOptions _partitioningStrategyConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapOptions"/> class.
    /// </summary>
    public MapOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapOptions"/> class.
    /// </summary>
    /// <param name="name"></param>
    public MapOptions(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapOptions"/> class.
    /// </summary>
    /// <param name="options"></param>
    public MapOptions(MapOptions options)
    {
        _name = options._name;
        _backupCount = options._backupCount;
        _asyncBackupCount = options._asyncBackupCount;
        _timeToLiveSeconds = options._timeToLiveSeconds;
        _maxIdleSeconds = options._maxIdleSeconds;
        _metadataPolicy = options._metadataPolicy;
        _evictionConfig = new EvictionOptions(options._evictionConfig);
        _inMemoryFormat = options._inMemoryFormat;
        _mapStoreConfig = options._mapStoreConfig != null ? new MapStoreOptions(options._mapStoreConfig) : null;
        _nearCacheConfig = options._nearCacheConfig != null ? new NearCacheOptions(options._nearCacheConfig) : null;
        _readBackupData = options._readBackupData;
        _cacheDeserializedValues = options._cacheDeserializedValues;
        _statisticsEnabled = options._statisticsEnabled;
        _perEntryStatsEnabled = options._perEntryStatsEnabled;
        _mergePolicyConfig = new MergePolicyOptions(options._mergePolicyConfig);
        _wanReplicationRef = options._wanReplicationRef != null ? new WanReplicationRef(options._wanReplicationRef) : null;
        _entryListenerConfigs = new List<EntryListenerOptions>(options._entryListenerConfigs);
        _partitionLostListenerConfigs = new List<MapPartitionLostListenerOptions>(options._partitionLostListenerConfigs);
        _indexConfigs = new List<IndexOptions>(options._indexConfigs);
        _attributeConfigs = new List<AttributeOptions>(options._attributeConfigs);
        _queryCacheConfigs = new List<QueryCacheOptions>(options._queryCacheConfigs);
        _partitioningStrategyConfig = options._partitioningStrategyConfig != null ? new PartitioningStrategyOptions(options._partitioningStrategyConfig) : null;
        _splitBrainProtectionName = options._splitBrainProtectionName;
        _hotRestartConfig = new HotRestartOptions(options._hotRestartConfig);
        _dataPersistenceConfig = new DataPersistenceOptions(options._dataPersistenceConfig);
        _merkleTreeConfig = new MerkleTreeOptions(options._merkleTreeConfig);
        _eventJournalConfig = new EventJournalOptions(options._eventJournalConfig);
        _tieredStoreConfig = new TieredStoreOptions(options._tieredStoreConfig);
        _partitioningAttributeConfigs = new List<PartitioningAttributeOptions>(options._partitioningAttributeConfigs);
    }

    /// <summary>
    /// Gets or sets the name of the map.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value.ThrowIfNullNorWhiteSpace();
    }

    /// <summary>
    /// Gets or sets the data type that will be used for storing records.
    /// </summary>
    public InMemoryFormat InMemoryFormat
    {
        get => _inMemoryFormat;
        set => _inMemoryFormat = value.ThrowIfUndefined();
    }

    /// <summary>
    /// Gets or sets the eviction configuration.
    /// </summary>
    public EvictionOptions Eviction
    {
        get => _evictionConfig;
        set => _evictionConfig = value.ThrowIfNull();
    }

    /// <summary>
    /// Gets or sets the backup count for this map.
    /// </summary>
    public int BackupCount
    {
        get => _backupCount;
        set => _backupCount = Preconditions.ValidateNewBackupCount(value, _asyncBackupCount);
    }

    /// <summary>
    /// Gets or sets the asynchronous backup count for this map.
    /// </summary>
    public int AsyncBackupCount
    {
        get => _asyncBackupCount;
        set => _asyncBackupCount = Preconditions.ValidateNewAsyncBackupCount(_backupCount, value);
    }

    /// <summary>
    /// Gets the total number of backups (BackupCount plus AsyncBackupCount).
    /// </summary>
    public int TotalBackupCount => _backupCount + _asyncBackupCount;

    /// <summary>
    /// Gets or sets the maximum number of seconds for each entry to stay in the map.
    /// </summary>
    public int TimeToLiveSeconds
    {
        get => _timeToLiveSeconds;
        set => _timeToLiveSeconds = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of seconds for each entry to stay idle in the map.
    /// </summary>
    public int MaxIdleSeconds
    {
        get => _maxIdleSeconds;
        set => _maxIdleSeconds = value;
    }

    /// <summary>
    /// Gets or sets the map store configuration.
    /// </summary>
    public MapStoreOptions MapStore
    {
        get => _mapStoreConfig;
        set => _mapStoreConfig = value;
    }

    /// <summary>
    /// Gets or sets the NearCache configuration.
    /// </summary>
    public NearCacheOptions NearCache
    {
        get => _nearCacheConfig;
        set => _nearCacheConfig = value;
    }

    /// <summary>
    /// Gets or sets the merge policy configuration.
    /// </summary>
    public MergePolicyOptions MergePolicy
    {
        get => _mergePolicyConfig;
        set => _mergePolicyConfig = value;
    }

    /// <summary>
    /// Determines whether statistics are enabled for this map.
    /// </summary>
    public bool StatisticsEnabled
    {
        get => _statisticsEnabled;
        set => _statisticsEnabled = value;
    }

    /// <summary>
    /// Determines whether entry level statistics are enabled for this map.
    /// </summary>
    public bool PerEntryStatsEnabled
    {
        get => _perEntryStatsEnabled;
        set => _perEntryStatsEnabled = value;
    }

    /// <summary>
    /// Determines whether read-backup-data (reading local backup entries) is enabled for this map.
    /// </summary>
    public bool ReadBackupData
    {
        get => _readBackupData;
        set => _readBackupData = value;
    }

    /// <summary>
    /// Gets or sets the WAN target replication reference.
    /// </summary>
    public WanReplicationRef WanReplicationRef
    {
        get => _wanReplicationRef;
        set => _wanReplicationRef = value;
    }

    /// <summary>
    /// Gets or sets the entry listener configurations.
    /// </summary>
    /// <returns>The entry listeners configurations.</returns>
    public List<EntryListenerOptions> EntryListeners
    {
        get => _entryListenerConfigs ??= new();
        set => _entryListenerConfigs = value;
    }

    /// <summary>
    /// Gets or sets the partition lost listener configurations.
    /// </summary>
    public List<MapPartitionLostListenerOptions> PartitionLostListeners
    {
        get => _partitionLostListenerConfigs ??= new();
        set => _partitionLostListenerConfigs = value;
    }

    /// <summary>
    /// Gets or sets the index configurations.
    /// </summary>
    public List<IndexOptions> Indexes
    {
        get => _indexConfigs ??= new();
        set => _indexConfigs = value;
    }

    /// <summary>
    /// Gets or sets the attribute configurations.
    /// </summary>
    public List<AttributeOptions> Attributes
    {
        get => _attributeConfigs ??= new();
        set => _attributeConfigs = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="MetadataPolicy"/> for this map.
    /// </summary>
    public MetadataPolicy MetadataPolicy
    {
        get => _metadataPolicy;
        set => _metadataPolicy = value;
    }

    /// <summary>
    /// Gets or sets the query cache configs.
    /// </summary>
    public List<QueryCacheOptions> QueryCaches
    {
        get => _queryCacheConfigs ??= new();
        set => _queryCacheConfigs = value;
    }

    /// <summary>
    /// Gets or sets the partitioning strategy config.
    /// </summary>
    /// <returns></returns>
    public PartitioningStrategyOptions PartitioningStrategy
    {
        get => _partitioningStrategyConfig;
        set => _partitioningStrategyConfig = value;
    }

    /// <summary>
    /// Whether Near Cache is enabled.
    /// </summary>
    public bool IsNearCacheEnabled => _nearCacheConfig != null;

    /// <summary>
    /// Gets or sets the hot restart config.
    /// </summary>
    [Obsolete("Use DataPersistenceConfig.", false)]
    public HotRestartOptions HotRestart
    {
        get => _hotRestartConfig;
        set
        {
            _hotRestartConfig = value.ThrowIfNull();
            Merge(_hotRestartConfig, _dataPersistenceConfig);
        }
    }

    /// <summary>
    /// Gets or sets the data persistence config.
    /// </summary>
    public DataPersistenceOptions DataPersistence
    {
        get => _dataPersistenceConfig;
        set
        {
            _dataPersistenceConfig = value.ThrowIfNull();
            Merge(_hotRestartConfig, _dataPersistenceConfig);
        }
    }

    /// <summary>
    /// Gets or sets the merkle tree config.
    /// </summary>
    public MerkleTreeOptions MerkleTree
    {
        get => _merkleTreeConfig;
        set => _merkleTreeConfig = value.ThrowIfNull();
    }

    /// <summary>
    /// Gets or sets the event journal config.
    /// </summary>
    public EventJournalOptions EventJournal
    {
        get => _eventJournalConfig;
        set => _eventJournalConfig = value.ThrowIfNull();
    }

    /// <summary>
    /// Gets or sets the tiered-store config.
    /// </summary>
    public TieredStoreOptions TieredStore
    {
        get => _tieredStoreConfig;
        set => _tieredStoreConfig = value.ThrowIfNull();
    }

    /// <summary>
    /// Gets the value cache settings.
    /// </summary>
    public CacheDeserializedValues CacheDeserializedValues
    {
        get => _cacheDeserializedValues;
        set => _cacheDeserializedValues = value;
    }

    /// <summary>
    /// Gets or sets the split brain protection name.
    /// </summary>
    public string SplitBrainProtectionName
    {
        get => _splitBrainProtectionName;
        set => _splitBrainProtectionName = value;
    }

    /// <summary>
    /// Gets or sets the partition attribute configurations.
    /// </summary>
    public List<PartitioningAttributeOptions> PartitioningAttributes
    {
        get => _partitioningAttributeConfigs ??= new();
        set => _partitioningAttributeConfigs = value.ThrowIfNull();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "MapConfig{"
                + "name='" + _name + '\''
                + ", inMemoryFormat='" + _inMemoryFormat + '\''
                + ", metadataPolicy=" + _metadataPolicy
                + ", backupCount=" + _backupCount
                + ", asyncBackupCount=" + _asyncBackupCount
                + ", timeToLiveSeconds=" + _timeToLiveSeconds
                + ", maxIdleSeconds=" + _maxIdleSeconds
                + ", readBackupData=" + _readBackupData
                + ", evictionConfig=" + _evictionConfig
                + ", merkleTree=" + _merkleTreeConfig
                + ", eventJournal=" + _eventJournalConfig
                + ", hotRestart=" + _hotRestartConfig
                + ", dataPersistenceConfig=" + _dataPersistenceConfig
                + ", nearCacheConfig=" + _nearCacheConfig
                + ", mapStoreConfig=" + _mapStoreConfig
                + ", mergePolicyConfig=" + _mergePolicyConfig
                + ", wanReplicationRef=" + _wanReplicationRef
                + ", entryListenerConfigs=" + _entryListenerConfigs
                + ", indexConfigs=" + _indexConfigs
                + ", attributeConfigs=" + _attributeConfigs
                + ", splitBrainProtectionName=" + _splitBrainProtectionName
                + ", queryCacheConfigs=" + _queryCacheConfigs
                + ", cacheDeserializedValues=" + _cacheDeserializedValues
                + ", statisticsEnabled=" + _statisticsEnabled
                + ", entryStatsEnabled=" + _perEntryStatsEnabled
                + ", tieredStoreConfig=" + _tieredStoreConfig
                + ", partitioningAttributeConfigs=" + _partitioningAttributeConfigs
                + '}';
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.MapConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_name);
        output.WriteInt(_backupCount);
        output.WriteInt(_asyncBackupCount);
        output.WriteInt(_timeToLiveSeconds);
        output.WriteInt(_maxIdleSeconds);
        output.WriteObject(_evictionConfig);
        output.WriteObject(_mapStoreConfig);
        output.WriteObject(_nearCacheConfig);
        output.WriteBoolean(_readBackupData);
        output.WriteString(_cacheDeserializedValues.ToJavaString());
        output.WriteObject(_mergePolicyConfig);
        output.WriteString(_inMemoryFormat.ToJavaString());
        output.WriteObject(_wanReplicationRef);
        output.WriteNullableList(_entryListenerConfigs);
        output.WriteNullableList(_partitionLostListenerConfigs);
        output.WriteNullableList(_indexConfigs);
        output.WriteNullableList(_attributeConfigs);
        output.WriteNullableList(_queryCacheConfigs);
        output.WriteBoolean(_statisticsEnabled);
        output.WriteObject(_partitioningStrategyConfig);
        output.WriteString(_splitBrainProtectionName);
        output.WriteObject(_hotRestartConfig);
        output.WriteObject(_merkleTreeConfig);
        output.WriteObject(_eventJournalConfig);
        output.WriteShort((short)_metadataPolicy);
        output.WriteBoolean(_perEntryStatsEnabled);
        output.WriteObject(_dataPersistenceConfig);
        output.WriteObject(_tieredStoreConfig);

        // Java writes this only if output.getVersion > 5.3 but this code will never run in < 5.4
        output.WriteNullableList(_partitioningAttributeConfigs);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _name = input.ReadString();
        _backupCount = input.ReadInt();
        _asyncBackupCount = input.ReadInt();
        _timeToLiveSeconds = input.ReadInt();
        _maxIdleSeconds = input.ReadInt();
        _evictionConfig = input.ReadObject<EvictionOptions>();
        _mapStoreConfig = input.ReadObject<MapStoreOptions>();
        _nearCacheConfig = input.ReadObject<NearCacheOptions>();
        _readBackupData = input.ReadBoolean();
        _cacheDeserializedValues = Enums.ParseJava<CacheDeserializedValues>(input.ReadString());
        _mergePolicyConfig = input.ReadObject<MergePolicyOptions>();
        _inMemoryFormat = Enums.ParseJava<InMemoryFormat>(input.ReadString());
        _wanReplicationRef = input.ReadObject<WanReplicationRef>();
        _entryListenerConfigs = input.ReadNullableList<EntryListenerOptions>();
        _partitionLostListenerConfigs = input.ReadNullableList<MapPartitionLostListenerOptions>();
        _indexConfigs = input.ReadNullableList<IndexOptions>();
        _attributeConfigs = input.ReadNullableList<AttributeOptions>();
        _queryCacheConfigs = input.ReadNullableList<QueryCacheOptions>();
        _statisticsEnabled = input.ReadBoolean();
        _partitioningStrategyConfig = input.ReadObject<PartitioningStrategyOptions>();
        _splitBrainProtectionName = input.ReadString();
        HotRestart = input.ReadObject<HotRestartOptions>(); // property !
        _merkleTreeConfig = input.ReadObject<MerkleTreeOptions>();
        _eventJournalConfig = input.ReadObject<EventJournalOptions>();
        _metadataPolicy = ((MetadataPolicy)input.ReadShort()).ThrowIfUndefined();
        _perEntryStatsEnabled = input.ReadBoolean();
        DataPersistence = input.ReadObject<DataPersistenceOptions>(); // property!
        TieredStore = input.ReadObject<TieredStoreOptions>(); // property!

        // reads writes this only if output.getVersion > 5.3 but this code will never run in < 5.4
        _partitioningAttributeConfigs = input.ReadNullableList<PartitioningAttributeOptions>();
    }


    // Java says:

    /**
     * if hot-restart: enabled="true" and data-persistence: enabled="false"
     * => enable persistence and use the config from hot-restart.
     * Does not break current deployments.
     *
     * if hot-restart: enabled="false" and data-persistence: enabled="true"
     * => enable persistence and use the config from data-persistence. This is
     * for the new users.
     *
     * if hot-restart: enabled="true" and data-persistence: enabled="true"
     * => enable persistence and use the config from data-persistence. We prefer
     * the new element, and the old one might get removed at some point.
     *
     * if hot-restart: enabled="false" and data-persistence: enabled="false"
     * => we still do override hot-restart using data-persistence.
     * It is necessary to maintain equality consistency.
     */

    private static void Merge(HotRestartOptions hotRestartConfig, DataPersistenceOptions dataPersistenceConfig)
    {
        if (Equals(hotRestartConfig, dataPersistenceConfig))
        {
            return;
        }

        if (hotRestartConfig.Enabled && !dataPersistenceConfig.Enabled)
        {
            dataPersistenceConfig.Enabled = true;
            dataPersistenceConfig.Fsync = hotRestartConfig.Fsync;
            return;
        }

        //var over = hotRestartConfig.Enabled && dataPersistenceConfig.Enabled;

        hotRestartConfig.SetIsEnabled(dataPersistenceConfig.Enabled).SetIsFsync(dataPersistenceConfig.Fsync);

        // Java logs, we don't.
        //if (over) {
        //    LOGGER.warning(
        //            "Please note that HotRestart is deprecated and should not be used. "
        //            + "Since both HotRestart and DataPersistence are enabled, "
        //            + "and thus there is a conflict, the latter is used in persistence configuration."
        //    );
        //}
    }

    private static bool Equals(HotRestartOptions hotRestartConfig, DataPersistenceOptions dataPersistenceConfig)
    {
        return hotRestartConfig.Enabled == dataPersistenceConfig.Enabled &&
               hotRestartConfig.Fsync == dataPersistenceConfig.Fsync;
    }
}