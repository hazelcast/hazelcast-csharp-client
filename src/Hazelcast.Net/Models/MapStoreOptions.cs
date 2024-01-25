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

using Hazelcast.Serialization;
using System;
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Configuration;

namespace Hazelcast.Models;

/// <summary>
/// Represents options for a map store.
/// </summary>
public class MapStoreOptions : IIdentifiedDataSerializable
{
    /// <summary>
    /// Provides the default options values.
    /// </summary>
#pragma warning disable CA1034
    public static class Defaults
    {
        /// <summary>
        /// Gets the default value for enable.
        /// </summary>
        public const bool Enabled = false;

        /// <summary>
        /// Gets the default delay for writing, in seconds.
        /// </summary>
        public const int WriteDelaySeconds = 0;

        /// <summary>
        /// Gets the default batch size for writing.
        /// </summary>
        public const int WriteBatchSize = 1;

        /// <summary>
        /// Gets the default write coalescing behavior.
        /// </summary>
        public const bool WriteCoalescing = true;

        /// <summary>
        /// Gets the default offload behavior.
        /// </summary>
        public const bool Offload = true;

        /// <summary>
        /// Gets the default initial load mode.
        /// </summary>
        public const LoadMode InitialLoadMode = LoadMode.Lazy;

        /// <summary>
        /// Gets the default class name.
        /// </summary>
        public const string ClassName = "";

        /// <summary>
        /// Gets the default factory class name.
        /// </summary>
        public const string FactoryClassName = "";
    }
#pragma warning restore CA1034

    private bool _enabled = Defaults.Enabled;
    private bool _offload = Defaults.Offload;
    private bool _writeCoalescing = Defaults.WriteCoalescing;
    private int _writeDelaySeconds = Defaults.WriteDelaySeconds;
    private int _writeBatchSize = Defaults.WriteBatchSize;
    private string _className = Defaults.ClassName;
    private string _factoryClassName = Defaults.FactoryClassName;
    private Dictionary<string, string> _properties = new();
    private LoadMode _initialLoadMode = Defaults.InitialLoadMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapStoreOptions"/> class.
    /// </summary>
    public MapStoreOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapStoreOptions"/> class.
    /// </summary>
    public MapStoreOptions(MapStoreOptions config)
    {
        _enabled = config._enabled;
        _className = config._className;
        _factoryClassName = config._factoryClassName;
        _writeDelaySeconds = config._writeDelaySeconds;
        _writeBatchSize = config._writeBatchSize;
        _initialLoadMode = config._initialLoadMode;
        _writeCoalescing = config._writeCoalescing;
        _offload = config._offload;
        foreach (var (key, value) in config._properties)
            _properties.Add(key, value);
    }

    /// <summary>
    /// Gets or sets the name of the implementation class.
    /// </summary>
    public string ClassName
    {
        get => _className;
        set => _className = value.ThrowIfNullNorWhiteSpace();
    }

    /// <summary>
    /// Gets or sets the name of the map store factory implementation class.
    /// </summary>
    public string FactoryClassName
    {
        get => _factoryClassName;
        set => _factoryClassName = value.ThrowIfNullNorWhiteSpace();
    }

    /// <summary>
    /// Gets or sets the number of seconds to delay the store writes.
    /// </summary>
    public int WriteDelaySeconds
    {
        get => _writeDelaySeconds;
        set => _writeDelaySeconds = value;
    }

    /// <summary>
    /// Gets or sets the number of operations to be included in each batch processing round.
    /// </summary>
    public int WriteBatchSize
    {
        get => _writeBatchSize;
        set => _writeBatchSize = value.ThrowIfLessThanOrZero();
    }

    /// <summary>
    /// Whether this configuration is enabled.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// Whether offload behavior is enabled.
    /// </summary>
    public bool Offload
    {
        get => _offload;
        set => _offload = value;
    }

    /// <summary>
    /// Sets a property.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value.</param>
    /// <returns>This instance.</returns>
    public MapStoreOptions SetProperty(string name, string value)
    {
        _properties.Add(name, value);
        return this;
    }

    /// <summary>
    /// Gets a property.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>The property value.</returns>
    public string GetProperty(string name)
    {
        return _properties.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>
    /// Gets or sets the properties.
    /// </summary>
    public Dictionary<string, string> Properties
    {
        get => _properties;
        set => _properties = value.ThrowIfNull();
    }

    /// <summary>
    /// Gets or sets the initial load mode.
    /// </summary>
    public LoadMode InitialLoadMode
    {
        get => _initialLoadMode;
        set => _initialLoadMode = value;
    }

    /// <summary>
    /// Whether write-coalescing is enabled.
    /// </summary>
    public bool WriteCoalescing
    {
        get => _writeCoalescing;
        set => _writeCoalescing = value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "MapStoreConfig{"
                + "enabled=" + _enabled
                + ", className='" + _className + '\''
                + ", factoryClassName='" + _factoryClassName + '\''
                + ", writeDelaySeconds=" + _writeDelaySeconds
                + ", writeBatchSize=" + _writeBatchSize
                + ", properties=" + _properties
                + ", initialLoadMode=" + _initialLoadMode
                + ", writeCoalescing=" + _writeCoalescing
                + ", offload=" + _offload
                + '}';
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.MapStoreConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteBoolean(_enabled);
        output.WriteBoolean(_writeCoalescing);
        output.WriteString(_className);
        output.WriteString(_factoryClassName);
        output.WriteInt(_writeDelaySeconds);
        output.WriteInt(_writeBatchSize);
        output.WriteObject(null/*implementation*/);
        output.WriteObject(null/*factoryImplementation*/);
        output.WriteObject(Properties);
        output.WriteString(_initialLoadMode.ToJavaString());
        output.WriteBoolean(_offload);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _enabled = input.ReadBoolean();
        _writeCoalescing = input.ReadBoolean();
        _className = input.ReadString();
        _factoryClassName = input.ReadString();
        _writeDelaySeconds = input.ReadInt();
        _writeBatchSize = input.ReadInt();
        _/*implementation*/ = input.ReadObject<object>();
        _/*factoryImplementation*/ = input.ReadObject<object>();
        _properties = input.ReadObject<Dictionary<string, string>>();
        _initialLoadMode = Enums.ParseJava<LoadMode>(input.ReadString());
        _offload = input.ReadBoolean();
    }
}