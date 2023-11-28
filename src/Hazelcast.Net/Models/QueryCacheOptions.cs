// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents configuration for the query cache.
/// </summary>
public class QueryCacheOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// By default, after reaching this minimum size, node immediately sends buffered events to the QueryCache.
    /// </summary>
    public const int DEFAULT_BATCH_SIZE = 1;

    /// <summary>
    /// By default, only buffer last <see cref="DEFAULT_BUFFER_SIZE"/> events fired from a partition.
    /// </summary>
    public const int DEFAULT_BUFFER_SIZE = 16;

    /// <summary>
    /// Default value of delay seconds which an event wait in the buffer of a node, before sending to the query cache.
    /// </summary>
    public const int DEFAULT_DELAY_SECONDS = 0;

    /// <summary>
    /// By default, also cache values of entries besides keys.
    /// </summary>
    public const bool DEFAULT_INCLUDE_VALUE = true;

    /// <summary>
    /// By default, execute an initial population query prior to creation of the query cache.
    /// </summary>
    public const bool DEFAULT_POPULATE = true;

    /// <summary>
    /// Default value of coalesce property.
    /// </summary>
    public const bool DEFAULT_COALESCE = false;

    /// <summary>
    /// Do not serialize given keys by default.
    /// </summary>
    public const bool DEFAULT_SERIALIZE_KEYS = false;

    /// <summary>
    /// By default, hold values of entries in the query cache as binary.
    /// </summary>
    public const InMemoryFormat DEFAULT_IN_MEMORY_FORMAT = InMemoryFormat.Binary;

    private int _batchSize = DEFAULT_BATCH_SIZE;
    private int _bufferSize = DEFAULT_BUFFER_SIZE;
    private int _delaySeconds = DEFAULT_DELAY_SECONDS;
    private bool _includeValue = DEFAULT_INCLUDE_VALUE;
    private bool _populate = DEFAULT_POPULATE;
    private bool _coalesce = DEFAULT_COALESCE;
    private bool _serializeKeys = DEFAULT_SERIALIZE_KEYS;
    private InMemoryFormat _inMemoryFormat = DEFAULT_IN_MEMORY_FORMAT;

    private string _name;
    private PredicateOptions _predicateConfig = new();
    private EvictionOptions _evictionConfig = new();
    private List<EntryListenerOptions> _entryListenerConfigs;
    private List<IndexOptions> _indexConfigs;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCacheOptions"/> class.
    /// </summary>
    public QueryCacheOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCacheOptions"/> class.
    /// </summary>
    public QueryCacheOptions(string name)
    {
        name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCacheOptions"/> class.
    /// </summary>
    public QueryCacheOptions(QueryCacheOptions other)
    {
        _batchSize = other._batchSize;
        _bufferSize = other._bufferSize;
        _delaySeconds = other._delaySeconds;
        _includeValue = other._includeValue;
        _populate = other._populate;
        _coalesce = other._coalesce;
        _inMemoryFormat = other._inMemoryFormat;
        _name = other._name;
        _predicateConfig = other._predicateConfig;
        _evictionConfig = other._evictionConfig;
        _entryListenerConfigs = other._entryListenerConfigs;
        _indexConfigs = other._indexConfigs;
    }

    /// <summary>
    /// Gets or sets the name of the query cache.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value.ThrowIfNullNorWhiteSpace();
    }

    /// <summary>
    /// Sets the name of the query cache.
    /// </summary>
    /// <param name="name">The name of the query cache.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    /// Gets or sets the predicate of the query cache.
    /// </summary>
    public PredicateOptions Predicate
    {
        get => _predicateConfig;
        set => _predicateConfig = value.ThrowIfNull();
    }

    /// <summary>
    /// Sets the predicate of the query cache.
    /// </summary>
    /// <param name="predicateConfig">The predicate of the query cache.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetPredicateConfig(PredicateOptions predicateConfig)
    {
        Predicate = predicateConfig;
        return this;
    }

    /// <summary>
    /// Gets or sets the batch size.
    /// </summary>
    public int BatchSize
    {
        get => _batchSize;
        set
        {
            if (value < 0) throw new ArgumentException("Value cannot be negative.", nameof(value));
            _batchSize = value;
        }
    }

    /// <summary>
    /// Sets the batch size.
    /// </summary>
    /// <param name="batchSize">The batch size.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetBatchSize(int batchSize)
    {
        BatchSize = batchSize;
        return this;
    }

    /// <summary>
    /// Gets or sets the maximum number of events which can be stored in a buffer of partition.
    /// </summary>
    public int BufferSize
    {
        get => _bufferSize;
        set
        {
            if (value < 0) throw new ArgumentException("Value cannot be negative.", nameof(value));
            _bufferSize = value;
        }
    }

    /// <summary>
    /// Sets the maximum number of events which can be stored in a buffer of partition.
    /// </summary>
    /// <param name="bufferSize">The maximum number of events which can be stored in a buffer of partition.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetBufferSize(int bufferSize)
    {
        BufferSize = bufferSize;
        return this;
    }

    /// <summary>
    /// Gets or sets the minimum number of delay seconds which an event waits in the buffer of node before sending to a query cache.
    /// </summary>
    public int DelaySeconds
    {
        get => _delaySeconds;
        set => _delaySeconds = value.ThrowIfLessThanZero();
    }

    /// <summary>
    /// Sets the minimum number of delay seconds which an event waits in the buffer of node before sending to a query cache.
    /// </summary>
    /// <param name="delaySeconds">The minimum number of delay seconds which an event waits in the buffer of node before sending to a query cache.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetDelaySeconds(int delaySeconds)
    {
        _delaySeconds = delaySeconds;
        return this;
    }

    /// <summary>
    /// Gets or sets the memory format of values of entries in the query cache.
    /// </summary>
    public InMemoryFormat InMemoryFormat
    {
        get => _inMemoryFormat;
        set
        {
            if (value == InMemoryFormat.Native)
                throw new ArgumentException("NATIVE memory format is not supported.", nameof(value));
            _inMemoryFormat = value.ThrowIfUndefined();
        }
    }

    /// <summary>
    /// Sets the memory format of values of entries in the query cache.
    /// </summary>
    /// <param name="inMemoryFormat">The memory format of values of entries in the query cache.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetInMemoryFormat(InMemoryFormat inMemoryFormat)
    {
        InMemoryFormat = inMemoryFormat;
        return this;
    }

    /// <summary>
    /// Whether value caching is enabled.
    /// </summary>
    public bool IncludeValue // FIXME name?!
    {
        get => _includeValue;
        set => _includeValue = value;
    }

    /// <summary>
    /// Sets whether value caching is enabled.
    /// </summary>
    /// <param name="includeValue">Whether value caching is enabled.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetIncludeValue(bool includeValue)
    {
        IncludeValue = includeValue;
        return this;
    }

    /// <summary>
    /// Whether initial population of the query cache is enabled.
    /// </summary>
    public bool IsPopulate // FIXME name?!
    {
        get => _populate;
        set => _populate = value;
    }

    /// <summary>
    /// Sets whether initial population of the query cache is enabled.
    /// </summary>
    /// <param name="populate">Whether initial population of the query cache is enabled.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetPopulate(bool populate)
    {
        IsPopulate = populate;
        return this;
    }

    /// <summary>
    /// Whether coalescing is enabled.
    /// </summary>
    public bool Coalesce // FIXME CoalesceEnabled ?!
    {
        get => _coalesce;
        set => _coalesce = value;
    }

    /// <summary>
    /// Sets whether coalescing is enabled.
    /// </summary>
    /// <param name="coalesce">Whether coalescing is is enabled.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetCoalesce(bool coalesce)
    {
        Coalesce = coalesce;
        return this;
    }

    /// <summary>
    /// Whether the query cache key is stored in serialized format (or by-reference).
    /// </summary>
    public bool IsSerializeKeys // FIXME name?
    {
        get => _serializeKeys;
        set => _serializeKeys = value;
    }

    /// <summary>
    /// Sets whether the query cache key is stored in serialized format (or by-reference).
    /// </summary>
    /// <param name="serializeKeys">Whether the query cache key is stored in serialized format (or by-reference).</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetSerializeKeys(bool serializeKeys)
    {
        IsSerializeKeys = serializeKeys;
        return this;
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
    /// Sets the eviction configuration.
    /// </summary>
    /// <param name="evictionConfig">The eviction configuration.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetEvictionConfig(EvictionOptions evictionConfig)
    {
        Eviction = evictionConfig;
        return this;
    }

    /// <summary>
    /// Adds an entry listener configuration.
    /// </summary>
    /// <param name="listenerConfig">The entry listener configuration to add.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions AddEntryListenerConfig(EntryListenerOptions listenerConfig)
    {
        EntryListeners.Add(listenerConfig);
        return this;
    }

    /// <summary>
    /// Gets or sets the entry listener configurations.
    /// </summary>
    public List<EntryListenerOptions> EntryListeners
    {
        get => _entryListenerConfigs ??= new();
        set => _entryListenerConfigs = value.ThrowIfNull();
    }

    /// <summary>
    /// Sets the entry listener configurations.
    /// </summary>
    /// <param name="listenerConfigs">The entry listener configurations.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetEntryListenerConfigs(List<EntryListenerOptions> listenerConfigs)
    {
        EntryListeners = listenerConfigs;
        return this;
    }

    /// <summary>
    /// Adds an index configuration.
    /// </summary>
    /// <param name="indexConfig">The index configuration to add.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions AddIndexConfig(IndexOptions indexConfig)
    {
        Indexes.Add(indexConfig);
        return this;
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
    /// Sets the index configurations.
    /// </summary>
    /// <param name="indexConfigs">The index configurations.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions SetIndexConfigs(List<IndexOptions> indexConfigs)
    {
        Indexes = indexConfigs;
        return this;
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.QueryCacheConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteInt(_batchSize);
        output.WriteInt(_bufferSize);
        output.WriteInt(_delaySeconds);
        output.WriteBoolean(_includeValue);
        output.WriteBoolean(_populate);
        output.WriteBoolean(_coalesce);
        output.WriteString(_inMemoryFormat.ToJavaString());
        output.WriteString(_name);
        output.WriteObject(_predicateConfig);
        output.WriteObject(_evictionConfig);
        output.WriteNullableList(_entryListenerConfigs);
        output.WriteNullableList(_indexConfigs);
        output.WriteBoolean(_serializeKeys);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _batchSize = input.ReadInt();
        _bufferSize = input.ReadInt();
        _delaySeconds = input.ReadInt();
        _includeValue = input.ReadBoolean();
        _populate = input.ReadBoolean();
        _coalesce = input.ReadBoolean();
        _inMemoryFormat = Enums.ParseJava<InMemoryFormat>(input.ReadString());
        _name = input.ReadString();
        _predicateConfig = input.ReadObject<PredicateOptions>();
        _evictionConfig = input.ReadObject<EvictionOptions>();
        _entryListenerConfigs = input.ReadNullableList<EntryListenerOptions>();
        _indexConfigs = input.ReadNullableList<IndexOptions>();
        _serializeKeys = input.ReadBoolean();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "QueryCacheConfig{"
                + "batchSize=" + _batchSize
                + ", bufferSize=" + _bufferSize
                + ", delaySeconds=" + _delaySeconds
                + ", includeValue=" + _includeValue
                + ", populate=" + _populate
                + ", coalesce=" + _coalesce
                + ", serializeKeys=" + _serializeKeys
                + ", inMemoryFormat=" + _inMemoryFormat
                + ", name='" + _name + '\''
                + ", predicateConfig=" + _predicateConfig
                + ", evictionConfig=" + _evictionConfig
                + ", entryListenerConfigs=" + _entryListenerConfigs
                + ", indexConfigs=" + _indexConfigs
                + "}";
    }
}