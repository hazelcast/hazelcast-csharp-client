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

internal class MapStoreConfigHolder
{
    private bool? _isOffload;

    public MapStoreConfigHolder()
    { }

    public MapStoreConfigHolder(bool enabled, bool writeCoalescing, int writeDelaySeconds, int writeBatchSize,
                                string className, IData implementation,
                                string factoryClassName, IData factoryImplementation,
                                Dictionary<string, string> properties,
                                string initialLoadMode, bool isOffloadExists, bool offload)
    {
        IsEnabled = enabled;
        IsWriteCoalescing = writeCoalescing;
        ClassName = className;
        Implementation = implementation;
        FactoryClassName = factoryClassName;
        FactoryImplementation = factoryImplementation;
        WriteDelaySeconds = writeDelaySeconds;
        WriteBatchSize = writeBatchSize;
        Properties = properties;
        InitialLoadMode = initialLoadMode;
        _isOffload = isOffloadExists ? offload : null;
    }

    public bool IsEnabled { get; set; }

    public bool IsOffload
    {
        get => _isOffload ?? false;
        set => _isOffload = value;
    }

    public bool IsWriteCoalescing { get; set; }

    public string ClassName { get; set; }

    public IData Implementation { get; set; }

    public string FactoryClassName { get; set; }

    public IData FactoryImplementation { get; set; }

    public int WriteDelaySeconds { get; set; }

    public int WriteBatchSize { get; set; }

    public Dictionary<string, string> Properties { get; set; }

    public string InitialLoadMode { get; set; }

    public MapStoreOptions ToMapStoreConfig()
    {
        var config = new MapStoreOptions();
        if (!string.IsNullOrEmpty(ClassName))
            config.ClassName = ClassName;
        config.Enabled = IsEnabled;
        if (!string.IsNullOrEmpty(FactoryClassName))
            config.FactoryClassName = FactoryClassName;
        config.InitialLoadMode = Enums.ParseJava<LoadMode>(InitialLoadMode);
        if (Properties != null)
            config.Properties = Properties;
        config.WriteBatchSize = WriteBatchSize;
        config.WriteCoalescing = IsWriteCoalescing;
        config.WriteDelaySeconds = WriteDelaySeconds;
        if (_isOffload.HasValue)
            config.Offload = _isOffload.Value;
        return config;
    }

    public static MapStoreConfigHolder Of(MapStoreOptions config)
    {
        if (config == null)
            return null;

        var holder = new MapStoreConfigHolder
        {
            ClassName = config.ClassName,
            IsEnabled = config.Enabled,
            FactoryClassName = config.FactoryClassName,
            InitialLoadMode = config.InitialLoadMode.ToJavaString(),
            Properties = config.Properties,
            WriteBatchSize = config.WriteBatchSize,
            IsWriteCoalescing = config.WriteCoalescing,
            WriteDelaySeconds = config.WriteDelaySeconds,
            IsOffload = config.Offload
        };
        return holder;
    }
}
