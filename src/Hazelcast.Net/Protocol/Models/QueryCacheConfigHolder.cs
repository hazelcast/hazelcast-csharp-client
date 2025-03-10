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
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.Models;

internal class QueryCacheConfigHolder
{
    private bool? _isSerializeKeys;

    public QueryCacheConfigHolder()
    { }

    public QueryCacheConfigHolder(int batchSize, int bufferSize, int delaySeconds, bool includeValue,
                                  bool populate, bool coalesce, string inMemoryFormat, string name,
                                  PredicateConfigHolder predicateConfigHolder,
                                  EvictionConfigHolder evictionConfigHolder, List<ListenerConfigHolder> listenerConfigs,
                                  List<IndexOptions> indexConfigs, bool serializeKeysExist, bool serializeKeys)
    {
        BatchSize = batchSize;
        BufferSize = bufferSize;
        DelaySeconds = delaySeconds;
        IsIncludeValue = includeValue;
        IsPopulate = populate;
        IsCoalesce = coalesce;
        _isSerializeKeys = serializeKeysExist ? serializeKeys : null;
        InMemoryFormat = inMemoryFormat;
        Name = name;
        PredicateConfigHolder = predicateConfigHolder;
        EvictionConfigHolder = evictionConfigHolder;
        ListenerConfigs = listenerConfigs;
        IndexConfigs = indexConfigs;
    }

    public int BatchSize { get; set; }

    public int BufferSize { get; set; }

    public int DelaySeconds { get; set; }

    public bool IsIncludeValue { get; set; }

    public bool IsPopulate { get; set; }

    public bool IsSerializeKeys
    {
        get => _isSerializeKeys ?? false;
        set => _isSerializeKeys = value;
    }

    public bool IsCoalesce { get; set; }

    public string InMemoryFormat { get; set; }

    public string Name { get; set; }
    public PredicateConfigHolder PredicateConfigHolder { get; set; }

    public EvictionConfigHolder EvictionConfigHolder { get; set; }

    public List<ListenerConfigHolder> ListenerConfigs { get; set; }

    public List<IndexOptions> IndexConfigs { get; set; }

    public QueryCacheOptions ToQueryCacheConfig(SerializationService serializationService)
    {
        var config = new QueryCacheOptions
        {
            BatchSize = BatchSize,
            BufferSize = BufferSize,
            Coalesce = IsCoalesce,
            DelaySeconds = DelaySeconds,
            Eviction = EvictionConfigHolder.ToEvictionConfig(),
            EntryListeners = ListenerConfigs is {Count: > 0}
                ? ListenerConfigs.Map(x => x.ToListenerConfig<EntryListenerOptions>()) 
                : new(),
            IncludeValue = IsIncludeValue,
            InMemoryFormat = Enums.ParseJava<InMemoryFormat>(InMemoryFormat),
            Indexes = IndexConfigs ?? new List<IndexOptions>(),
            Name = Name,
            Predicate = PredicateConfigHolder.ToPredicateConfig(),
            Populate = IsPopulate
        };

        if (_isSerializeKeys.HasValue)
            config.SerializeKeys = _isSerializeKeys.Value;

        return config;
    }

    public static QueryCacheConfigHolder Of(QueryCacheOptions config, SerializationService serializationService)
    {
        var holder = new QueryCacheConfigHolder
            {
                BatchSize = config.BatchSize,
                BufferSize = config.BufferSize,
                IsCoalesce = config.Coalesce,
                DelaySeconds = config.DelaySeconds,
                EvictionConfigHolder = EvictionConfigHolder.Of(config.Eviction),
                IsIncludeValue = config.IncludeValue,
                InMemoryFormat = config.InMemoryFormat.ToJavaString(),
                Name = config.Name,
            };

        if (config.Indexes is {Count: > 0})
            holder.IndexConfigs = config.Indexes.Map(x => new IndexOptions(x));

        if (config.EntryListeners is {Count: > 0})
            holder.ListenerConfigs = config.EntryListeners.Map(ListenerConfigHolder.Of);

        holder.PredicateConfigHolder = PredicateConfigHolder.Of(config.Predicate);
        holder.IsPopulate = config.Populate;
        holder.IsSerializeKeys = config.SerializeKeys;

        return holder;
    }
}