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
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// By default, after reaching this minimum size, node immediately sends buffered events to the QueryCache.
        /// </summary>
        public const int BatchSize = 1;

        /// <summary>
        /// By default, only buffer last <see cref="BufferSize"/> events fired from a partition.
        /// </summary>
        public const int BufferSize = 16;

        /// <summary>
        /// Default value of delay seconds which an event wait in the buffer of a node, before sending to the query cache.
        /// </summary>
        public const int DelaySeconds = 0;

        /// <summary>
        /// By default, also cache values of entries besides keys.
        /// </summary>
        public const bool IncludeValue = true;

        /// <summary>
        /// By default, execute an initial population query prior to creation of the query cache.
        /// </summary>
        public const bool Populate = true;

        /// <summary>
        /// Default value of coalesce property.
        /// </summary>
        public const bool Coalesce = false;

        /// <summary>
        /// Do not serialize given keys by default.
        /// </summary>
        public const bool SerializeKeys = false;

        /// <summary>
        /// By default, hold values of entries in the query cache as binary.
        /// </summary>
        public const InMemoryFormat InMemoryFormat = Core.InMemoryFormat.Binary;
    }
#pragma warning restore CA1034

    private int _batchSize = Defaults.BatchSize;
    private int _bufferSize = Defaults.BufferSize;
    private int _delaySeconds = Defaults.DelaySeconds;
    private bool _includeValue = Defaults.IncludeValue;
    private bool _populate = Defaults.Populate;
    private bool _coalesce = Defaults.Coalesce;
    private bool _serializeKeys = Defaults.SerializeKeys;
    private InMemoryFormat _inMemoryFormat = Defaults.InMemoryFormat;

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
    /// Gets or sets the predicate of the query cache.
    /// </summary>
    public PredicateOptions Predicate
    {
        get => _predicateConfig;
        set => _predicateConfig = value.ThrowIfNull();
    }

    /// <summary>
    /// Gets or sets the batch size.
    /// </summary>
    public int BatchSize
    {
        get => _batchSize;
        set => _batchSize = value.ThrowIfLessThanZero();
    }

    /// <summary>
    /// Gets or sets the maximum number of events which can be stored in a buffer of partition.
    /// </summary>
    public int BufferSize
    {
        get => _bufferSize;
        set => _bufferSize = value.ThrowIfLessThanZero();
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
    /// Whether value caching is enabled.
    /// </summary>
    public bool IncludeValue
    {
        get => _includeValue;
        set => _includeValue = value;
    }

    /// <summary>
    /// Whether initial population of the query cache is enabled.
    /// </summary>
    public bool Populate
    {
        get => _populate;
        set => _populate = value;
    }


    /// <summary>
    /// Whether coalescing is enabled.
    /// </summary>
    public bool Coalesce
    {
        get => _coalesce;
        set => _coalesce = value;
    }

    /// <summary>
    /// Whether the query cache key is stored in serialized format (or by-reference).
    /// </summary>
    public bool SerializeKeys
    {
        get => _serializeKeys;
        set => _serializeKeys = value;
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
    /// Adds an entry listener configuration.
    /// </summary>
    /// <param name="listenerConfig">The entry listener configuration to add.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions AddEntryListener(EntryListenerOptions listenerConfig)
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
    /// Adds an index configuration.
    /// </summary>
    /// <param name="indexConfig">The index configuration to add.</param>
    /// <returns>This instance.</returns>
    public QueryCacheOptions AddIndex(IndexOptions indexConfig)
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