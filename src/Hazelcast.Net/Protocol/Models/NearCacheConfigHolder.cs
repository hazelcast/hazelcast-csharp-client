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

using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.NearCaching;

namespace Hazelcast.Protocol.Models;

internal class NearCacheConfigHolder
{
    public NearCacheConfigHolder(string name, string inMemoryFormat, bool serializeKeys,
                                 bool invalidateOnChange, int timeToLiveSeconds, int maxIdleSeconds,
                                 EvictionConfigHolder evictionConfigHolder, bool cacheLocalEntries,
                                 string localUpdatePolicy, NearCachePreloaderOptions preloaderConfig)
    {
        Name = name;
        InMemoryFormat = inMemoryFormat;
        IsSerializeKeys = serializeKeys;
        IsInvalidateOnChange = invalidateOnChange;
        TimeToLiveSeconds = timeToLiveSeconds;
        MaxIdleSeconds = maxIdleSeconds;
        EvictionConfigHolder = evictionConfigHolder;
        IsCacheLocalEntries = cacheLocalEntries;
        LocalUpdatePolicy = localUpdatePolicy;
        PreloaderConfig = preloaderConfig;
    }

    public string Name { get; set; }

    public string InMemoryFormat { get; set; }

    public bool IsSerializeKeys { get; set; }

    public bool IsInvalidateOnChange { get; set; }

    public int TimeToLiveSeconds { get; set; }

    public int MaxIdleSeconds { get; set; }

    public EvictionConfigHolder EvictionConfigHolder { get; set; }

    public bool IsCacheLocalEntries { get; set; }

    public string LocalUpdatePolicy { get; set; }

    public NearCachePreloaderOptions PreloaderConfig { get; set; }

    public NearCacheOptions ToNearCacheConfig()
    {
        var config = new NearCacheOptions
        {
            Name = Name,
            InMemoryFormat = Enums.ParseJava<InMemoryFormat>(InMemoryFormat),
            SerializeKeys = IsSerializeKeys,
            InvalidateOnChange = IsInvalidateOnChange,
            TimeToLiveSeconds = TimeToLiveSeconds,
            MaxIdleSeconds = MaxIdleSeconds,
            Eviction = EvictionConfigHolder.ToEvictionConfig(),
            CacheLocalEntries = IsCacheLocalEntries,
            LocalUpdatePolicy = Enums.ParseJava<UpdatePolicy>(LocalUpdatePolicy),
            Preloader = PreloaderConfig
        };
        return config;
    }

    public static NearCacheConfigHolder Of(NearCacheOptions config)
    {
        if (config == null)
            return null;

        return new NearCacheConfigHolder(config.Name, config.InMemoryFormat.ToJavaString(), config.SerializeKeys,
                config.InvalidateOnChange, config.TimeToLiveSeconds, config.MaxIdleSeconds,
                EvictionConfigHolder.Of(config.Eviction),
                config.CacheLocalEntries, config.LocalUpdatePolicy.ToJavaString(), config.Preloader);
    }
}