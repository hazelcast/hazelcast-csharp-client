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
using Hazelcast.Configuration;
using Hazelcast.Serialization;
using System;

namespace Hazelcast.Models;

internal class CacheSimpleEntryListenerOptions : IIdentifiedDataSerializable
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
    /// Gets or sets the name of the cache entry filter factory.
    /// </summary>
    public string CacheEntryEventFilterFactory
    {
        get => _cacheEntryEventFilterFactory;
        set => _cacheEntryEventFilterFactory = value;
    }

    /// <summary>
    /// Whether the old value is required.
    /// </summary>
    public bool OldValueRequired
    {
        get => _oldValueRequired;
        set => _oldValueRequired = value;
    }

    /// <summary>
    /// Whether this cache entry listener implementation will be called in a synchronous manner.
    /// </summary>
    public bool Synchronous
    {
        get => _synchronous;
        set => _synchronous = value;
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
