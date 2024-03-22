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

using System.Collections.Generic;
using System.IO;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents the configuration of a ringbuffer store.
/// </summary>
public class RingbufferStoreOptions : IIdentifiedDataSerializable
{
    private bool _enabled = true;
    private string _className;
    private string _factoryClassName;
    private Dictionary<string, string> _properties = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RingbufferStoreOptions"/> class.
    /// </summary>
    public RingbufferStoreOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RingbufferStoreOptions"/> class.
    /// </summary>
    public RingbufferStoreOptions(RingbufferStoreOptions options)
    {
        _enabled = options._enabled;
        _className = options._className;
        _factoryClassName = options._factoryClassName;
        _properties = new Dictionary<string, string>(options._properties);
    }

    /// <summary>
    /// Whether the store is enabled.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    /// <summary>
    /// Gets or sets the store class name.
    /// </summary>
    public string ClassName
    {
        get => _className;
        set => _className = value.ThrowIfNullNorWhiteSpace();
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
    /// Gets or sets the factory class name.
    /// </summary>
    public string FactoryClassName
    {
        get => _factoryClassName;
        set => _factoryClassName = value.ThrowIfNullNorWhiteSpace();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return "RingbufferStoreConfig{"
                + "enabled=" + _enabled
                + ", className='" + _className + '\''
                + ", factoryClassName='" + _factoryClassName + '\''
                + ", properties=" + _properties
                + '}';
    }

    /// <inheritdoc />
    public int FactoryId => ConfigurationDataSerializerHook.FactoryIdConst;

    /// <inheritdoc />
    public int ClassId => ConfigurationDataSerializerHook.RingbufferStoreConfig;

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteBoolean(_enabled);
        output.WriteString(_className);
        output.WriteString(_factoryClassName);
        output.WriteObject(_properties);
        output.WriteObject(null/*_storeImplementation*/);
        output.WriteObject(null/*_factoryImplementation*/);
    }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        _enabled = input.ReadBoolean();
        _className = input.ReadString();
        _factoryClassName = input.ReadString();
        _properties = input.ReadObject<Dictionary<string, string>>();
        _ /*_storeImplementation*/ = input.ReadObject<object>();
        _ /*_factoryImplementation*/ = input.ReadObject<object>();
    }
}
