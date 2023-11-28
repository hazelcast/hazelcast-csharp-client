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

using Hazelcast.Configuration;
using Hazelcast.Serialization;
using System;

namespace Hazelcast.Models;

public class CacheSimpleEntryListenerOptions : IIdentifiedDataSerializable
{

    private string _cacheEntryListenerFactory;
    private string _cacheEntryEventFilterFactory;
    private bool _oldValueRequired;
    private bool _synchronous;

    public CacheSimpleEntryListenerOptions()
    { }

    public CacheSimpleEntryListenerOptions(CacheSimpleEntryListenerOptions listenerConfig)
    {
        _cacheEntryEventFilterFactory = listenerConfig._cacheEntryEventFilterFactory;
        _cacheEntryListenerFactory = listenerConfig._cacheEntryListenerFactory;
        _oldValueRequired = listenerConfig._oldValueRequired;
        _synchronous = listenerConfig._synchronous;
    }

    /// <summary>
    /// Gets or sets the name of the cache entry listener factory.
    /// </summary>
    /// <returns></returns>
    public string CacheEntryListenerFactory
    {
        get => _cacheEntryListenerFactory;
        set => _cacheEntryListenerFactory = value;
    }

    /// <summary>
    /// Sets the name of the cache entry listener factory.
    /// </summary>
    /// <param name="cacheEntryListenerFactory">The name of the cache entry listener factory.</param>
    /// <returns>This instance.</returns>
    public CacheSimpleEntryListenerOptions SetCacheEntryListenerFactory(string cacheEntryListenerFactory)
    {
        CacheEntryListenerFactory = cacheEntryListenerFactory;
        return this;
    }

    /// <summary>
    /// Gets or sets the name of the cache entry filter factory.
    /// </summary>
    public string CacheEntryEventFilterFactory
    {
        get => _cacheEntryEventFilterFactory;
        set => _cacheEntryEventFilterFactory = value;
    }

    /// <summary>
    /// Sets the name of the cache entry filter factory.
    /// </summary>
    /// <param name="cacheEntryEventFilterFactory">The name of the cache entry filter factory.</param>
    /// <returns>This instance.</returns>
    public CacheSimpleEntryListenerOptions SetCacheEntryEventFilterFactory(string cacheEntryEventFilterFactory)
    {
        CacheEntryEventFilterFactory = cacheEntryEventFilterFactory;
        return this;
    }

    /// <summary>
    /// Whether the old value is required.
    /// </summary>
    public bool IsOldValueRequired
    {
        get => _oldValueRequired;
        set => _oldValueRequired = value;
    }

    /// <summary>
    /// Sets whether old value is required.
    /// </summary>
    /// <param name="oldValueRequired">Whether old value is required.</param>
    /// <returns>This instance.</returns>
    public CacheSimpleEntryListenerOptions SetIsOldValueRequired(bool oldValueRequired)
    {
        IsOldValueRequired = oldValueRequired;
        return this;
    }

    /// <summary>
    /// Whether this cache entry listener implementation will be called in a synchronous manner.
    /// </summary>
    public bool IsSynchronous
    {
        get => _synchronous;
        set => _synchronous = value;
    }

    /// <summary>
    /// Sets whether this cache entry listener implementation will be called in a synchronous manner.
    /// </summary>
    /// <param name="synchronous">Whether this cache entry listener implementation will be called in a synchronous manner.</param>
    /// <returns>This instance.</returns>
    public CacheSimpleEntryListenerOptions SetIsSynchronous(bool synchronous)
    {
        IsSynchronous = synchronous;
        return this;
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.SimpleCacheEntryListenerConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteString(_cacheEntryEventFilterFactory);
        output.WriteString(_cacheEntryListenerFactory);
        output.WriteBoolean(_oldValueRequired);
        output.WriteBoolean(_synchronous);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _cacheEntryEventFilterFactory = input.ReadString();
        _cacheEntryListenerFactory = input.ReadString();
        _oldValueRequired = input.ReadBoolean();
        _synchronous = input.ReadBoolean();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "CacheSimpleEntryListenerConfig{"
            + "cacheEntryListenerFactory='" + _cacheEntryListenerFactory + '\''
            + ", cacheEntryEventFilterFactory='" + _cacheEntryEventFilterFactory + '\''
            + ", oldValueRequired=" + _oldValueRequired
            + ", synchronous=" + _synchronous
            + '}';
    }
}
