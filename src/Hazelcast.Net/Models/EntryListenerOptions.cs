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
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

/// <summary>
/// Represents the configuration for an entry listener.
/// </summary>
public class EntryListenerOptions : ListenerOptions
{

    private bool _local;
    private bool _includeValue = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryListenerOptions"/> class.
    /// </summary>
    public EntryListenerOptions()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryListenerOptions"/> class.
    /// </summary>
    public EntryListenerOptions(string className, bool local, bool includeValue)
        : base(className)
    {
        _local = local;
        _includeValue = includeValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryListenerOptions"/> class.
    /// </summary>
    public EntryListenerOptions(EntryListenerOptions config)
        : base(config)
    {
        _includeValue = config.IncludeValue;
        _local = config.Local;
    }

    public override bool Local
    {
        get => _local;
        set => _local = value;
    }

    public override bool IncludeValue
    {
        get => _includeValue;
        set => _includeValue = value;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"EntryListenerConfig{{local={_local}, includeValue={_includeValue}}}";

    /// <inheritdoc />
    public override int ClassId => ConfigurationDataSerializerHook.EntryListenerConfig;

    /// <inheritdoc />
    public override void WriteData(IObjectDataOutput output)
    {
        base.WriteData(output);
        output.WriteBoolean(_local);
        output.WriteBoolean(_includeValue);
    }

    /// <inheritdoc />
    public override void ReadData(IObjectDataInput input)
    {
        base.ReadData(input);
        _local = input.ReadBoolean();
        _includeValue = input.ReadBoolean();
    }
}