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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;

namespace Hazelcast.NearCaching;

/// <summary>
/// Represents the configuration of a near cache.
/// </summary>
public class NearCacheOptions : IIdentifiedDataSerializable, INamedOptions
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Default in-memory format.
        /// </summary>
        public const InMemoryFormat MemoryFormat = InMemoryFormat.Binary;

        /// <summary>
        /// Whether to serialize keys by default.
        /// </summary>
        public const bool SerializeKeys = false;

        /// <summary>
        /// Whether to invalidate on change by default.
        /// </summary>
        public const bool InvalidateOnChange = true;

        /// <summary>
        /// Default local update policy.
        /// </summary>
        public const UpdatePolicy LocalUpdatePolicy = UpdatePolicy.Invalidate;

        /// <summary>
        /// Default value of the time to live in seconds.
        /// </summary>
        public const int TtlSeconds = 0;

        /// <summary>
        /// Default value of the maximum idle time for eviction in seconds.
        /// </summary>
        public const int MaxIdleSeconds = 0;

        /// <summary>
        /// Default name.
        /// </summary>
        public const string Name = "default";
    }
#pragma warning restore CA1034

    private string _name = Defaults.Name;
    private bool _cacheLocalEntries;
    private bool _serializeKeys = Defaults.SerializeKeys;
    private bool _invalidateOnChange = Defaults.InvalidateOnChange;
    private int _timeToLiveSeconds = Defaults.TtlSeconds;
    private int _maxIdleSeconds = Defaults.MaxIdleSeconds;
    private EvictionOptions _evictionConfig = new();
    private InMemoryFormat _inMemoryFormat = Defaults.MemoryFormat;
    private UpdatePolicy _localUpdatePolicy = Defaults.LocalUpdatePolicy;
    private NearCachePreloaderOptions _preloaderConfig = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="NearCacheOptions"/> class.
    /// </summary>
    public NearCacheOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NearCacheOptions"/> class.
    /// </summary>
    public NearCacheOptions(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NearCacheOptions"/> class.
    /// </summary>
    public NearCacheOptions([NotNull] NearCacheOptions config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        _name = config._name;
        _cacheLocalEntries = config._cacheLocalEntries;
        _serializeKeys = config._serializeKeys;
        _invalidateOnChange = config._invalidateOnChange;
        _timeToLiveSeconds = config._timeToLiveSeconds;
        _maxIdleSeconds = config._maxIdleSeconds;
        _evictionConfig = new EvictionOptions(config._evictionConfig);
        _inMemoryFormat = config._inMemoryFormat;
        _localUpdatePolicy = config._localUpdatePolicy;
        _preloaderConfig = new NearCachePreloaderOptions(config._preloaderConfig);
    }

    /// <summary>
    /// Gets or sets the name of the cache.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value.ThrowIfNull();
    }

    /// <summary>
    /// Gets or sets the eviction policy.
    /// </summary>
    [Obsolete("Use Eviction.EvictionPolicy.", false)]
    public EvictionPolicy EvictionPolicy
    {
        get => _evictionConfig.EvictionPolicy;
        set => _evictionConfig.EvictionPolicy = value;
    }

    /// <summary>
    /// Gets or sets the maximum size of the cache (number of entries) before entries get evicted.
    /// </summary>
    [Obsolete("Use Eviction.Size.", false)]
    public int MaxSize
    {
        get => _evictionConfig.Size;
        set => _evictionConfig.Size = value;
    }

    /// <summary>
    /// Gets or sets the eviction percentage.
    /// </summary>
    /// <remarks>
    /// <para>This is internal for now, corresponds to Java system properties.</para>
    /// </remarks>
    internal int EvictionPercentage { get; set; } = 20;

    /// <summary>
    /// Gets or sets the period of the cleanup.
    /// </summary>
    /// <remarks>
    /// <para>This is internal for now, corresponds to Java system properties.</para>
    /// </remarks>
    internal int CleanupPeriodSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the in-memory format.
    /// </summary>
    public InMemoryFormat InMemoryFormat
    {
        get => _inMemoryFormat;
        set => _inMemoryFormat = value.ThrowIfUndefined();
    }

    /// <summary>
    /// Whether the key is stored in serialized format or by-reference.
    /// </summary>
    public bool SerializeKeys
    {
        get => _serializeKeys || _inMemoryFormat == InMemoryFormat.Native;
        set => _serializeKeys = value;
    }

    /// <summary>
    /// Whether cache entries are invalidated when the entries in the backing data structure are changed (updated or removed).
    /// </summary>
    public bool InvalidateOnChange
    {
        get => _invalidateOnChange;
        set => _invalidateOnChange = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of seconds for each entry to stay in the Near Cache (time to live).
    /// </summary>
    public int TimeToLiveSeconds
    {
        get => _timeToLiveSeconds;
        set => _timeToLiveSeconds = value.ThrowIfLessThanZero();
    }

    /// <summary>
    /// Gets or sets the maximum number of seconds each entry can stay in the Near Cache as untouched (not-read).
    /// </summary>
    public int MaxIdleSeconds
    {
        get => _maxIdleSeconds;
        set => _maxIdleSeconds = value.ThrowIfLessThanZero();
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
    /// Whether local entries are also cached in the near cache.
    /// </summary>
    public bool CacheLocalEntries
    {
        get => _cacheLocalEntries;
        set => _cacheLocalEntries = value;
    }

    /// <summary>
    /// Gets or sets the local update policy.
    /// </summary>
    public UpdatePolicy LocalUpdatePolicy
    {
        get => _localUpdatePolicy;
        set => _localUpdatePolicy = value.ThrowIfUndefined();
    }

    /// <summary>
    /// Gets or sets the pre-loader configuration.
    /// </summary>
    public NearCachePreloaderOptions Preloader
    {
        get => _preloaderConfig;
        set => _preloaderConfig = value.ThrowIfNull();
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.NearCacheConfig;

    /// <inheritdoc />
    public void WriteData([NotNull] IObjectDataOutput output)
    {
        output.WriteString(_name);
        output.WriteInt(_timeToLiveSeconds);
        output.WriteInt(_maxIdleSeconds);
        output.WriteBoolean(_invalidateOnChange);
        output.WriteBoolean(_cacheLocalEntries);
        output.WriteInt((int)_inMemoryFormat);
        output.WriteInt((int)_localUpdatePolicy);
        output.WriteObject(_evictionConfig);
        output.WriteObject(_preloaderConfig);
    }

    /// <inheritdoc />
    public void ReadData([NotNull] IObjectDataInput input)
    {
        _name = input.ReadString();
        _timeToLiveSeconds = input.ReadInt();
        _maxIdleSeconds = input.ReadInt();
        _invalidateOnChange = input.ReadBoolean();
        _cacheLocalEntries = input.ReadBoolean();
        _inMemoryFormat = ((InMemoryFormat)input.ReadInt()).ThrowIfUndefined();
        _localUpdatePolicy = ((UpdatePolicy)input.ReadInt()).ThrowIfUndefined();
        _evictionConfig = input.ReadObject<EvictionOptions>();
        _preloaderConfig = input.ReadObject<NearCachePreloaderOptions>();
    }

    /// <summary>
    /// Clone.
    /// </summary>
    internal NearCacheOptions Clone() => new(this);

    /// <inheritdoc />
    public override string ToString()
    {
        return "NearCacheConfig{"
                + "name=" + _name
                + ", inMemoryFormat=" + _inMemoryFormat
                + ", invalidateOnChange=" + _invalidateOnChange
                + ", timeToLiveSeconds=" + _timeToLiveSeconds
                + ", maxIdleSeconds=" + _maxIdleSeconds
                + ", evictionConfig=" + _evictionConfig
                + ", cacheLocalEntries=" + _cacheLocalEntries
                + ", localUpdatePolicy=" + _localUpdatePolicy
                + ", preloaderConfig=" + _preloaderConfig.ToString()
                + '}';
    }
}